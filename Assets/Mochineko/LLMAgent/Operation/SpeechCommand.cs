#nullable enable
using Mochineko.FacialExpressions.Emotion;
using Mochineko.VOICEVOX_API.QueryCreation;
using UnityEngine;

namespace Mochineko.LLMAgent.Operation
{
    internal readonly struct SpeechCommand
    {
        public readonly AudioQuery AudioQuery;
        public readonly AudioClip AudioClip;
        public readonly EmotionSample<FacialExpressions.Emotion.Emotion> Emotion;

        public SpeechCommand(AudioQuery audioQuery, AudioClip audioClip,
            EmotionSample<FacialExpressions.Emotion.Emotion> emotion)
        {
            AudioQuery = audioQuery;
            AudioClip = audioClip;
            Emotion = emotion;
        }
    }
}