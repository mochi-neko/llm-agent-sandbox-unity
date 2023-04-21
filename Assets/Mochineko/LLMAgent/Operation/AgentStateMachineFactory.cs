#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;
using Mochineko.RelentStateMachine;

namespace Mochineko.LLMAgent.Operation
{
    internal static class AgentStateMachineFactory
    {
        public static async UniTask<IFiniteStateMachine<AgentEvent, AgentContext>> CreateAsync(
            AgentContext context,
            CancellationToken cancellationToken)
        {
            var transitionMapBuilder = TransitionMapBuilder<AgentEvent, AgentContext>
                .Create<AgentIdleState>();

            var initializeResult = await FiniteStateMachine<AgentEvent, AgentContext>.CreateAsync(
                transitionMapBuilder.Build(),
                context,
                cancellationToken);
            switch (initializeResult)
            {
                case ISuccessResult<FiniteStateMachine<AgentEvent, AgentContext>> initializeSuccess:
                    return initializeSuccess.Result;
                
                case IFailureResult<FiniteStateMachine<AgentEvent, AgentContext>> initializeFailure:
                    throw new Exception($"Failed to initialize state machine because -> {initializeFailure.Message}.");

                default:
                    throw new ResultPatternMatchException(nameof(initializeResult));
            }
        }
    }
}