#nullable enable
using Mochineko.Relent.Result;
using Mochineko.RelentStateMachine;

namespace Mochineko.LLMAgent.Operation
{
    // TODO: Apply original state machine
    public static class StateResultsExtension<TEvent>
    {
        public static ISuccessResult<IEventRequest<TEvent>> Succeed { get; }
            = Results.Succeed(EventRequests.None<TEvent>());
    }
}