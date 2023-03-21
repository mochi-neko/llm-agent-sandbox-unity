using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Resilience;
using Mochineko.Relent.UncertainResult;
using Mochineko.SimpleAudioCodec;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mochineko.KoeiromapAPI.Samples
{
    internal sealed class KoeiromapAPISample : MonoBehaviour
    {
        [SerializeField, Range(-3f, 3f)] private float speakerX;
        [SerializeField, Range(-3f, 3f)] private float speakerY;
        [SerializeField] private bool useSeed;
        [SerializeField] private ulong seed;
        [SerializeField, TextArea] private string text;
        [SerializeField] private Style style;
        [SerializeField] private AudioSource audioSource;

        private static readonly HttpClient httpClient = new();
        
        private IPolicy<Stream> policy;

        private void Awake()
        {
            Assert.IsNotNull(audioSource);
            
            policy = PolicyFactory.BuildPolicy();
        }

        [ContextMenu(nameof(Synthesis))]
        public void Synthesis()
        {
            SynthesisAsync(text, style, this.GetCancellationTokenOnDestroy())
                .Forget();
        }
        
        private async UniTask SynthesisAsync(
            string text,
            Style style,
            CancellationToken cancellationToken)
        {
            Debug.Log($"Begin to synthesis speech from text:{text}.");
            
            await UniTask.SwitchToThreadPool();

            var synthesisResult = await policy.ExecuteAsync(
                async innerCancellationToken => await SpeechSynthesisAPI.SynthesisAsync(
                    httpClient,
                    text,
                    innerCancellationToken,
                    speakerX: speakerX,
                    speakerY: speakerY,
                    style: style,
                    seed: useSeed ? seed : null),
                cancellationToken);

            if (synthesisResult is IUncertainSuccessResult<Stream> success)
            {
                await using (success.Result)
                {
                    try
                    {
                        var audioClip = await WaveDecoder.DecodeByBlockAsync(
                            success.Result,
                            fileName: "KoeiromapSynthesized.wav",
                            cancellationToken);

                        await UniTask.SwitchToMainThread(cancellationToken);

                        Debug.Log($"Succeeded to synthesis speech from text:{text}.");

                        audioSource.PlayOneShot(audioClip);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError($"Failed to decode audio stream because -> {exception}.");
                    }
                }
            }
            else if (synthesisResult is IUncertainRetryableResult<Stream> retryable)
            {
                Debug.LogError($"Failed to synthesis speech from text because -> {retryable.Message}.");
            }
            else if (synthesisResult is IUncertainFailureResult<Stream> failure)
            {
                Debug.LogError($"Failed to synthesis speech from text because -> {failure.Message}.");
            }
            else
            {
                throw new UncertainResultPatternMatchException(nameof(synthesisResult));
            }
        }
    }
}
