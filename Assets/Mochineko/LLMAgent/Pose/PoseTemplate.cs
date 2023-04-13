#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace Mochineko.LLMAgent.Pose
{
    public static class PoseTemplate
    {
        public static Dictionary<HumanBodyBones, BoneLocalRotation> Neutral = new()
        {
            [HumanBodyBones.Hips] = new(0f, 0f, 0f),
            [HumanBodyBones.Spine] = new(0f, 0f, 0f),
            [HumanBodyBones.Chest] = new(0f, 0f, 0f),
            [HumanBodyBones.Neck] = new(0f, 0f, 0f),
            [HumanBodyBones.Head] = new(0f, 0f, 0f),
            [HumanBodyBones.LeftUpperArm] = new(0f, 0f, 75f),
            [HumanBodyBones.LeftLowerArm] = new(0f, 0f, 0f),
            [HumanBodyBones.LeftHand] = new(0f, 0f, 15f),
            [HumanBodyBones.RightUpperArm] = new(0f, 0f, -75f),
            [HumanBodyBones.RightLowerArm] = new(0f, 0f, 0f),
            [HumanBodyBones.RightHand] = new(0f, 0f, -15f),
            [HumanBodyBones.LeftUpperLeg] = new(0f, 0f, 0f),
            [HumanBodyBones.LeftLowerLeg] = new(0f, 0f, 0f),
            [HumanBodyBones.LeftFoot] = new(0f, 0f, 0f),
            [HumanBodyBones.RightUpperLeg] = new(0f, 0f, 0f),
            [HumanBodyBones.RightLowerLeg] = new(0f, 0f, 0f),
            [HumanBodyBones.RightFoot] = new(0f, 0f, 0f),
        };
    }
}