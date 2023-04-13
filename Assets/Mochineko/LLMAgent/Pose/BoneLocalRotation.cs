#nullable enable
using Newtonsoft.Json;
using UnityEngine;

namespace Mochineko.LLMAgent.Pose
{
    [JsonObject]
    public sealed class BoneLocalRotation
    {
        [JsonProperty("x"), JsonRequired] public float X { get; private set; }
        [JsonProperty("y"), JsonRequired] public float Y { get; private set; }
        [JsonProperty("z"), JsonRequired] public float Z { get; private set; }

        public BoneLocalRotation(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        
        [JsonIgnore]
        public Quaternion Rotation
            => Quaternion.Euler(new(X, Y, Z));
    }
}