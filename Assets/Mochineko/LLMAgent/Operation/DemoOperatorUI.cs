#nullable enable
using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Mochineko.LLMAgent.Operation
{
    internal sealed class DemoOperatorUI : MonoBehaviour
    {
        [SerializeField] private DemoOperator? demoOperator = null;
        [SerializeField] private TMPro.TMP_InputField? messageInput = null;
        [SerializeField] private Button? sendButton = null;

        private void Awake()
        {
            if (demoOperator == null)
            {
                throw new NullReferenceException(nameof(demoOperator));
            }

            if (messageInput == null)
            {
                throw new NullReferenceException(nameof(messageInput));
            }

            if (sendButton == null)
            {
                throw new NullReferenceException(nameof(sendButton));
            }

            sendButton
                .OnClickAsObservable()
                .Subscribe(async _ =>
                {
                    if (string.IsNullOrWhiteSpace(messageInput.text))
                    {
                        return;
                    }

                    await demoOperator.ChatAsync(
                        messageInput.text,
                        this.GetCancellationTokenOnDestroy()
                    );
                })
                .AddTo(this);
        }
    }
}