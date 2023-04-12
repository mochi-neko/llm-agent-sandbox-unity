#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.LLMAgent.Memory
{
    public sealed class NullChatMemoryStore : IChatMemoryStore
    {
        public UniTask<IResult<string>> LoadAsync(CancellationToken cancellationToken)
        {
            return UniTask.FromResult<IResult<string>>(
                Results.Succeed(string.Empty));
        }

        public UniTask<IResult> SaveAsync(string memory, CancellationToken cancellationToken)
        {
            return UniTask.FromResult<IResult>(
                Results.Succeed());
        }
    }
}