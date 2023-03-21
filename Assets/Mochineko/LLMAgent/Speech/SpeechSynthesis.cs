#nullable enable
using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.KoeiromapAPI;
using Mochineko.Relent.Resilience;
using Mochineko.Relent.Result;
using Mochineko.Relent.UncertainResult;
using Mochineko.SimpleAudioCodec;
using UnityEngine;

namespace Mochineko.LLMAgent.Speech
{
    public class SpeechSynthesis
    {
        private readonly Vector2 speaker;
        private readonly int? seed;
        private readonly IPolicy<Stream> policy;

        public SpeechSynthesis(
            Vector2 speaker,
            int? seed = null)
        {
            this.speaker = speaker;
            this.seed = seed;
            this.policy = PolicyFactory.BuildPolicy();
        }

        public async UniTask<IResult<AudioClip>> SynthesisAsync(
            string text,
            Style style,
            CancellationToken cancellationToken)
        {
            await UniTask.SwitchToThreadPool();

            var synthesisResult = await policy.ExecuteAsync(
                async innerCancellationToken => await SpeechSynthesisAPI.SynthesisAsync(
                    HttpClientPool.PooledClient, // TODO:
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
                try
                {
                    var audioClip = await WaveDecoder.DecodeByBlockAsync(
                        success.Result,
                        fileName: "KoeiromapSynthesized.wav",
                        cancellationToken);

                    Debug.Log($"Succeeded to synthesis speech from text:{text}.");
                    return ResultFactory.Succeed(audioClip);
                }
                catch (Exception exception)
                {
                    return ResultFactory.Fail<AudioClip>(
                        $"Failed to decode audio stream because -> {exception}.");
                }
            }
            else if (synthesisResult is IUncertainRetryableResult<Stream> retryable)
            {
                Debug.LogError($"Failed to synthesis speech from text because -> {retryable.Message}.");
                return ResultFactory.Fail<AudioClip>(
                    $"Failed to synthesis speech from text because -> {retryable.Message}.");
            }
            else if (synthesisResult is IUncertainFailureResult<Stream> failure)
            {
                Debug.LogError($"Failed to synthesis speech from text because -> {failure.Message}.");
                return ResultFactory.Fail<AudioClip>(
                    $"Failed to synthesis speech from text because -> {failure.Message}.");
            }
            else
            {
                throw new UncertainResultPatternMatchException(nameof(synthesisResult));
            }
        }
    }
}