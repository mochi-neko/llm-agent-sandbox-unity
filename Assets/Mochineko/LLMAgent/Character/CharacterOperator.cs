#nullable enable
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.FacialExpressions.Blink;
using Mochineko.FacialExpressions.Emotion;
using Mochineko.FacialExpressions.Extensions.VOICEVOX;
using Mochineko.FacialExpressions.Extensions.VRM;
using Mochineko.FacialExpressions.LipSync;
using Mochineko.VOICEVOX_API.QueryCreation;
using UnityEngine;
using UniVRM10;
using VRMShaders;

namespace Mochineko.FacialExpressions.Samples
{
    public sealed class CharacterOperator : MonoBehaviour
    {
        [SerializeField] private string path = string.Empty;
        [SerializeField] private float emotionFollowingTime = 1f;

        private ILipMorpher? lipMorpher;
        private ILipAnimator? lipAnimator;
        private IEyelidAnimator? eyelidAnimator;
        private IEmotionMorpher<Emotion.Emotion>? emotionMorpher;
        private ExclusiveFollowingEmotionAnimator<Emotion.Emotion>? emotionAnimator;

        private async void Start()
        {
            var binary = await File.ReadAllBytesAsync(
                path,
                this.GetCancellationTokenOnDestroy());

            var instance = await LoadVRMAsync(
                binary,
                this.GetCancellationTokenOnDestroy());

            lipMorpher = new VRMLipMorpher(instance.Runtime.Expression);
            lipAnimator = new FollowingLipAnimator(lipMorpher);

            var eyelidMorpher = new VRMEyelidMorpher(instance.Runtime.Expression);
            eyelidAnimator = new SequentialEyelidAnimator(eyelidMorpher);

            var eyelidFrames = ProbabilisticEyelidAnimationGenerator.Generate(
                Eyelid.Both,
                blinkCount: 20);

            eyelidAnimator.AnimateAsync(
                    eyelidFrames,
                    loop: true,
                    this.GetCancellationTokenOnDestroy())
                .Forget();

            emotionMorpher = new VRMEmotionMorpher(instance.Runtime.Expression);
            emotionAnimator = new ExclusiveFollowingEmotionAnimator<Emotion.Emotion>(
                emotionMorpher,
                followingTime: emotionFollowingTime);
        }

        private void Update()
        {
            emotionAnimator?.Update();
        }

        // ReSharper disable once InconsistentNaming
        private static async UniTask<Vrm10Instance> LoadVRMAsync(
            byte[] binaryData,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await Vrm10.LoadBytesAsync(
                bytes: binaryData,
                canLoadVrm0X: true,
                controlRigGenerationOption: ControlRigGenerationOption.None,
                showMeshes: true,
                awaitCaller: new RuntimeOnlyAwaitCaller(),
                materialGenerator: null,
                vrmMetaInformationCallback: null,
                ct: cancellationToken
            );
        }

        public async UniTask AnimateLipAsync(
            AudioQuery audioQuery,
            CancellationToken cancellationToken)
        {
            if (lipAnimator == null)
            {
                return;
            }

            var lipFrames = AudioQueryConverter
                .ConvertToSequentialAnimationFrames(audioQuery);

            await lipAnimator
                .AnimateAsync(lipFrames, cancellationToken);
        }

        public void ResetLip()
        {
            lipMorpher?.Reset();
        }

        public void Emote(EmotionSample<Emotion.Emotion> sample)
        {
            emotionAnimator?.Emote(sample);
        }

        public void ResetEmotion()
        {
            emotionMorpher?.Reset();
        }
    }
}