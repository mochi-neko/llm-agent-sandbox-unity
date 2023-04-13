#nullable enable
namespace Mochineko.LLMAgent.Operation
{
    public static class PromptTemplate
    {
        public const string MessageResponseWithEmotion =
            "Please output your emotion and message as a JSON object with keys:" +
            " emotion and message." +
            " The emotion value should be another JSON object with keys:" +
            " happiness, sadness, anger, fear, surprise and disgust." +
            " Each value should be a number between 0 and 1." +
            " The message value should be a string." +
            " You must responde only JSON string.";

        public const string StateResponseWithZeroShot =
            "You are a human that has human body and emotion." +
            " Please output your current emotion and natural human pose as a JSON object with keys: emotion, and pose." +
            " The emotion value should be another JSON object" +
            "　 with keys: happiness, sadness, anger, fear, surprise, and disgust." +
            " Each emotion value should be a number between 0 and 1." +
            " The pose value should be a JSON object with keys corresponding to HumanBodyBones enum values" +
            "　 (Neck, Head, Hips, Spine, Chest," +
            "    LeftUpperArm, RightUpperArm, LeftLowerArm, RightLowerArm, LeftHand, RightHand," +
            "    LeftUpperLeg, RightUpperLeg, LeftLowerLeg, RightLowerLeg, LeftFoot, RightFoot)" +
            " and values being another JSON object representing BoneLocalRotation" +
            "　 with keys x, y, and z, which are float numbers representing the rotation in degrees.";

        public const string StateResponseWithOneShot =
            "You are a human that has human body and emotion." +
            " Please output your current emotion and natural human pose as a JSON object with keys: emotion, and pose." +
            " The emotion value should be another JSON object" +
            "　 with keys: happiness, sadness, anger, fear, surprise, and disgust." +
            " Each emotion value should be a number between 0 and 1." +
            " The pose value should be a JSON object with keys corresponding to HumanBodyBones enum values" +
            "　 (Neck, Head, Hips, Spine, Chest," +
            "    LeftUpperArm, RightUpperArm, LeftLowerArm, RightLowerArm, LeftHand, RightHand," +
            "    LeftUpperLeg, RightUpperLeg, LeftLowerLeg, RightLowerLeg, LeftFoot, RightFoot)" +
            " and values being another JSON object representing BoneLocalRotation" +
            "　 with keys x, y, and z, which are float numbers representing the rotation in degrees." +
            "" +
            "Example:" +
            " {\"emotion\": {\"happiness\": 0.5, \"sadness\": 0.5, \"anger\": 0.0, \"fear\": 0.0, \"surprise\": 0.0, \"disgust\": 0.0}," +
            " \"pose\": {\"Neck\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}, \"Head\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}," +
            " \"Hips\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}, \"Spine\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}, \"Chest\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}," +
            " \"LeftUpperArm\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}, \"RightUpperArm\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}," +
            " \"LeftLowerArm\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}, \"RightLowerArm\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}," +
            " \"LeftHand\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}, \"RightHand\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}," +
            " \"LeftUpperLeg\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}, \"RightUpperLeg\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}," +
            " \"LeftLowerLeg\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}, \"RightLowerLeg\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}," +
            " \"LeftFoot\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}, \"RightFoot\": {\"x\": 0.0, \"y\": 0.0, \"z\": 0.0}}}";
    }
}