#nullable enable
using System;
using Mochineko.Relent.Result;
using Newtonsoft.Json;

namespace Mochineko.KoeiromapAPI
{
    [JsonObject]
    internal sealed class ResponseBody
    {
        /// <summary>
        /// Required.
        /// </summary>
        [JsonProperty("audio"), JsonRequired]
        public string Audio { get; private set; } = string.Empty;

        /// <summary>
        /// Required.
        /// </summary>
        [JsonProperty("phonemes"), JsonRequired]
        public string[] Phonemes { get; private set; } = Array.Empty<string>();
        
        /// <summary>
        /// Required.
        /// </summary>
        [JsonProperty("seed"), JsonRequired]
        public long Seed { get; private set; }

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
                        $"Failed to serialize because serialized JSON of {nameof(ResponseBody)} was null or empty.");
                }
            }
            catch (JsonSerializationException exception)
            {
                return ResultFactory.Fail<string>(
                    $"Failed to serialize {nameof(ResponseBody)} to JSON because -> {exception}");
            }
            catch (Exception exception)
            {
                return ResultFactory.Fail<string>(
                    $"Failed to serialize {nameof(ResponseBody)} to JSON because unhandled exception -> {exception}");
            }
        }

        public static IResult<ResponseBody> Deserialize(string json)
        {
            try
            {
                var requestBody = JsonConvert.DeserializeObject<ResponseBody>(json);

                if (requestBody != null)
                {
                    return ResultFactory.Succeed(requestBody);
                }
                else
                {
                    return ResultFactory.Fail<ResponseBody>(
                        $"Failed to deserialize because deserialized object of {nameof(ResponseBody)} was null.");
                }
            }
            catch (JsonSerializationException exception)
            {
                return ResultFactory.Fail<ResponseBody>(
                    $"Failed to deserialize {nameof(ResponseBody)} from JSON because -> {exception}");
            }
            catch (JsonReaderException exception)
            {
                return ResultFactory.Fail<ResponseBody>(
                    $"Failed to deserialize {nameof(ResponseBody)} from JSON because -> {exception}");
            }
            catch (Exception exception)
            {
                return ResultFactory.Fail<ResponseBody>(
                    $"Failed to deserialize {nameof(ResponseBody)} from JSON because unhandled exception -> {exception}");
            }
        }
    }
}