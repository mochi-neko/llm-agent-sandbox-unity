#nullable enable
using System.Collections.Generic;
using Mochineko.ChatGPT_API;

namespace Mochineko.LLMAgent.Memory
{
    public sealed class FiniteQueueChatMemory : IChatMemory
    {
        private readonly Queue<Message> queue;
        private readonly int maxMemoriesCount;
            
        public IReadOnlyList<Message> Memories => queue.ToArray();

        public FiniteQueueChatMemory(int maxMemoriesCount)
        {
            this.maxMemoriesCount = maxMemoriesCount;
            this.queue = new Queue<Message>(maxMemoriesCount);
        }
        
        public void AddMessage(Message message)
        {
            queue.Enqueue(message);

            while (queue.Count > maxMemoriesCount)
            {
                queue.Dequeue();
            }
        }

        public void ClearAllMessages()
        {
            queue.Clear();
        }
    }
}