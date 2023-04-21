#nullable enable
using System.Collections.Generic;
using Mochineko.FacialExpressions.Blink;

namespace Mochineko.LLMAgent.Operation
{
    internal sealed class AgentContext
    {
        public IEyelidAnimator EyelidAnimator { get; }
        public IEnumerable<EyelidAnimationFrame> EyelidAnimationFrames { get; }

        public AgentContext(
            IEyelidAnimator eyelidAnimator,
            IEnumerable<EyelidAnimationFrame> eyelidAnimationFrames)
        {
            EyelidAnimator = eyelidAnimator;
            EyelidAnimationFrames = eyelidAnimationFrames;
        }
    }
}