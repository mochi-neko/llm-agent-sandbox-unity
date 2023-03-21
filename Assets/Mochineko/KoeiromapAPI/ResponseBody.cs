#nullable enable
using System;
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
        public ulong Seed { get; private set; }
    }
}