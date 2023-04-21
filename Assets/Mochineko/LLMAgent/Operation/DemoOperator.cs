#nullable enable
using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.ChatGPT_API;
using Mochineko.FacialExpressions.Emotion;
using Mochineko.LLMAgent.Chat;
using Mochineko.LLMAgent.Emotion;
using Mochineko.LLMAgent.Memory;
using Mochineko.LLMAgent.Speech;
using Mochineko.LLMAgent.Summarization;
using Mochineko.Relent.Extensions.NewtonsoftJson;
using Mochineko.Relent.Result;
using Mochineko.VOICEVOX_API.QueryCreation;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mochineko.LLMAgent.Operation
{
    internal sealed class DemoOperator : MonoBehaviour
    {
        [SerializeField] private Model model = Model.Turbo;
        [SerializeField, TextArea] private string prompt = string.Empty;
        [SerializeField, TextArea] private string defaultConversations = string.Empty;
        [SerializeField, TextArea] private string message = string.Empty;
        [SerializeField] private int speakerID;
        [SerializeField] private SpeechQueue? speechQueue = null;
        [SerializeField] private float emotionWeight = 1f;

        private IChatMemoryStore? store;
        private LongTermChatMemory? memory;
        internal LongTermChatMemory? Memory => memory;
        private ChatCompletion? chatCompletion;
        private ChatCompletion? stateCompletion;
        private VoiceVoxSpeechSynthesis? speechSynthesis;

        private void Awake()
        {
            Assert.IsNotNull(speechQueue);
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

            if (speechQueue == null)
            {
                throw new NullReferenceException(nameof(speechQueue));
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
    }
}