#nullable enable
using System.Collections.Generic;
using System.Linq;
using Mochineko.ChatGPT_API;

namespace Mochineko.LLMAgent.Memory
{
    public sealed class FiniteQueueWithFixedPromptsChatMemory : IChatMemory
    {
        private readonly int maxMemoriesCountWithoutPrompts;
        private readonly Queue<Message> queue;
        private readonly List<Message> prompts = new();

        public FiniteQueueWithFixedPromptsChatMemory(int maxMemoriesCountWithoutPrompts)
        {
            this.maxMemoriesCountWithoutPrompts = maxMemoriesCountWithoutPrompts;
            this.queue = new Queue<Message>(maxMemoriesCountWithoutPrompts);
        }

        public IReadOnlyList<Message> Memories
            => prompts
                .Concat(queue)
                .ToList();

        public void AddMessage(Message message)
        {
            if (message.Role is Role.System)
            {
                prompts.Add(message);
            }
            else
            {
                queue.Enqueue(message);

                while (queue.Count > maxMemoriesCountWithoutPrompts)
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