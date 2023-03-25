#nullable enable
using System.Collections.Generic;
using Mochineko.ChatGPT_API;
using TiktokenSharp;

namespace Mochineko.LLMAgent.Memory
{
    public sealed class FiniteTokenLengthQueueChatMemory : IChatMemory
    {
        private readonly int maxTokenLength;
        private readonly TikToken tikToken;
        private readonly Queue<Message> queue = new();

        public FiniteTokenLengthQueueChatMemory(int maxTokenLength, string model = "gpt-3.5-turbo")
        {
            this.maxTokenLength = maxTokenLength;
            this.tikToken = TikToken.EncodingForModel(model);
        }
        
        public IReadOnlyList<Message> Memories
            => queue.ToArray();
        
        public int TokenLength
            => CalculateTotalTokenLength();

        public void AddMessage(Message message)
        {
            queue.Enqueue(message);
            
            while (TokenLength > maxTokenLength)
            {
                queue.Dequeue();
            }
        }

        public void ClearAllMessages()
        {
            queue.Clear();
        }

        private int CalculateTotalTokenLength()
        {
            var length = 0;
            foreach (var message in queue)
            {
                length += tikToken
                    .Encode(message.Content)
                    .Count;
            }

            return length;
        }
    }
}