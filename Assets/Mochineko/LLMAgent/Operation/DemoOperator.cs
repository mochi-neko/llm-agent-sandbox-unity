#nullable enable
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.ChatGPT_API;
using Mochineko.FacialExpressions.Blink;
using Mochineko.FacialExpressions.Emotion;
using Mochineko.FacialExpressions.Extensions.VOICEVOX;
using Mochineko.FacialExpressions.Extensions.VRM;
using Mochineko.FacialExpressions.LipSync;
using Mochineko.FacialExpressions.Samples;
using Mochineko.LLMAgent.Chat;
using Mochineko.LLMAgent.Emotion;
using Mochineko.LLMAgent.Memory;
using Mochineko.LLMAgent.Speech;
using Mochineko.LLMAgent.Summarization;
using Mochineko.Relent.Extensions.NewtonsoftJson;
using Mochineko.Relent.Result;
using Mochineko.RelentStateMachine;
using Mochineko.VOICEVOX_API.QueryCreation;
using UnityEngine;
using UnityEngine.Assertions;
using UniVRM10;
using VRMShaders;

namespace Mochineko.LLMAgent.Operation
{
    internal sealed class DemoOperator : MonoBehaviour
    {
        [SerializeField] private Model model = Model.Turbo;
        [SerializeField, TextArea] private string prompt = string.Empty;
        [SerializeField, TextArea] private string defaultConversations = string.Empty;
        [SerializeField, TextArea] private string message = string.Empty;
        [SerializeField] private int speakerID;
        [SerializeField] private string vrmAvatarPath = string.Empty;
        [SerializeField] private float emotionFollowingTime = 1f;
        [SerializeField] private float emotionWeight = 1f;
        [SerializeField] private AudioSource? audioSource = null;

        private readonly ConcurrentQueue<SpeechCommand> speechQueue = new();

        private AudioClip? currentClip;

        private IChatMemoryStore? store;
        private LongTermChatMemory? memory;
        internal LongTermChatMemory? Memory => memory;
        private ChatCompletion? chatCompletion;
        private ChatCompletion? stateCompletion;
        private VoiceVoxSpeechSynthesis? speechSynthesis;
        private IFiniteStateMachine<AgentEvent, AgentContext>? stateMachine;

        private ILipMorpher? lipMorpher;
        private ILipAnimator? lipAnimator;
        private IEmotionMorpher<FacialExpressions.Emotion.Emotion>? emotionMorpher;
        private ExclusiveFollowingEmotionAnimator<FacialExpressions.Emotion.Emotion>? emotionAnimator;

        private void Awake()
        {
            Assert.IsNotNull(audioSource);
        }

        private async void Start()
        {
            var apiKeyPath = Path.Combine(
                Application.dataPath,
                "Mochineko/LLMAgent/Operation/OpenAI_API_Key.txt");

            var apiKey = await File.ReadAllTextAsync(apiKeyPath);
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception($"[LLMAgent.Operation] Loaded API Key is empty from path:{apiKeyPath}");
            }

            store = new NullChatMemoryStore();

            memory = await LongTermChatMemory.InstantiateAsync(
                maxShortTermMemoriesTokenLength: 1000,
                maxBufferMemoriesTokenLength: 1000,
                apiKey,
                model,
                store,
                this.GetCancellationTokenOnDestroy());

            chatCompletion = new ChatCompletion(
                apiKey,
                model,
                prompt + PromptTemplate.MessageResponseWithEmotion,
                memory);

            if (!string.IsNullOrEmpty(defaultConversations))
            {
                var conversationsDeserializeResult = RelentJsonSerializer
                    .Deserialize<ConversationCollection>(defaultConversations);
                if (conversationsDeserializeResult is ISuccessResult<ConversationCollection> successResult)
                {
                    for (var i = 0; i < successResult.Result.Conversations.Count; i++)
                    {
                        await memory.AddMessageAsync(
                            successResult.Result.Conversations[i],
                            this.GetCancellationTokenOnDestroy());
                    }
                }
                else if (conversationsDeserializeResult is IFailureResult<ConversationCollection> failureResult)
                {
                    Debug.LogError(
                        $"[LLMAgent.Operation] Failed to deserialize default conversations because -> {failureResult.Message}");
                }
            }

            speechSynthesis = new VoiceVoxSpeechSynthesis(speakerID);

            var binary = await File.ReadAllBytesAsync(
                vrmAvatarPath,
                this.GetCancellationTokenOnDestroy());

            var instance = await LoadVRMAsync(
                binary,
                this.GetCancellationTokenOnDestroy());

            lipMorpher = new VRMLipMorpher(instance.Runtime.Expression);
            lipAnimator = new FollowingLipAnimator(lipMorpher);

            emotionMorpher = new VRMEmotionMorpher(instance.Runtime.Expression);
            emotionAnimator = new ExclusiveFollowingEmotionAnimator<FacialExpressions.Emotion.Emotion>(
                emotionMorpher,
                followingTime: emotionFollowingTime);

            var eyelidMorpher = new VRMEyelidMorpher(instance.Runtime.Expression);
            var eyelidAnimator = new SequentialEyelidAnimator(eyelidMorpher);

            var eyelidAnimationFrames =
                ProbabilisticEyelidAnimationGenerator.Generate(
                    Eyelid.Both,
                    blinkCount: 20);

            var agentContext = new AgentContext(
                eyelidAnimator,
                eyelidAnimationFrames);

            stateMachine = await AgentStateMachineFactory.CreateAsync(
                agentContext,
                this.GetCancellationTokenOnDestroy());
        }

        private async void Update()
        {
            if (stateMachine == null)
            {
                return;
            }
            
            emotionAnimator?.Update();
            
            var updateResult = await stateMachine
                .UpdateAsync(this.GetCancellationTokenOnDestroy());
            if (updateResult is IFailureResult updateFailure)
            {
                Debug.LogError($"Failed to update agent state machine because -> {updateFailure.Message}.");
                return;
            }

            if (audioSource == null)
            {
                throw new NullReferenceException(nameof(audioSource));
            }

            if (audioSource.isPlaying)
            {
                return;
            }

            if (speechQueue.TryDequeue(out var command))
            {
                BeginSpeechAsync(command, this.GetCancellationTokenOnDestroy())
                    .Forget();
            }
        }

        private void OnDestroy()
        {
            stateMachine?.Dispose();
            if (currentClip != null)
            {
                Destroy(currentClip);
                currentClip = null;
            }
        }

        // ReSharper disable once InconsistentNaming
        private static async UniTask<Vrm10Instance> LoadVRMAsync(
            byte[] binaryData,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await Vrm10.LoadBytesAsync(
                bytes: binaryData,
                canLoadVrm0X: true,
                controlRigGenerationOption: ControlRigGenerationOption.Generate,
                showMeshes: true,
                awaitCaller: new RuntimeOnlyAwaitCaller(),
                ct: cancellationToken
            );
        }

        public async UniTask AnimateLipAsync(
            AudioQuery audioQuery,
            CancellationToken cancellationToken)
        {
            if (lipAnimator == null)
            {
                return;
            }

            var lipFrames = AudioQueryConverter
                .ConvertToSequentialAnimationFrames(audioQuery);

            await lipAnimator
                .AnimateAsync(lipFrames, cancellationToken);
        }

        [ContextMenu(nameof(StartChatAsync))]
        public void StartChatAsync()
        {
            ChatAsync(message, this.GetCancellationTokenOnDestroy())
                .Forget();
        }

        private async UniTask ChatAsync(string message, CancellationToken cancellationToken)
        {
            if (chatCompletion == null)
            {
                throw new NullReferenceException(nameof(chatCompletion));
            }

            if (speechSynthesis == null)
            {
                throw new NullReferenceException(nameof(speechSynthesis));
            }

            string chatResponse;
            var chatResult = await chatCompletion.CompleteChatAsync(message, cancellationToken);
            switch (chatResult)
            {
                case ISuccessResult<string> chatSuccess:
                {
                    Debug.Log($"[LLMAgent.Operation] Complete chat message:{chatSuccess.Result}.");
                    chatResponse = chatSuccess.Result;
                    break;
                }

                case IFailureResult<string> chatFailure:
                {
                    Debug.LogError($"[LLMAgent.Operation] Failed to complete chat because of {chatFailure.Message}.");
                    return;
                }

                default:
                    throw new ResultPatternMatchException(nameof(chatResult));
            }

            EmotionalMessage emotionalMessage;
            var deserializeResult = RelentJsonSerializer.Deserialize<EmotionalMessage>(chatResponse);
            switch (deserializeResult)
            {
                case ISuccessResult<EmotionalMessage> deserializeSuccess:
                {
                    emotionalMessage = deserializeSuccess.Result;
                    break;
                }

                case IFailureResult<EmotionalMessage> deserializeFailure:
                {
                    Debug.LogError(
                        $"[LLMAgent.Operation] Failed to deserialize emotional message:{chatResponse} because of {deserializeFailure.Message}.");
                    return;
                }

                default:
                    throw new ResultPatternMatchException(nameof(deserializeResult));
            }

            var emotion = EmotionConverter.ExcludeHighestEmotion(emotionalMessage.Emotion);
            Debug.Log($"[LLMAgent.Operation] Exclude emotion:{emotion}.");

            var synthesisResult = await speechSynthesis.SynthesisSpeechAsync(
                HttpClientPool.PooledClient,
                chatResponse,
                cancellationToken);

            switch (synthesisResult)
            {
                case ISuccessResult<(AudioQuery query, AudioClip clip)> synthesisSuccess:
                {
                    speechQueue.Enqueue(new SpeechCommand(
                        synthesisSuccess.Result.query,
                        synthesisSuccess.Result.clip,
                        new EmotionSample<FacialExpressions.Emotion.Emotion>(emotion, emotionWeight)));
                    break;
                }

                case IFailureResult<(AudioQuery query, AudioClip clip)> synthesisFailure:
                {
                    Debug.Log(
                        $"[LLMAgent.Operation] Failed to synthesis speech because of {synthesisFailure.Message}.");
                    return;
                }

                default:
                    return;
            }
        }

        private async UniTask BeginSpeechAsync(
            SpeechCommand command,
            CancellationToken cancellationToken)
        {
            if (audioSource == null)
            {
                throw new NullReferenceException(nameof(audioSource));
            }

            if (lipAnimator == null)
            {
                throw new NullReferenceException(nameof(lipAnimator));
            }

            if (currentClip != null)
            {
                Destroy(currentClip);
            }

            currentClip = command.AudioClip;
            audioSource.clip = currentClip;
            audioSource.Play();

            emotionAnimator?.Emote(command.Emotion);

            var lipFrames = AudioQueryConverter
                .ConvertToSequentialAnimationFrames(command.AudioQuery);

            await lipAnimator.AnimateAsync(
                lipFrames,
                cancellationToken);

            emotionMorpher?.Reset();
            lipMorpher?.Reset();
        }
    }
}