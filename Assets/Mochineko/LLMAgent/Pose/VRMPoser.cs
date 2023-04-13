#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace Mochineko.LLMAgent.Pose
{
    // ReSharper disable once InconsistentNaming
    public sealed class VRMPoser
    {
        private readonly UniVRM10.Vrm10RuntimeControlRig controlRig;

        public VRMPoser(UniVRM10.Vrm10RuntimeControlRig controlRig)
        {
            this.controlRig = controlRig;
        }

        public void ApplyPose(Dictionary<HumanBodyBones, BoneLocalRotation> pose)
        {
            foreach (var bone in pose)
            {
                if (bone.Key == HumanBodyBones.LastBone)
                {
                    continue;
                }

                var boneTransform = controlRig.GetBoneTransform(bone.Key);
                if (boneTransform != null)
                {
                    boneTransform.localRotation = bone.Value.Rotation;
                }
            }
        }
    }
}