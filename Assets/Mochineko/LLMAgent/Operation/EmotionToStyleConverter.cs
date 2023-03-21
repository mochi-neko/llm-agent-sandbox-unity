#nullable enable
using Mochineko.KoeiromapAPI;

namespace Mochineko.LLMAgent.Operation
{
    internal static class EmotionToStyleConverter
    {
        public static Style ExcludeHighestEmotionStyle(
            Emotion.Emotion emotion,
            float threshold = 0.5f)
        {
            var style = Style.Talk;
            var hightestEmotion = threshold;
            
            if (hightestEmotion < emotion.Happiness)
            {
                style = Style.Happy;
                hightestEmotion = emotion.Happiness;
            }
            
            if (hightestEmotion < emotion.Sadness)
            {
                style = Style.Sad;
                hightestEmotion = emotion.Sadness;
            }
            
            if (hightestEmotion < emotion.Anger)
            {
                style = Style.Angry;
                hightestEmotion = emotion.Anger;
            }
            
            if (hightestEmotion < emotion.Fear)
            {
                style = Style.Fear;
                hightestEmotion = emotion.Fear;
            }
            
            if (hightestEmotion < emotion.Surprise)
            {
                style = Style.Surprised;
                hightestEmotion = emotion.Surprise;
            }
            
            // NOTE: Disgust is not supported.
            // if (hightestEmotion < emotion.Disgust)
            // {
            //     style = Style.Angry;
            // }

            return style;
        }
    }
}