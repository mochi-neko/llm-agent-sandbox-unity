#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mochineko.ChatGPT_API;
using Mochineko.LLMAgent.Memory;

namespace Mochineko.LLMAgent.Operation
{
    public sealed class StateMemory : IChatMemory
    {
        private readonly LongTermChatMemory memory;
        private Message? prompt;

        public IReadOnlyList<Message> Messages
        {
            get
            {
                if (prompt != null)
                {
                    return memory.Conversations
                        .Concat(new[] { prompt })
                        .ToList();
                }
                else
                {
                    return memory.Conversations;
                }
            }
        }

        public StateMemory(LongTermChatMemory memory)
        {
            this.memory = memory;
        }

        public void SetPromptAsUserMessage(string prompt)
        {
            this.prompt = new Message(Role.User, prompt);
        }

        public Task AddMessageAsync(Message message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void ClearAllMessages()
        {
        }
    }
}