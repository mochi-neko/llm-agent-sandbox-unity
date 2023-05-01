#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mochineko.FacialExpressions.Blink;
using Mochineko.FacialExpressions.Emotion;
using Mochineko.FacialExpressions.LipSync;
using UnityEngine;

namespace Mochineko.LLMAgent.Operation
{
    internal sealed class AgentContext
    {
        public IEyelidAnimator EyelidAnimator { get; }
        public IEnumerable<EyelidAnimationFrame> EyelidAnimationFrames { get; }
        
        public ILipMorpher LipMorpher { get; }
        public ILipAnimator LipAnimator { get; }

        public ExclusiveFollowingEmotionAnimator<FacialExpressions.Emotion.Emotion>
            EmotionAnimator { get; }

        public AudioSource AudioSource { get; }
        public ConcurrentQueue<SpeechCommand> SpeechQueue { get; } = new();

        public AgentContext(
            IEyelidAnimator eyelidAnimator,
            IEnumerable<EyelidAnimationFrame> eyelidAnimationFrames,
            ILipMorpher lipMorpher,
            ILipAnimator lipAnimator,
            AudioSource audioSource, ExclusiveFollowingEmotionAnimator<FacialExpressions.Emotion.Emotion> emotionAnimator)
        {
            EyelidAnimator = eyelidAnimator;
            EyelidAnimationFrames = eyelidAnimationFrames;
            LipMorpher = lipMorpher;
            LipAnimator = lipAnimator;
            AudioSource = audioSource;
            EmotionAnimator = emotionAnimator;
        }
    }
}