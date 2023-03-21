#nullable enable
using Newtonsoft.Json;

namespace Mochineko.KoeiromapAPI
{
    [JsonObject]
    internal sealed class RequestBody
    {
        /// <summary>
        /// Required.
        /// </summary>
        [JsonProperty("text"), JsonRequired]
        public string Text { get; }
        
        /// <summary>
        /// Optional.
        /// Defaults to 0.0.
        /// [-3.0, 3.0]
        /// </summary>
        [JsonProperty("speaker_x")]
        public float? SpeakerX { get; }
        
        /// <summary>
        /// Optional.
        /// Defaults to 0.0.
        /// [-3.0, 3.0]
        /// </summary>
        [JsonProperty("speaker_y")]
        public float? SpeakerY { get; }
        
        /// <summary>
        /// Optional.
        /// Defaults to "talk".
        /// </summary>
        [JsonProperty("style")]
        public string? Style { get; }
        
        /// <summary>
        /// Optional.
        /// Defaults to null.
        /// </summary>
        [JsonProperty("seed")]
        public ulong? Seed { get; }

        public RequestBody(
            string text,
            float? speakerX = null,
            float? speakerY = null,
            Style? style = null,
            ulong? seed = null)
        {
            Text = text;
            SpeakerX = speakerX;
            SpeakerY = speakerY;
            Style = style?.ToText();
            Seed = seed;
        }
    }
}