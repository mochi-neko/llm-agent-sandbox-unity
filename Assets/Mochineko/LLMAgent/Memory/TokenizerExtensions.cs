#nullable enable
using System.Collections.Generic;
using Mochineko.ChatGPT_API;
using TiktokenSharp;

namespace Mochineko.LLMAgent.Memory
{
    public static class TokenizerExtensions
    {
        public static int TokenLength(
            this IEnumerable<Message> messages,
            TikToken tikToken)
        {
            var length = 0;
            foreach (var message in messages)
            {
                length += tikToken
                    .Encode(message.Content)
                    .Count;
            }

            return length;
        }

        public static int TokenLength(this Message message, TikToken tikToken)
            => tikToken
                .Encode(message.Content)
                .Count;
    }
}