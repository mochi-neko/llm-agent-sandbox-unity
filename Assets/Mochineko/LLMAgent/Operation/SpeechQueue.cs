#nullable enable
using System;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using Mochineko.FacialExpressions.Samples;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mochineko.LLMAgent.Operation
{
    internal sealed class SpeechQueue : MonoBehaviour
    {
        [SerializeField] private AudioSource? audioSource = null;
        [SerializeField] private CharacterOperator? characterOperator = null;

        private readonly ConcurrentQueue<SpeechCommand> queue = new();

        private AudioClip? currentClip;

        private void Awake()
        {
            Assert.IsNotNull(audioSource);
            if (audioSource == null)
            {
                throw new NullReferenceException(nameof(audioSource));
            }

            audioSource.loop = false;
        }

        private void OnDestroy()
        {
            if (currentClip != null)
            {
                Destroy(currentClip);
                currentClip = null;
            }
        }

        private void Update()
        {
            if (audioSource == null)
            {
                throw new NullReferenceException(nameof(audioSource));
            }

            if (audioSource.isPlaying)
            {
                return;
            }

            if (queue.TryDequeue(out var command))
            {
                BeginSpeechAsync(command)
                    .Forget();
            }
        }

        private async UniTask BeginSpeechAsync(SpeechCommand command)
        {
            if (audioSource == null)
            {
                throw new NullReferenceException(nameof(audioSource));
            }

            if (currentClip != null)
            {
                Destroy(currentClip);
            }

            currentClip = command.AudioClip;
            audioSource.clip = currentClip;
            audioSource.Play();

            if (characterOperator != null)
            {
                characterOperator.Emote(command.Emotion);
                await characterOperator.AnimateLipAsync(
                    command.AudioQuery,
                    this.GetCancellationTokenOnDestroy());

                characterOperator.ResetEmotion();
                characterOperator.ResetLip();
            }
        }

        public void Enqueue(SpeechCommand command)
        {
            queue.Enqueue(command);
        }
    }
}