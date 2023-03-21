using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.ChatGPT_API;
using Mochineko.KoeiromapAPI;
using Mochineko.LLMAgent.Chat;
using Mochineko.LLMAgent.Speech;
using Mochineko.Relent.Result;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mochineko.LLMAgent.Operation
{
    internal sealed class DemoOperator : MonoBehaviour
    {
        [SerializeField, TextArea] private string prompt;
        [SerializeField, TextArea] private string message;
        [SerializeField, Range(-3f, 3f)] private float speakerX;
        [SerializeField, Range(-3f, 3f)] private float speakerY;
        [SerializeField] private AudioSource audioSource;

        private ChatCompletion chatCompletion;
        private SpeechSynthesis speechSynthesis;

        private void Awake()
        {
            Assert.IsNotNull(audioSource);
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
                prompt, // PromptTemplate.ChatAgentTemplate,
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
            var chatResult = await chatCompletion.SendChatAsync(message, cancellationToken);
            if (chatResult is ISuccessResult<string> chatSuccess)
            {
                var synthesisResult = await speechSynthesis.SynthesisAsync(
                    chatSuccess.Result,
                    Style.Talk,
                    cancellationToken);
                if (synthesisResult is ISuccessResult<AudioClip> synthesisSuccess)
                {
                    // TODO: Queueing
                    audioSource.PlayOneShot(synthesisSuccess.Result);
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