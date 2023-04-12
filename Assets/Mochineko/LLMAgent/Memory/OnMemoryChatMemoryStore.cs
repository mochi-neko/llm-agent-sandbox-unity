#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.LLMAgent.Memory
{
    public sealed class OnMemoryChatMemoryStore : IChatMemoryStore
    {
        private string memory = string.Empty;
        
        public UniTask<IResult<string>> LoadAsync(CancellationToken cancellationToken)
        {
            return UniTask.FromResult<IResult<string>>(
                Results.Succeed(memory));
        }

        public UniTask<IResult> SaveAsync(string memory, CancellationToken cancellationToken)
        {
            this.memory = memory;
            return UniTask.FromResult<IResult>(
                Results.Succeed());
        }
    }
}