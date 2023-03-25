#nullable enable
using System;
using Mochineko.Relent.Result;
using Newtonsoft.Json;

namespace Mochineko.RelentJsonSerialization
{
    public static class RelentJsonSerializer
    {
        public static IResult<string> Serialize<T>(
            T serialized,
            JsonSerializerSettings? settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(
                    serialized,
                    Formatting.Indented,
                    settings);

                if (!string.IsNullOrEmpty(json))
                {
                    return ResultFactory.Succeed(json);
                }
                else
                {
                    return ResultFactory.Fail<string>(
                        $"Failed to serialize because serialized JSON of {typeof(T)} was null or empty.");
                }
            }
            catch (JsonSerializationException exception)
            {
                return ResultFactory.Fail<string>(
                    $"Failed to serialize {typeof(T)} to JSON because -> {exception}");
            }
            catch (Exception exception)
            {
                return ResultFactory.Fail<string>(
                    $"Failed to serialize {typeof(T)} to JSON because unhandled exception -> {exception}");
            }
        }

        public static IResult<T> Deserialize<T>(string json)
        {
            try
            {
                var requestBody = JsonConvert.DeserializeObject<T>(json);

                if (requestBody != null)
                {
                    return ResultFactory.Succeed(requestBody);
                }
                else
                {
                    return ResultFactory.Fail<T>(
                        $"Failed to deserialize because deserialized object of {typeof(T)} was null.");
                }
            }
            catch (JsonSerializationException exception)
            {
                return ResultFactory.Fail<T>(
                    $"Failed to deserialize {typeof(T)} from JSON because -> {exception}");
            }
            catch (JsonReaderException exception)
            {
                return ResultFactory.Fail<T>(
                    $"Failed to deserialize {typeof(T)} from JSON because -> {exception}");
            }
            catch (Exception exception)
            {
                return ResultFactory.Fail<T>(
                    $"Failed to deserialize {typeof(T)} from JSON because unhandled exception -> {exception}");
            }
        }
    }
}