#nullable enable
using System.Collections.Generic;
using System.Linq;
using Mochineko.ChatGPT_API;
using Newtonsoft.Json;

namespace Mochineko.LLMAgent.Summarization
{
    [JsonObject]
    public sealed class ConversationCollection
    {
        [JsonProperty("conversations"), JsonRequired]
        public List<Message> Conversations { get; private set; }

        public ConversationCollection(IReadOnlyList<Message> conversations)
        {
            this.Conversations = conversations.ToList();
        }
    }
}