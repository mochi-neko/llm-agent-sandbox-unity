#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;

namespace Mochineko.LLMAgent.Memory
{
    public interface IChatMemoryStore
    {
        UniTask<IResult<string>> LoadAsync(
            CancellationToken cancellationToken);

        UniTask<IResult> SaveAsync(
            string memory,
            CancellationToken cancellationToken);
    }
}