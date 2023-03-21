#nullable enable
using System;
using Mochineko.Relent.Result;
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
        public long? Seed { get; }

        public RequestBody(
            string text,
            float? speakerX = null,
            float? speakerY = null,
            Style? style = null,
            long? seed = null)
        {
            Text = text;
            SpeakerX = speakerX;
            SpeakerY = speakerY;
            Style = style?.ToText();
            Seed = seed;
        }

        public IResult<string> Serialize()
        {
            try
            {
                var json = JsonConvert.SerializeObject(
                    this,
                    Formatting.Indented,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });

                if (!string.IsNullOrEmpty(json))
                {
                    return ResultFactory.Succeed(json);
                }
                else
                {
                    return ResultFactory.Fail<string>(
                        $"Failed to serialize because serialized JSON of {nameof(RequestBody)} was null or empty.");
                }
            }
            catch (JsonSerializationException exception)
            {
                return ResultFactory.Fail<string>(
                    $"Failed to serialize {nameof(RequestBody)} to JSON because -> {exception}");
            }
            catch (Exception exception)
            {
                return ResultFactory.Fail<string>(
                    $"Failed to serialize {nameof(RequestBody)} to JSON because unhandled exception -> {exception}");
            }
        }

        public static IResult<RequestBody> Deserialize(string json)
        {
            try
            {
                var requestBody = JsonConvert.DeserializeObject<RequestBody>(json);

                if (requestBody != null)
                {
                    return ResultFactory.Succeed(requestBody);
                }
                else
                {
                    return ResultFactory.Fail<RequestBody>(
                        $"Failed to deserialize because deserialized object of {nameof(RequestBody)} was null.");
                }
            }
            catch (JsonSerializationException exception)
            {
                return ResultFactory.Fail<RequestBody>(
                    $"Failed to deserialize {nameof(RequestBody)} from JSON because -> {exception}");
            }
            catch (JsonReaderException exception)
            {
                return ResultFactory.Fail<RequestBody>(
                    $"Failed to deserialize {nameof(RequestBody)} from JSON because -> {exception}");
            }
            catch (Exception exception)
            {
                return ResultFactory.Fail<RequestBody>(
                    $"Failed to deserialize {nameof(RequestBody)} from JSON because unhandled exception -> {exception}");
            }
        }
    }
}