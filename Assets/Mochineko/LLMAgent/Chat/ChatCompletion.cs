#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.ChatGPT_API;
using Mochineko.ChatGPT_API.Memories;
using Mochineko.ChatGPT_API.Relent;
using Mochineko.Relent.Resilience;
using Mochineko.Relent.Result;
using Mochineko.Relent.UncertainResult;
using Debug = UnityEngine.Debug;

namespace Mochineko.LLMAgent.Chat
{
    public sealed class ChatCompletion
    {
        private readonly Model model;
        private readonly RelentChatCompletionAPIConnection connection;
        private readonly FiniteQueueChatMemory memory;
        private readonly IPolicy<ChatCompletionResponseBody> policy;

        public ChatCompletion(
            string apiKey,
            Model model,
            string prompt,
            int maxMemoryCount)
        {
            this.model = model;
            
            memory = new FiniteQueueChatMemory(maxMemoryCount);

            connection = new RelentChatCompletionAPIConnection(
                apiKey,
                memory,
                prompt);

            policy = PolicyFactory.BuildPolicy();
        }
        
        public void ClearMemory()
        {
            memory.ClearAllMessages();
        }
        
        public async UniTask<IResult<string>> SendChatAsync(
            string message,
            CancellationToken cancellationToken)
        {
            await UniTask.SwitchToThreadPool();

            var result = await policy.ExecuteAsync(
                async innerCancellationToken
                    => await connection.CompleteChatAsync(
                        message,
                        innerCancellationToken,
                        model),
                cancellationToken);

            await UniTask.SwitchToMainThread(cancellationToken);
            
            if (result is IUncertainSuccessResult<ChatCompletionResponseBody> success)
            {
                Debug.Log($"[LLMAgent.Chat] Succeeded to complete chat -> {success.Result.ResultMessage}");
                return ResultFactory.Succeed(success.Result.ResultMessage);
            }
            else if (result is IUncertainRetryableResult<ChatCompletionResponseBody> retryable)
            {
                Debug.LogError($"[LLMAgent.Chat] Failed to complete chat because -> {retryable.Message}");
                return ResultFactory.Fail<string>(
                    $"Failed to complete chat because -> {retryable.Message}.");
            }
            else if (result is IUncertainFailureResult<ChatCompletionResponseBody> failure)
            {
                Debug.LogError($"[LLMAgent.Chat] Failed to complete chat because -> {failure.Message}");
                return ResultFactory.Fail<string>(
                    $"Failed to complete chat because -> {failure.Message}.");
            }
            else
            {
                throw new UncertainResultPatternMatchException(nameof(result));
            }
        }
    }
}