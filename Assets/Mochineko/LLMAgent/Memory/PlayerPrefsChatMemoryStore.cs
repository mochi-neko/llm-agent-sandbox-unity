#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Result;
using UnityEngine;

namespace Mochineko.LLMAgent.Memory
{
    public sealed class PlayerPrefsChatMemoryStore : IChatMemoryStore
    {
        private const string Key = "Mochineko.LLMAgent.ChatMemory";

        public UniTask<IResult<string>> LoadAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                var memory = PlayerPrefs.GetString(Key);
                if (!string.IsNullOrEmpty(memory))
                {
                    return UniTask.FromResult<IResult<string>>(
                        Results.Succeed(memory));
                }
                else
                {
                    return UniTask.FromResult<IResult<string>>(
                        Results.Fail<string>("Failed to load chat memory from PlayerPrefs because it is empty."));
                }
            }
            catch (Exception exception)
            {
                return UniTask.FromResult<IResult<string>>(
                    Results.Fail<string>(
                        $"Failed to load chat memory from PlayerPrefs because -> {exception}."));
            }
        }

        public UniTask<IResult> SaveAsync(
            string memory,
            CancellationToken cancellationToken)
        {
            try
            {
                PlayerPrefs.SetString(Key, memory);
                
                return UniTask.FromResult<IResult>(
                    Results.Succeed());
            }
            catch (Exception exception)
            {
                return UniTask.FromResult<IResult>(
                    Results.Fail(
                        $"Failed to save chat memory from PlayerPrefs because -> {exception}."));
            }
        }
    }
}