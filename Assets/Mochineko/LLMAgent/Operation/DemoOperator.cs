#nullable enable
using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.ChatGPT_API;
using Mochineko.KoeiromapAPI;
using Mochineko.LLMAgent.Chat;
using Mochineko.LLMAgent.Emotion;
using Mochineko.LLMAgent.Speech;
using Mochineko.Relent.Result;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mochineko.LLMAgent.Operation
{
    internal sealed class DemoOperator : MonoBehaviour
    {
        [SerializeField, TextArea] private string prompt = string.Empty;
        [SerializeField, TextArea] private string message = string.Empty;
        [SerializeField, Range(-3f, 3f)] private float speakerX;
        [SerializeField, Range(-3f, 3f)] private float speakerY;
        [SerializeField] private SpeechQueue? speechQueue;

        private ChatCompletion? chatCompletion;
        private SpeechSynthesis? speechSynthesis;

        private void Awake()
        {
            Assert.IsNotNull(speechQueue);
        }

        private void Start()
        {
            var apiKeyPath = Path.Combine(
                Application.dataPath,
                "Mochineko/LLMAgent/Operation/OpenAI_API_Key.txt");

            string apiKey = File.ReadAllText(apiKeyPath);

            chatCompletion = new ChatCompletion(
                apiKey,
                Model.Turbo,
                prompt + ". " + PromptTemplate.ChatAgentEmotionAnalysisTemplate,
                50);

            speechSynthesis = new SpeechSynthesis(
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