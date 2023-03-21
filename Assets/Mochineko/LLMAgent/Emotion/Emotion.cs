#nullable enable
using Newtonsoft.Json;

namespace Mochineko.LLMAgent.Emotion
{
    [JsonObject]
    public sealed class Emotion
    {
        [JsonProperty("happiness"), JsonRequired]
        public float Happiness { get; }
            
        [JsonProperty("sadness"), JsonRequired]
        public float Sadness { get; }
            
        [JsonProperty("anger"), JsonRequired]
        public float Anger { get; }
            
        [JsonProperty("fear"), JsonRequired]
        public float Fear { get; }
            
        [JsonProperty("surprise"), JsonRequired]
        public float Surprise { get; }
            
        [JsonProperty("disgust"), JsonRequired]
        public float Disgust { get; }

        public Emotion(
            float happiness,
            float sadness,
            float anger,
            float fear,
            float surprise,
            float disgust)
        {
            Happiness = happiness;
            Sadness = sadness;
            Anger = anger;
            Fear = fear;
            Surprise = surprise;
            Disgust = disgust;
        }
    }
}