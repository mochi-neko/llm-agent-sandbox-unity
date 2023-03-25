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

        public FiniteTokenLengthQueueWithFixedPromptsChatMemory(
            int maxTokenLengthWithoutPrompts,
            string model = "gpt-3.5-turbo")
        {
            this.maxTokenLengthWithoutPrompts = maxTokenLengthWithoutPrompts;
            this.tikToken = TikToken.EncodingForModel(model);
        }

        public IReadOnlyList<Message> Memories
            => prompts
                .Concat(queue)
                .ToList();

        public int TokenLengthWithoutPrompts
            => queue.TokenLength(tikToken);
        
        private int PromptsTokenLength
            => prompts.TokenLength(tikToken);

        public int MemoriesTokenLength
            => PromptsTokenLength
               + TokenLengthWithoutPrompts;

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
    }
}