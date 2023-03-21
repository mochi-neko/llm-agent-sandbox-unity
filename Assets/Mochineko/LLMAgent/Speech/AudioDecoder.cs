#nullable enable
using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;
using Mochineko.SimpleAudioCodec;
using UnityEngine;

namespace Mochineko.LLMAgent.Speech
{
    internal static class AudioDecoder
    {
        public static async UniTask<IResult<AudioClip>> DecodeAsync(
            Stream stream,
            string fileName,
            CancellationToken cancellationToken)
        {
            try
            {
                var audioClip = await WaveDecoder.DecodeByBlockAsync(
                    stream,
                    fileName: fileName,
                    cancellationToken);

                return ResultFactory.Succeed(audioClip);
            }
            catch (Exception exception)
            {
                return ResultFactory.Fail<AudioClip>(
                    $"Failed to decode audio stream because -> {exception}.");
            }
        }
    }
}