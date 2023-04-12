#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.ChatGPT_API;
using Mochineko.ChatGPT_API.Relent;
using Mochineko.Relent.Extensions.NewtonsoftJson;
using Mochineko.Relent.Resilience;
using Mochineko.Relent.Result;
using Mochineko.Relent.UncertainResult;
using TiktokenSharp;
using UnityEngine;

namespace Mochineko.LLMAgent.Summarization
{ 
    // summary tokens = X = 2000: variable
    // memory buffer tokens = Y = 1000: fixed
    // short term memory tokens = Z = 200: fixed
    // chat prompt tokens = W: variable

    // memory tokens = chat prompt(W:200) + summary(X:2000) + short term memory(Z:1000) + chat message(~200) + response(~200) < 4000
    // summarize tokens = summarize prompt(~200) + conversations(Y:1000) + summary(X:2000) < 4000

    public sealed class Summarizer
    {
        private readonly Model model;
        private readonly TikToken tikToken;
        private readonly RelentChatCompletionAPIConnection connection;
        private readonly IChatMemory simpleChatMemory = new SimpleChatMemory();
        private readonly IPolicy<ChatCompletionResponseBody> policy;
        private readonly string summary = string.Empty;
        private const int MaxSummaryTokenLength = 2000;

        public Summarizer(
            string apiKey,
            Model model)
        {
            this.model = model;

            tikToken = TikToken.EncodingForModel(model.ToText());

            connection = new RelentChatCompletionAPIConnection(
                apiKey,
                simpleChatMemory);

            policy = PolicyFactory.BuildPolicy();
        }

        public async UniTask<IResult<string>> SummarizeAsync(
            IReadOnlyList<Message> messages,
            CancellationToken cancellationToken)
        {
            Debug.Log($"[LLMAgent.Summarization] Begin to summarize messages.");

            await UniTask.SwitchToThreadPool();

            var conversations = new ConversationCollection(messages);
            string conversationsJson;
            var serializeResult = RelentJsonSerializer.Serialize(conversations);
            if (serializeResult is ISuccessResult<string> serializeSuccess)
            {
                conversationsJson = serializeSuccess.Result;
            }
            else if (serializeResult is IFailureResult<string> serializeFailure)
            {
                Debug.LogError(
                    $"[LLMAgent.Summarization] Failed to serialize conversations because -> {serializeFailure.Message}.");
                return Results.FailWithTrace<string>(
                    $"Failed to serialize conversations because -> {serializeFailure.Message}");
            }
            else
            {
                throw new ResultPatternMatchException(nameof(serializeResult));
            }

            var prompt = PromptTemplate.Summarize(summary, conversationsJson);

            simpleChatMemory.ClearAllMessages();

            var result = await policy.ExecuteAsync(
                async innerCancellationToken
                    => await connection.CompleteChatAsync(
                        prompt,
                        innerCancellationToken,
                        model,
                        maxTokens: MaxSummaryTokenLength),
                cancellationToken);

            if (result is IUncertainSuccessResult<ChatCompletionResponseBody> success)
            {
                Debug.Log(
                    $"[LLMAgent.Summarization] Succeeded to summarize messages -> {success.Result.ResultMessage}");
                return Results.Succeed(success.Result.ResultMessage);
            }
            else if (result is IUncertainRetryableResult<ChatCompletionResponseBody> retryable)
            {
                Debug.LogError($"[LLMAgent.Summarization] Failed to summarize messages because -> {retryable.Message}");
                return Results.FailWithTrace<string>(
                    $"Failed to summarize messages because -> {retryable.Message}.");
            }
            else if (result is IUncertainFailureResult<ChatCompletionResponseBody> failure)
            {
                Debug.LogError($"[LLMAgent.Summarization] Failed to summarize messages because -> {failure.Message}");
                return Results.FailWithTrace<string>(
                    $"Failed to summarize messages because -> {failure.Message}.");
            }
            else
            {
                throw new UncertainResultPatternMatchException(nameof(result));
            }
        }
    }
}