using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Utility.Extensions
{
    [Flags]
    public enum FixStringFormatOperation
    {
        ReplaceYekeArabi = 1,
        ToLatinNumber = 2,
        RemoveDiacritics = 4,
    }

    public static class StringExtensions
    {
        public static string FixStringFormat(this string input, FixStringFormatOperation actions = FixStringFormatOperation.ToLatinNumber)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            input = input.Trim();

            var normalizedString = input.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            var dataChars = normalizedString.ToCharArray();

            // todo: refactor --> "HasFlag check" per character is wrong
            for (var i = 0; i < dataChars.Length; i++)
            {
                // replace number
                if (actions.HasFlag(FixStringFormatOperation.ToLatinNumber))
                {
                    if (CharConstant.DigitCharMaps.ContainsKey(dataChars[i]))
                    {
                        dataChars[i] = CharConstant.DigitCharMaps[dataChars[i]];
                    }
                }

                // replace yeke
                if (actions.HasFlag(FixStringFormatOperation.ReplaceYekeArabi))
                {
                    if (CharConstant.YeKeCharMaps.ContainsKey(dataChars[i]))
                    {
                        dataChars[i] = CharConstant.YeKeCharMaps[dataChars[i]];
                    }
                }

                // remove diacritics
                if (actions.HasFlag(FixStringFormatOperation.RemoveDiacritics))
                {
                    if (CharUnicodeInfo.GetUnicodeCategory(dataChars[i]) != UnicodeCategory.NonSpacingMark)
                    {
                        stringBuilder.Append(dataChars[i]);
                    }
                }

                // default
                else
                {
                    stringBuilder.Append(dataChars[i]);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        public static T ToEnum<T>(this string value, T defaultValue) where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return Enum.TryParse<T>(value, true, out T result) ? result : defaultValue;
        }

        public static T ParseEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        // todo:refactor
        public static bool InCurrentTime(this string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value) || !value.Contains(';'))
                {
                    return false;
                }

                var times = value.Split(';');

                var startTime = TimeSpan.Parse(times[0]);
                var endTime = TimeSpan.Parse(times[1]);

                var now = DateTime.Now;

                if (now.Hour >= startTime.Hours && now.Hour <= endTime.Hours)
                {
                    if (now.Minute >= startTime.Minutes && now.Minute <= endTime.Minutes)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsMobileNumber(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            const string strRegex = "^09([0-9]{2})-?[0-9]{3}-?[0-9]{4}$";

            Regex regex = new(strRegex);
            return regex.IsMatch(value);
        }

        public static string ToUrlSlug(this string value)
        {
            // First to lower case
            value = value.ToLowerInvariant();

            value = Regex.Replace(value, @"/", "-", RegexOptions.Compiled);

            // Replace spaces
            value = Regex.Replace(value, @"\s", "-", RegexOptions.Compiled);

            // Trim dashes from end
            value = value.Trim('-', '_');

            // Replace double occurences of - or _
            value = Regex.Replace(value, "([-_]){2,}", "$1", RegexOptions.Compiled);

            return value;
        }

        public static List<string> GetWordCombinations(this string value)
        {
            List<string> output = new();

            if (string.IsNullOrEmpty(value))
            {
                return output;
            }

            string[] textPart = value.Split(" ");
            output.Add(textPart[0]);

            for (int i = 1; i < textPart.Length; i++)
            {
                var x = textPart[0] + " " + string.Join(" ", textPart[i..^0]);
                output.Add(x);
                for (int j = i + 1; j < textPart.Length; j++)
                {
                    output.Add(textPart[0] + " " + textPart[j]);
                }
            }

            var remainText = string.Join(" ", textPart[1..^0]);
            output.AddRange(remainText.GetWordCombinations());

            return output;
        }

        public static string ExtractNumber(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            var numbers = new string(input.Where(char.IsDigit).ToArray());

            if (input.Trim().StartsWith("-"))
            {
                return "-" + numbers;
            }

            return numbers;
        }
    }
}
