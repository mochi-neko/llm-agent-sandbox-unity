#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.FacialExpressions.Extensions.VOICEVOX;
using Mochineko.FacialExpressions.LipSync;
using Mochineko.Relent.Result;
using Mochineko.RelentStateMachine;
using Mochineko.VOICEVOX_API.QueryCreation;
using UnityEngine;

namespace Mochineko.LLMAgent.Operation
{
    internal sealed class AgentSpeakingState : IState<AgentEvent, AgentContext>
    {
        private CancellationTokenSource? speakingCanceller;
        private bool isSpeaking = false;

        public UniTask<IResult<IEventRequest<AgentEvent>>> EnterAsync(
            AgentContext context,
            CancellationToken cancellationToken)
        {
            if (!context.SpeechQueue.Any())
            {
                return UniTask.FromResult<IResult<IEventRequest<AgentEvent>>>(
                    StateResults.Fail<AgentEvent>("Speech queue is empty."));
            }

            Debug.Log($"[LLMAgent.Operation] Enter {nameof(AgentSpeakingState)}.");

            speakingCanceller?.Dispose();
            speakingCanceller = new CancellationTokenSource();

            return UniTask.FromResult<IResult<IEventRequest<AgentEvent>>>(
                StateResultsExtension<AgentEvent>.Succeed);
        }

        public UniTask<IResult<IEventRequest<AgentEvent>>> UpdateAsync(
            AgentContext context,
            CancellationToken cancellationToken)
        {
            if (speakingCanceller == null || speakingCanceller.IsCancellationRequested)
            {
                return UniTask.FromResult<IResult<IEventRequest<AgentEvent>>>(
                    StateResults.Fail<AgentEvent>("Speech has been cancelled."));
            }

            if (isSpeaking)
            {
                context.EmotionAnimator.Update();

                return UniTask.FromResult<IResult<IEventRequest<AgentEvent>>>(
                    StateResultsExtension<AgentEvent>.Succeed);
            }
            else
            {
                if (context.SpeechQueue.TryDequeue(out var command))
                {
                    try
                    {
                        SpeechAsync(context, command, speakingCanceller.Token)
                            .Forget();

                        return UniTask.FromResult<IResult<IEventRequest<AgentEvent>>>(
                            StateResultsExtension<AgentEvent>.Succeed);
                    }
                    catch (OperationCanceledException exception)
                    {
                        return UniTask.FromResult<IResult<IEventRequest<AgentEvent>>>(
                            StateResults.SucceedAndRequest(AgentEvent.FinishSpeaking));
                    }
                }
                else
                {
                    return UniTask.FromResult<IResult<IEventRequest<AgentEvent>>>(
                        StateResults.SucceedAndRequest(AgentEvent.FinishSpeaking));
                }
            }
        }

        public UniTask<IResult> ExitAsync(
            AgentContext context,
            CancellationToken cancellationToken)
        {
            Debug.Log($"[LLMAgent.Operation] Exit {nameof(AgentSpeakingState)}.");

            speakingCanceller?.Cancel();
            speakingCanceller = null;

            return UniTask.FromResult<IResult>(
                Results.Succeed());
        }

        public void Dispose()
        {
            speakingCanceller?.Dispose();
        }

        private async UniTask SpeechAsync(
            AgentContext context,
            SpeechCommand command,
            CancellationToken cancellationToken)
        {
            context.EmotionAnimator.Emote(command.Emotion);

            var lipAnimationFrames = AudioQueryConverter
                .ConvertToSequentialAnimationFrames(command.AudioQuery);

            isSpeaking = true;

            try
            {
                await UniTask.WhenAll(
                    context.AudioSource.PlayAsync(
                        command.AudioClip,
                        cancellationToken),
                    context.LipAnimator.AnimateAsync(
                        lipAnimationFrames,
                        cancellationToken));
            }
            finally
            {
                UnityEngine.Object.Destroy(command.AudioClip);
            }

            context.EmotionAnimator.Reset();
            context.LipMorpher.Reset();

            isSpeaking = false;
        }
    }
}