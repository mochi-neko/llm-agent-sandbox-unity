#nullable enable
using System.Collections.Generic;
using System.Linq;
using Mochineko.ChatGPT_API;
using TiktokenSharp;

namespace Mochineko.LLMAgent.Memory
{
    public sealed class FiniteTokenLengthQueueWithFixedPromptsChatMemory : IChatMemory
    {
        private readonly int maxTokenLengthWithoutPrompts;
        private readonly TikToken tikToken;
        private readonly Queue<Message> queue = new();
        private readonly List<Message> prompts = new();

        public FiniteTokenLengthQueueWithFixedPromptsChatMemory(int maxTokenLengthWithoutPrompts, string model = "gpt-3.5-turbo")
        {
            this.maxTokenLengthWithoutPrompts = maxTokenLengthWithoutPrompts;
            this.tikToken = TikToken.EncodingForModel(model);
        }

        public IReadOnlyList<Message> Memories
            => prompts
                .Concat(queue)
                .ToList();

        public int TokenLengthWithoutPrompts
            => CalculateTokenLength(queue);

        public int TotalTokenLength
            => CalculateTokenLength(prompts)
               + CalculateTokenLength(queue);

        public void AddMessage(Message message)
        {
            if (message.Role is Role.System)
            {
                prompts.Add(message);
            }
            else
            {
                queue.Enqueue(message);

                while (TokenLengthWithoutPrompts > maxTokenLengthWithoutPrompts)
                {
                    queue.Dequeue();
                }
            }
        }

        public void ClearAllMessages()
        {
            prompts.Clear();
            queue.Clear();
        }
        
        public void ClearMessagesWithoutPrompts()
        {
            queue.Clear();
        }

        private int CalculateTokenLength(IEnumerable<Message> messages)
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
    }
}