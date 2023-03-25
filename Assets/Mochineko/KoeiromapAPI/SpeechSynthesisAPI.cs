#nullable enable
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Mochineko.Relent.Result;
using Mochineko.Relent.UncertainResult;
using Mochineko.RelentJsonSerialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Mochineko.KoeiromapAPI
{
    public static class SpeechSynthesisAPI
    {
        private const string EndPoint = "https://api.rinna.co.jp/models/cttse/koeiro";
        private const string ResponsePrefix = "data:audio/wav;base64,";

        public static async Task<IUncertainResult<Stream>> SynthesisAsync(
            HttpClient httpClient,
            string text,
            CancellationToken cancellationToken,
            float? speakerX = null,
            float? speakerY = null,
            Style? style = null,
            ulong? seed = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return UncertainResultFactory.Fail<Stream>(
                    "Failed because text is null or empty.");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return UncertainResultFactory.Retry<Stream>(
                    "Retryable because cancellation has been already requested.");
            }

            // Create request body
            var requestBody = new RequestBody(
                text,
                speakerX,
                speakerY,
                style,
                seed);

            // Serialize request body
            string requestBodyJson;
            var serializationResult = RelentJsonSerializer.Serialize(
                requestBody,
                new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                });
            if (serializationResult is ISuccessResult<string> serializationSuccess)
            {
                requestBodyJson = serializationSuccess.Result;
            }
            else if (serializationResult is IFailureResult<string> serializationFailure)
            {
                return UncertainResultFactory.Fail<Stream>(
                    $"Failed because -> {serializationFailure}.");
            }
            else
            {
                throw new ResultPatternMatchException(nameof(serializationResult));
            }

            // Build request message
            using var requestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                EndPoint);

            // Request contents
            var requestContent = new StringContent(
                content: requestBodyJson,
                encoding: System.Text.Encoding.UTF8,
                mediaType: "application/json");
            requestMessage.Content = requestContent;

            try
            {
                // Post request and receive response
                using var responseMessage = await httpClient
                    .SendAsync(requestMessage, cancellationToken);

                if (responseMessage == null)
                {
                    return UncertainResultFactory.Fail<Stream>(
                        $"Failed because {nameof(HttpResponseMessage)} was null.");
                }

                var responseString = await responseMessage.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(responseString))
                {
                    return UncertainResultFactory.Fail<Stream>(
                        $"Failed because {nameof(Stream)} was null.");
                }
                
                Debug.Log(responseString);

                // Succeeded
                if (responseMessage.IsSuccessStatusCode)
                {
                    // Deserialize response body
                    var deserializationResult = RelentJsonSerializer.Deserialize<ResponseBody>(responseString);
                    if (deserializationResult is ISuccessResult<ResponseBody> deserializationSuccess)
                    {
                        var responseBody = deserializationSuccess.Result;
                    
                        if (!responseBody.Audio.StartsWith(ResponsePrefix))
                        {
                            return UncertainResultFactory.Fail<Stream>(
                                $"Failed because response text prefix was not start with \"{ResponsePrefix}\" -> {responseBody.Audio}.");
                        }

                        // Base64 decode
                        var base64EncodedAudio = responseBody.Audio
                            .Remove(0, ResponsePrefix.Length);

                        var base64DecodedAudio = Convert.FromBase64String(base64EncodedAudio);
                    
                        return UncertainResultFactory.Succeed<Stream>(
                            new MemoryStream(base64DecodedAudio));
                    }
                    if (deserializationResult is IFailureResult<ResponseBody> deserializationFailure)
                    {
                        return UncertainResultFactory.Fail<Stream>(
                            $"Failed because -> {deserializationFailure.Message}.");
                    }
                    else
                    {
                        throw new ResultPatternMatchException(nameof(deserializationResult));
                    }
                }
                // Retryable
                else if (responseMessage.StatusCode is HttpStatusCode.TooManyRequests
                         || (int)responseMessage.StatusCode is >= 500 and <= 599)
                {
                    return UncertainResultFactory.Retry<Stream>(
                        $"Retryable because the API returned status code:({(int)responseMessage.StatusCode}){responseMessage.StatusCode} with response -> {responseString}.");
                }
                // Response error
                else
                {
                    return UncertainResultFactory.Fail<Stream>(
                        $"Failed because the API returned status code:({(int)responseMessage.StatusCode}){responseMessage.StatusCode} with response -> {responseString}."
                    );
                }
            }
            // Request error
            catch (HttpRequestException exception)
            {
                return UncertainResultFactory.Retry<Stream>(
                    $"Retryable because {nameof(HttpRequestException)} was thrown during calling the API -> {exception}.");
            }
            // Task cancellation
            catch (TaskCanceledException exception)
                when (exception.CancellationToken == cancellationToken)
            {
                return UncertainResultFactory.Retry<Stream>(
                    $"Failed because task was canceled by user during call to the API -> {exception}.");
            }
            // Operation cancellation 
            catch (OperationCanceledException exception)
            {
                return UncertainResultFactory.Retry<Stream>(
                    $"Retryable because operation was cancelled during calling the API -> {exception}.");
            }
            // Unhandled error
            catch (Exception exception)
            {
                return UncertainResultFactory.Fail<Stream>(
                    $"Failed because an unhandled exception was thrown when calling the API -> {exception}.");
            }
        }
    }
}