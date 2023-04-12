#nullable enable
using System.IO;
using System.Net.Http;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.KoeiromapAPI;
using Mochineko.Relent.Resilience;
using Mochineko.Relent.Result;
using Mochineko.Relent.UncertainResult;
using UnityEngine;

namespace Mochineko.LLMAgent.Speech
{
    public sealed class KoeiromapSpeechSynthesis
    {
        private readonly Vector2 speaker;
        private readonly ulong? seed;
        private readonly IPolicy<Stream> policy;

        public KoeiromapSpeechSynthesis(
            Vector2 speaker,
            ulong? seed = null)
        {
            this.speaker = speaker;
            this.seed = seed;
            this.policy = PolicyFactory.BuildSynthesisPolicy();
        }

        public async UniTask<IResult<AudioClip>> SynthesisSpeechAsync(
            HttpClient httpClient,
            string text,
            Style style,
            CancellationToken cancellationToken)
        {
            Debug.Log($"[LLMAgent.Speech] Begin to synthesis speech from text:{text}.");
            
            await UniTask.SwitchToThreadPool();

            var synthesisResult = await policy.ExecuteAsync(
                async innerCancellationToken => await SpeechSynthesisAPI.SynthesisAsync(
                    httpClient,
                    text,
                    innerCancellationToken,
                    speakerX: speaker.x,
                    speakerY: speaker.y,
                    style: style,
                    seed: seed),
                cancellationToken);

            await UniTask.SwitchToMainThread(cancellationToken);

            if (synthesisResult is IUncertainSuccessResult<Stream> success)
            {
                Debug.Log($"[LLMAgent.Speech] Begin to decode audio:{text}.");

                var decodeResult = await AudioDecoder.DecodeAsync(
                    success.Result,
                    fileName: "KoeiromapSynthesized.wav",
                    cancellationToken);

                if (decodeResult is ISuccessResult<AudioClip> decodeSuccess)
                {
                    Debug.Log($"[LLMAgent.Speech] Succeeded to synthesis speech from text:{text}.");
                    return Results.Succeed(decodeSuccess.Result);
                }
                else if (decodeResult is IFailureResult<AudioClip> decodeFailure)
                {
                    Debug.LogError($"[LLMAgent.Speech] Failed to decode audio stream because -> {decodeFailure.Message}.");
                    return Results.FailWithTrace<AudioClip>(
                        $"Failed to decode audio stream because -> {decodeFailure.Message}.");
                }
                else
                {
                    throw new ResultPatternMatchException(nameof(decodeResult));
                }
            }
            else if (synthesisResult is IUncertainRetryableResult<Stream> retryable)
            {
                Debug.LogError($"[LLMAgent.Speech] Failed to synthesis speech from text because -> {retryable.Message}.");
                return Results.FailWithTrace<AudioClip>(
                    $"Failed to synthesis speech from text because -> {retryable.Message}.");
            }
            else if (synthesisResult is IUncertainFailureResult<Stream> failure)
            {
                Debug.LogError($"[LLMAgent.Speech] Failed to synthesis speech from text because -> {failure.Message}.");
                return Results.FailWithTrace<AudioClip>(
                    $"Failed to synthesis speech from text because -> {failure.Message}.");
            }
            else
            {
                throw new UncertainResultPatternMatchException(nameof(synthesisResult));
            }
        }
    }
}