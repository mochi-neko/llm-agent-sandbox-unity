#nullable enable
using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.ChatGPT_API;
using Mochineko.LLMAgent.Chat;
using Mochineko.LLMAgent.Emotion;
using Mochineko.LLMAgent.Memory;
using Mochineko.LLMAgent.Speech;
using Mochineko.LLMAgent.Summarization;
using Mochineko.Relent.Extensions.NewtonsoftJson;
using Mochineko.Relent.Result;
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
        [SerializeField, Range(-3f, 3f)] private float speakerX;
        [SerializeField, Range(-3f, 3f)] private float speakerY;
        [SerializeField] private SpeechQueue? speechQueue;
        [SerializeField] private bool skipSpeechSynthesis = false;

        private IChatMemoryStore? store;
        private LongTermChatMemory? memory;
        internal LongTermChatMemory? Memory => memory;
        
        private ChatCompletion? chatCompletion;
        private KoeiromapSpeechSynthesis? speechSynthesis;

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

            store = new PlayerPrefsChatMemoryStore();
            
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
                prompt + ". " + PromptTemplate.ChatAgentEmotionAnalysisTemplate,
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

            speechSynthesis = new KoeiromapSpeechSynthesis(
                new Vector2(speakerX, speakerY));
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

            var chatResult = await chatCompletion.CompleteChatAsync(message, cancellationToken);
            if (chatResult is ISuccessResult<string> chatSuccess)
            {
                Debug.Log($"[LLMAgent.Operation] Emotionalized JSON message:{chatSuccess.Result}.");

                var emotionalizationResult = RelentJsonSerializer.Deserialize<EmotionalMessage>(chatSuccess.Result);
                if (emotionalizationResult is ISuccessResult<EmotionalMessage> emotionalizationSuccess)
                {
                    var emotionStyle = EmotionToStyleConverter.ExcludeHighestEmotionStyle(
                        emotionalizationSuccess.Result.Emotion,
                        threshold: 0.5f);
                    
                    Debug.Log($"[LLMAgent.Operation] Exclude emotion style: {emotionStyle}.");

                    if (skipSpeechSynthesis)
                    {
                        return;
                    }
                    
                    var synthesisResult = await speechSynthesis.SynthesisSpeechAsync(
                        HttpClientPool.PooledClient,
                        emotionalizationSuccess.Result.Message,
                        emotionStyle,
                        cancellationToken);
                    
                    if (synthesisResult is ISuccessResult<AudioClip> synthesisSuccess)
                    {
                        speechQueue.Enqueue(synthesisSuccess.Result);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
    }
}