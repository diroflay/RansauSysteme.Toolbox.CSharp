using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace RansauSysteme.Utils
{
    /// <summary>
    /// Provides utility methods for string manipulation and formatting.
    /// </summary>
    public static class StringUtils
    {
        /// <summary>
        /// Converts a string to snake_case format.
        /// </summary>
        /// <param name="text">The string to convert.</param>
        /// <returns>A string in snake_case format.</returns>
        public static string ToSnakeCase(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var result = new StringBuilder();
            result.Append(char.ToLowerInvariant(text[0]));

            for (int i = 1; i < text.Length; i++)
            {
                var currentChar = text[i];
                if (char.IsUpper(currentChar))
                {
                    result.Append('_');
                    result.Append(char.ToLowerInvariant(currentChar));
                }
                else
                {
                    result.Append(currentChar);
                }
            }
            return result.ToString();
        }

        /// <summary>
        /// Capitalizes the first letter of each word in a string.
        /// </summary>
        /// <param name="word">The string to capitalize.</param>
        /// <returns>A string with the first letter of each word capitalized.</returns>
        public static string Capitalize(this string word) => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word.ToLower());

        /// <summary>
        /// Converts a string to camelCase format.
        /// </summary>
        /// <param name="text">The string to convert.</param>
        /// <returns>A string in camelCase format.</returns>
        public static string ToCamelCase(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Trim();
            if (text.Length < 2)
                return text.ToLowerInvariant();

            // If input is already snake_case, convert from that
            if (text.Contains('_'))
            {
                var parts = text.Split('_', StringSplitOptions.RemoveEmptyEntries);
                var result = new StringBuilder(parts[0].ToLowerInvariant());

                for (int i = 1; i < parts.Length; i++)
                {
                    if (parts[i].Length > 0)
                    {
                        result.Append(char.ToUpperInvariant(parts[i][0]));
                        if (parts[i].Length > 1)
                            result.Append(parts[i].Substring(1).ToLowerInvariant());
                    }
                }

                return result.ToString();
            }

            // Otherwise just make first char lowercase
            return char.ToLowerInvariant(text[0]) + text.Substring(1);
        }

        /// <summary>
        /// Truncates a string to the specified maximum length.
        /// </summary>
        /// <param name="text">The string to truncate.</param>
        /// <param name="maxLength">The maximum length of the returned string.</param>
        /// <param name="suffix">An optional suffix to append when truncation occurs.</param>
        /// <returns>The truncated string.</returns>
        public static string Truncate(this string text, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            int truncatedLength = maxLength - suffix.Length;
            if (truncatedLength <= 0)
                return suffix;

            return text.Substring(0, truncatedLength) + suffix;
        }

        /// <summary>
        /// Removes all whitespace characters from a string.
        /// </summary>
        /// <param name="text">The string to remove whitespace from.</param>
        /// <returns>A string with all whitespace removed.</returns>
        public static string RemoveWhitespace(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return string.Concat(text.Where(c => !char.IsWhiteSpace(c)));
        }

        /// <summary>
        /// Determines whether a string contains only alphanumeric characters.
        /// </summary>
        /// <param name="text">The string to check.</param>
        /// <returns>True if the string contains only alphanumeric characters; otherwise, false.</returns>
        public static bool IsAlphanumeric(this string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return text.All(char.IsLetterOrDigit);
        }
    }
}