#nullable enable
using System;
using System.Collections.Concurrent;
using Cysharp.Threading.Tasks;
using Mochineko.FacialExpressions.Emotion;
using Mochineko.FacialExpressions.Samples;
using Mochineko.VOICEVOX_API.QueryCreation;
using UnityEngine;
using UnityEngine.Assertions;

namespace Mochineko.LLMAgent.Operation
{
    public sealed class SpeechQueue : MonoBehaviour
    {
        [SerializeField] private AudioSource? audioSource = null;
        [SerializeField] private CharacterOperator? characterOperator = null;

        private readonly ConcurrentQueue<(
                AudioQuery query,
                AudioClip clip,
                EmotionSample<FacialExpressions.Emotion.Emotion> emotion)>
            queue = new();

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
                BeginSpeechAsync(command.query, command.clip, command.emotion)
                    .Forget();
            }
        }

        private async UniTask BeginSpeechAsync(
            AudioQuery query,
            AudioClip clip,
            EmotionSample<FacialExpressions.Emotion.Emotion> emotion)
        {
            if (audioSource == null)
            {
                throw new NullReferenceException(nameof(audioSource));
            }

            if (currentClip != null)
            {
                Destroy(currentClip);
            }

            currentClip = clip;
            audioSource.clip = currentClip;
            audioSource.Play();

            if (characterOperator != null)
            {
                characterOperator.Emote(emotion);
                await characterOperator.AnimateLipAsync(
                        query,
                        this.GetCancellationTokenOnDestroy());
                
                characterOperator.ResetEmotion();
                characterOperator.ResetLip();
            }
        }

        public void Enqueue((
            AudioQuery query,
            AudioClip clip,
            EmotionSample<FacialExpressions.Emotion.Emotion> emotion) command)
        {
            queue.Enqueue(command);
        }
    }
}