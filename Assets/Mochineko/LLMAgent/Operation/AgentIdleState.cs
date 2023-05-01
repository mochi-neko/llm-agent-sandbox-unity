#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;
using Mochineko.RelentStateMachine;
using UnityEngine;

namespace Mochineko.LLMAgent.Operation
{
    internal sealed class AgentIdleState : IState<AgentEvent, AgentContext>
    {
        private CancellationTokenSource? eyelidAnimationCanceller;
        
        public UniTask<IResult<IEventRequest<AgentEvent>>> EnterAsync(
            AgentContext context,
            CancellationToken cancellationToken)
        {
            Debug.Log($"[LLMAgent.Operation] Enter {nameof(AgentIdleState)}.");
            
            eyelidAnimationCanceller?.Cancel();
            eyelidAnimationCanceller = new CancellationTokenSource();
            
            context.EyelidAnimator.AnimateAsync(
                frames: context.EyelidAnimationFrames,
                loop: true,
                cancellationToken: eyelidAnimationCanceller.Token
            );
            
            return UniTask.FromResult<IResult<IEventRequest<AgentEvent>>>(
                StateResultsExtension<AgentEvent>.Succeed);
        }

        public UniTask<IResult<IEventRequest<AgentEvent>>> UpdateAsync(
            AgentContext context,
            CancellationToken cancellationToken)
        {
            context.EmotionAnimator.Update();
            
            return UniTask.FromResult<IResult<IEventRequest<AgentEvent>>>(
                StateResultsExtension<AgentEvent>.Succeed);
        }

        public UniTask<IResult> ExitAsync(
            AgentContext context,
            CancellationToken cancellationToken)
        {
            Debug.Log($"[LLMAgent.Operation] Exit {nameof(AgentIdleState)}.");
            
            eyelidAnimationCanceller?.Cancel();
            eyelidAnimationCanceller = null;
            
            return UniTask.FromResult<IResult>(
                Results.Succeed());
        }

        public void Dispose()
        {
            eyelidAnimationCanceller?.Dispose();
        }
    }
}