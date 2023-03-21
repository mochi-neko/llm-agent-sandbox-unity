#nullable enable
namespace Mochineko.LLMAgent.Emotion
{
    public static class PromptTemplate
    {
        public const string ChatAgentEmotionAnalysisTemplate =
            "Please output your emotion and message as a JSON object with keys:" +
            " emotion and message." +
            " The emotion value should be another JSON object with keys:" +
            " happiness, sadness, anger, fear, surprise and disgust." +
            " Each value should be a number between 0 and 1." +
            " The message value should be a string.";
    }
}