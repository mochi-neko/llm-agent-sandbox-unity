#nullable enable
using System.Collections.Generic;

namespace Mochineko.KoeiromapAPI
{
    internal static class StyleResolver
    {
        private static readonly IReadOnlyDictionary<Style, string> Dictionary = new Dictionary<Style, string>
        {
            [Style.Talk] = "talk",
            [Style.Happy] = "happy",
            [Style.Sad] = "sad",
            [Style.Angry] = "angry",
            [Style.Fear] = "fear",
            [Style.Surprised] = "surprised",
        };

        public static Style ToStyle(this string style)
            => Dictionary.Inverse(style);

        public static string ToText(this Style style)
            => Dictionary[style];
    }
}