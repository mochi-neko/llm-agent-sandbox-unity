#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Mochineko.ChatGPT_API;
using Mochineko.LLMAgent.Summarization;
using Mochineko.Relent.Result;
using TiktokenSharp;
using UnityEngine;

namespace Mochineko.LLMAgent.Memory
{
    public sealed class LongTermChatMemory : IChatMemory
    {
        private readonly int maxShortTermMemoriesTokenLength;
        private readonly int maxBufferMemoriesTokenLength;
        private readonly TikToken tikToken;
        private readonly List<Message> prompts = new();
        internal IEnumerable<Message> Prompts => prompts.ToArray();
        private readonly Queue<Message> shortTermMemories = new();
        internal IEnumerable<Message> ShortTermMemories => shortTermMemories.ToArray();
        private readonly Queue<Message> bufferMemories = new();
        internal IEnumerable<Message> BufferMemories => bufferMemories.ToArray();
        private readonly Summarizer summarizer;
        private readonly IChatMemoryStore store;
        private Message summary;
        internal Message Summary => summary;
        private readonly object lockObject = new();

        public static async UniTask<LongTermChatMemory> InstantiateAsync(
            int maxShortTermMemoriesTokenLength,
            int maxBufferMemoriesTokenLength,
            string apiKey,
            Model model,
            IChatMemoryStore? store,
            CancellationToken cancellationToken)
        {
            var instance = new LongTermChatMemory(
                maxShortTermMemoriesTokenLength,
                maxBufferMemoriesTokenLength,
                apiKey,
                model,
                store);

            var result = await instance.store.LoadAsync(cancellationToken);
            if (result is ISuccessResult<string> success)
            {
                Debug.Log($"[LLMAgent.Memory] Succeeded to load chat memory from store:{success.Result}");
                instance.summary = new Message(Role.System, success.Result);
            }
            else if (result is IFailureResult<string> failure)
            {
                Debug.LogError(
                    $"[LLMAgent.Memory] Failed to load chat memory from store because -> {failure.Message}");
            }
            else
            {
                throw new ResultPatternMatchException(nameof(result));
            }

            return instance;
        }

        private LongTermChatMemory(
            int maxShortTermMemoriesTokenLength,
            int maxBufferMemoriesTokenLength,
            string apiKey,
            Model model,
            IChatMemoryStore? store)
        {
            this.maxShortTermMemoriesTokenLength = maxShortTermMemoriesTokenLength;
            this.maxBufferMemoriesTokenLength = maxBufferMemoriesTokenLength;
            this.tikToken = TikToken.EncodingForModel(model.ToText());
            this.summarizer = new Summarizer(apiKey, model);

            this.store = store ?? new NullChatMemoryStore();
            this.summary = new Message(Role.System, string.Empty);
        }

        public IReadOnlyList<Message> Messages
            => prompts
                .Concat(new[] { summary })
                .Concat(shortTermMemories)
                .ToList();

        public IReadOnlyList<Message> Conversations
            => new[] { summary }
                .Concat(shortTermMemories)
                .Concat(bufferMemories)
                .ToList();

        public int ShortTermMemoriesTokenLength
            => shortTermMemories.TokenLength(tikToken);

        public int BufferMemoriesTokenLength
            => bufferMemories.TokenLength(tikToken);

        public int SummaryTokenLength
            => summary.TokenLength(tikToken);

        public int PromptsTokenLength
            => prompts.TokenLength(tikToken);

        public int TotalMemoriesTokenLength
            => PromptsTokenLength
               + SummaryTokenLength
               + ShortTermMemoriesTokenLength;

        public async Task AddMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (message.Role is Role.System)
            {
                lock (lockObject)
                {
                    prompts.Add(message);
                }
            }
            else if (message.Role is Role.User)
            {
                lock (lockObject)
                {
                    shortTermMemories.Enqueue(message);
                }
            }
            else if (message.Role is Role.Assistant)
            {
                lock (lockObject)
                {
                    shortTermMemories.Enqueue(message);
                }

                while (ShortTermMemoriesTokenLength > maxShortTermMemoriesTokenLength)
                {
                    bool tryDequeue;
                    Message? dequeued;
                    lock (lockObject)
                    {
                        tryDequeue = shortTermMemories.TryDequeue(out dequeued);
                    }

                    if (tryDequeue)
                    {
                        lock (lockObject)
                        {
                            bufferMemories.Enqueue(dequeued);
                        }

                        if (BufferMemoriesTokenLength > maxBufferMemoriesTokenLength)
                        {
                            await SummarizeAsync(cancellationToken);
                        }
                    }
                }
            }

            Debug.Log(
                $"[LLMAgent.Chat] Update memory by adding {message}, short term:{ShortTermMemoriesTokenLength}, buffer:{BufferMemoriesTokenLength}, summary:{SummaryTokenLength}/{summary.Content}.");
        }

        private async UniTask SummarizeAsync(CancellationToken cancellationToken)
        {
            List<Message> buffers;
            lock (lockObject)
            {
                buffers = bufferMemories.ToList();
            }

            var summarizeResult = await summarizer.SummarizeAsync(
                buffers,
                cancellationToken);
            if (summarizeResult is ISuccessResult<string> summarizeSuccess)
            {
                Debug.Log(
                    $"[LLMAgent.Summarization] Succeeded to summarize long term memory:{summarizeSuccess.Result}");

                lock (lockObject)
                {
                    summary = new Message(
                        Role.System,
                        summarizeSuccess.Result);
                }
            }
            else
            {
                Debug.LogError(
                    $"[LLMAgent.Summarization] Failed to summarize long term memory then keep summary and forget buffers:{summary.Content}");
            }

            lock (lockObject)
            {
                bufferMemories.Clear();
            }
        }

        public void ClearAllMessages()
        {
            lock (lockObject)
            {
                prompts.Clear();
                shortTermMemories.Clear();
                bufferMemories.Clear();
                summary = new Message(Role.System, "");
            }
        }

        public void ClearPrompts()
        {
            lock (lockObject)
            {
                prompts.Clear();
            }
        }

        public void ClearShortTermMemory()
        {
            lock (lockObject)
            {
                shortTermMemories.Clear();
            }
        }

        public void ClearBufferMemory()
        {
            lock (lockObject)
            {
                bufferMemories.Clear();
            }
        }

        public void ClearSummary()
        {
            lock (lockObject)
            {
                summary = new Message(Role.System, "");
            }
        }
    }
}