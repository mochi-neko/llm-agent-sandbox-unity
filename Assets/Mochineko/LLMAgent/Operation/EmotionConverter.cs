#nullable enable
using Mochineko.KoeiromapAPI;

namespace Mochineko.LLMAgent.Operation
{
    internal static class EmotionConverter
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
        
        public static FacialExpressions.Emotion.Emotion ExcludeHighestEmotion(
            Emotion.Emotion emotion,
            float threshold = 0.5f)
        {
            var emotionEnum = FacialExpressions.Emotion.Emotion.Neutral;
            var hightestEmotion = threshold;
            
            if (hightestEmotion < emotion.Happiness)
            {
                emotionEnum = FacialExpressions.Emotion.Emotion.Happy;
                hightestEmotion = emotion.Happiness;
            }
            
            if (hightestEmotion < emotion.Sadness)
            {
                emotionEnum = FacialExpressions.Emotion.Emotion.Sad;
                hightestEmotion = emotion.Sadness;
            }
            
            if (hightestEmotion < emotion.Anger)
            {
                emotionEnum = FacialExpressions.Emotion.Emotion.Angry;
                hightestEmotion = emotion.Anger;
            }
            
            if (hightestEmotion < emotion.Fear)
            {
                emotionEnum = FacialExpressions.Emotion.Emotion.Fear;
                hightestEmotion = emotion.Fear;
            }
            
            if (hightestEmotion < emotion.Surprise)
            {
                emotionEnum = FacialExpressions.Emotion.Emotion.Surprised;
                hightestEmotion = emotion.Surprise;
            }
            
            if (hightestEmotion < emotion.Disgust)
            {
                emotionEnum = FacialExpressions.Emotion.Emotion.Disgusted;
            }

            return emotionEnum;
        }
    }
}