﻿using static System.StringComparison;

namespace Wordo.Core
{
    public static class ExtensionMethods
    {
        public static bool IsNotNullEmptyOrWhiteSpace(this string text)
        {
            return !string.IsNullOrWhiteSpace(text);
        }

        public static bool Matches(this string text, string comparisonText)
        {
            return text.Equals(comparisonText, InvariantCultureIgnoreCase);
        }

        public static string RandomElement(this List<string> options)
        {
            return options.None()
                ? ""
                : options[RngCreator.GetNumberBetween(0, options.Count - 1)];
        }

        public static bool None<T>(this IEnumerable<T> elements, Func<T, bool> func = null)
        {
            return !elements.Any(func.Invoke);
        }
    }
}