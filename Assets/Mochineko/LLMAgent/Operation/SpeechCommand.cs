#nullable enable
using System.Collections.Generic;
using Mochineko.FacialExpressions.Emotion;
using Mochineko.LLMAgent.Pose;
using Mochineko.VOICEVOX_API.QueryCreation;
using UnityEngine;

namespace Mochineko.LLMAgent.Operation
{
    internal readonly struct SpeechCommand
    {
        public readonly AudioQuery AudioQuery;
        public readonly AudioClip AudioClip;
        public readonly EmotionSample<FacialExpressions.Emotion.Emotion> Emotion;
        public readonly Dictionary<HumanBodyBones, BoneLocalRotation> Pose;

        public SpeechCommand(AudioQuery audioQuery, AudioClip audioClip,
            EmotionSample<FacialExpressions.Emotion.Emotion> emotion,
            Dictionary<HumanBodyBones, BoneLocalRotation> pose)
        {
            AudioQuery = audioQuery;
            AudioClip = audioClip;
            Emotion = emotion;
            Pose = pose;
        }
    }
}