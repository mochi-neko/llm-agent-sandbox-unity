#nullable enable
using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.ChatGPT_API;
using Mochineko.FacialExpressions.Blink;
using Mochineko.FacialExpressions.Emotion;
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
        [SerializeField] private RuntimeAnimatorController? animatorController = null;

        private IChatMemoryStore? store;
        private LongTermChatMemory? memory;
        internal LongTermChatMemory? Memory => memory;
        private ChatCompletion? chatCompletion;
        private ChatCompletion? stateCompletion;
        private VoiceVoxSpeechSynthesis? speechSynthesis;
        private IFiniteStateMachine<AgentEvent, AgentContext>? agentStateMachine;

        private async void Start()
        {
            await SetupAgentAsync(this.GetCancellationTokenOnDestroy());
        }

        private async void Update()
        {
            if (agentStateMachine == null)
            {
                return;
            }

            var updateResult = await agentStateMachine
                .UpdateAsync(this.GetCancellationTokenOnDestroy());
            if (updateResult is IFailureResult updateFailure)
            {
                Debug.LogError($"Failed to update agent state machine because -> {updateFailure.Message}.");
            }
        }

        private void OnDestroy()
        {
            agentStateMachine?.Dispose();
        }

        private async UniTask SetupAgentAsync(CancellationToken cancellationToken)
        {
            if (audioSource == null)
            {
                throw new NullReferenceException(nameof(audioSource));
            }

            if (animatorController == null)
            {
                throw new NullReferenceException(nameof(animatorController));
            }

            var apiKeyPath = Path.Combine(
                Application.dataPath,
                "Mochineko/LLMAgent/Operation/OpenAI_API_Key.txt");

            var apiKey = await File.ReadAllTextAsync(apiKeyPath, cancellationToken);
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
                cancellationToken);

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
                            cancellationToken);
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
                cancellationToken);

            var instance = await LoadVRMAsync(
                binary,
                cancellationToken);

            var lipMorpher = new VRMLipMorpher(instance.Runtime.Expression);
            var lipAnimator = new FollowingLipAnimator(lipMorpher);

            var emotionMorpher = new VRMEmotionMorpher(instance.Runtime.Expression);
            var emotionAnimator = new ExclusiveFollowingEmotionAnimator<FacialExpressions.Emotion.Emotion>(
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
                eyelidAnimationFrames,
                lipMorpher,
                lipAnimator,
                audioSource,
                emotionAnimator);

            agentStateMachine = await AgentStateMachineFactory.CreateAsync(
                agentContext,
                cancellationToken);

            instance
                .GetComponent<Animator>()
                .runtimeAnimatorController = animatorController;
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

        [ContextMenu(nameof(StartChatAsync))]
        public void StartChatAsync()
        {
            ChatAsync(message, this.GetCancellationTokenOnDestroy())
                .Forget();
        }

        public async UniTask ChatAsync(string message, CancellationToken cancellationToken)
        {
            if (chatCompletion == null)
            {
                throw new NullReferenceException(nameof(chatCompletion));
            }

            if (speechSynthesis == null)
            {
                throw new NullReferenceException(nameof(speechSynthesis));
            }

            if (agentStateMachine == null)
            {
                throw new NullReferenceException(nameof(agentStateMachine));
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
                emotionalMessage.Message,
                cancellationToken);

            switch (synthesisResult)
            {
                case ISuccessResult<(AudioQuery query, AudioClip clip)> synthesisSuccess:
                {
                    agentStateMachine.Context.SpeechQueue.Enqueue(new SpeechCommand(
                        synthesisSuccess.Result.query,
                        synthesisSuccess.Result.clip,
                        new EmotionSample<FacialExpressions.Emotion.Emotion>(emotion, emotionWeight)));

                    if (agentStateMachine.IsCurrentState<AgentIdleState>())
                    {
                        var sendEventResult = await agentStateMachine.SendEventAsync(
                            AgentEvent.BeginSpeaking,
                            cancellationToken);
                        if (sendEventResult is IFailureResult sendEventFailure)
                        {
                            Debug.LogError(
                                $"[LLMAgent.Operation] Failed to send event because of {sendEventFailure.Message}.");
                        }
                    }

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
    }
}