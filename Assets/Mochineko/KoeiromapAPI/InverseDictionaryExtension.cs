#nullable enable
using System;
using System.Collections.Generic;

namespace Mochineko.KoeiromapAPI
{
    internal static class InverseDictionaryExtension
    {
        public static T Inverse<T>(this IReadOnlyDictionary<T, string> dictionary, string key)
            where T : Enum
        {
            foreach (var pair in dictionary)
            {
                if (pair.Value == key)
                {
                    return pair.Key;
                }
            }

            throw new KeyNotFoundException(key);
        }
    }
}