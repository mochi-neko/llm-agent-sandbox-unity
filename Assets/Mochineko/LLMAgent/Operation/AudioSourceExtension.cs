#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Mochineko.LLMAgent.Operation
{
    internal static class AudioSourceExtension
    {
        public static async UniTask PlayAsync(
            this AudioSource audioSource,
            AudioClip audioClip,
            CancellationToken cancellationToken)
        {
            audioSource.Stop();
            audioSource.clip = audioClip;
            audioSource.Play();

            await UniTask.Delay(
                TimeSpan.FromSeconds(audioClip.length),
                cancellationToken: cancellationToken);
            
            audioSource.Stop();
        }
    }
}