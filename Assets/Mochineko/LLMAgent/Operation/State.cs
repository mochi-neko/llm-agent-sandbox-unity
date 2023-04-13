#nullable enable
using System.Collections.Generic;
using Mochineko.LLMAgent.Pose;
using Newtonsoft.Json;
using UnityEngine;

namespace Mochineko.LLMAgent.Operation
{
    [JsonObject]
    public sealed class State
    {
        [JsonProperty("emotion"), JsonRequired]
        public Emotion.Emotion Emotion { get; }

        [JsonProperty("pose"), JsonRequired]
        public Dictionary<HumanBodyBones, BoneLocalRotation> Pose { get; }

        public State(
            Emotion.Emotion emotion,
            Dictionary<HumanBodyBones, BoneLocalRotation> pose)
        {
            Emotion = emotion;
            Pose = pose;
        }
    }
}