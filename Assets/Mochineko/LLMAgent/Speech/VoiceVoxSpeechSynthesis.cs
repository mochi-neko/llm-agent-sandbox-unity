#nullable enable
using System.IO;
using System.Net.Http;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Resilience;
using Mochineko.Relent.Result;
using Mochineko.Relent.UncertainResult;
using Mochineko.VOICEVOX_API.QueryCreation;
using Mochineko.VOICEVOX_API.Synthesis;
using UnityEngine;

namespace Mochineko.LLMAgent.Speech
{
    public sealed class VoiceVoxSpeechSynthesis
    {
        private readonly int speakerID;
        private readonly IPolicy<AudioQuery> queryPolicy;
        private readonly IPolicy<Stream> synthesisPolicy;

        public VoiceVoxSpeechSynthesis(
            int speakerID)
        {
            this.speakerID = speakerID;
            this.queryPolicy = PolicyFactory.BuildQueryPolicy();
            this.synthesisPolicy = PolicyFactory.BuildSynthesisPolicy();
        }

        public async UniTask<IResult<(AudioQuery query, AudioClip clip)>> SynthesisSpeechAsync(
            HttpClient httpClient,
            string text,
            CancellationToken cancellationToken)
        {
            Debug.Log($"[LLMAgent.Speech] Begin to synthesis speech from text:{text}.");

            await UniTask.SwitchToThreadPool();

            AudioQuery audioQuery;
            var createAudioQueryResult = await queryPolicy.ExecuteAsync(
                async innerCancellationToken => await QueryCreationAPI.CreateQueryAsync(
                    httpClient,
                    text,
                    speakerID,
                    coreVersion: null,
                    innerCancellationToken
                ),
                cancellationToken);
            switch (createAudioQueryResult)
            {
                case IUncertainSuccessResult<AudioQuery> createAudioQuerySuccess:
                    audioQuery = createAudioQuerySuccess.Result;
                    break;

                case IUncertainRetryableResult<AudioQuery> createAudioQueryRetryable:
                    Debug.LogError(
                        $"[LLMAgent.Speech] Failed to create audio query because -> {createAudioQueryRetryable.Message}.");
                    return Results.FailWithTrace<(AudioQuery, AudioClip)>(
                        $"Failed to create audio query because -> {createAudioQueryRetryable.Message}.");

                case IUncertainFailureResult<AudioQuery> createAudioQueryFailure:
                    Debug.LogError(
                        $"[LLMAgent.Speech] Failed to create audio query because -> {createAudioQueryFailure.Message}.");
                    return Results.FailWithTrace<(AudioQuery, AudioClip)>(
                        $"Failed to create audio query because -> {createAudioQueryFailure.Message}.");

                default:
                    throw new UncertainResultPatternMatchException(nameof(createAudioQueryResult));
            }

            var synthesisResult = await synthesisPolicy.ExecuteAsync(
                async innerCancellationToken => await SynthesisAPI.SynthesizeAsync(
                    httpClient,
                    audioQuery,
                    speakerID,
                    enableInterrogativeUpspeak: null,
                    coreVersion:
                    null,
                    innerCancellationToken),
                cancellationToken);

            await UniTask.SwitchToMainThread(cancellationToken);

            switch (synthesisResult)
            {
                case IUncertainSuccessResult<Stream> success:
                {
                    Debug.Log($"[LLMAgent.Speech] Begin to decode audio:{text}.");

                    var decodeResult = await AudioDecoder.DecodeAsync(
                        success.Result,
                        fileName: "VoiceVoxSynthesized.wav",
                        cancellationToken);

                    if (decodeResult is ISuccessResult<AudioClip> decodeSuccess)
                    {
                        Debug.Log($"[LLMAgent.Speech] Succeeded to synthesis speech from text:{text}.");
                        return Results.Succeed((audioQuery,decodeSuccess.Result));
                    }
                    else if (decodeResult is IFailureResult<AudioClip> decodeFailure)
                    {
                        Debug.LogError(
                            $"[LLMAgent.Speech] Failed to decode audio stream because -> {decodeFailure.Message}.");
                        return Results.FailWithTrace<(AudioQuery, AudioClip)>(
                            $"Failed to decode audio stream because -> {decodeFailure.Message}.");
                    }
                    else
                    {
                        throw new ResultPatternMatchException(nameof(decodeResult));
                    }
                }

                case IUncertainRetryableResult<Stream> retryable:
                    Debug.LogError(
                        $"[LLMAgent.Speech] Failed to synthesis speech from text because -> {retryable.Message}.");
                    return Results.FailWithTrace<(AudioQuery, AudioClip)>(
                        $"Failed to synthesis speech from text because -> {retryable.Message}.");

                case IUncertainFailureResult<Stream> failure:
                    Debug.LogError(
                        $"[LLMAgent.Speech] Failed to synthesis speech from text because -> {failure.Message}.");
                    return Results.FailWithTrace<(AudioQuery, AudioClip)>(
                        $"Failed to synthesis speech from text because -> {failure.Message}.");

                default:
                    throw new UncertainResultPatternMatchException(nameof(synthesisResult));
            }
        }
    }
}