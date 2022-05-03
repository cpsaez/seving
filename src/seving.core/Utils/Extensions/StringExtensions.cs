using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Lendsum.Crosscutting.Common.Extensions
{
    /// <summary>
    /// Class to group the extensions for the string date type
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Do the string.Format with the invariant culture inline.
        /// </summary>
        /// <param name="origin">The origin string with the parameters encapsulated with brackets.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>The string formated</returns>
        public static string InvariantFormat(this string origin, params object[] arguments)
        {
            return string.Format(CultureInfo.InvariantCulture, origin, arguments);
        }

        /// <summary>
        /// Converts from base64.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public static string ConvertFromBase64(this string origin, Encoding? encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bytes = System.Convert.FromBase64String(origin);
            var result = encoding.GetString(bytes);
            return result;
        }

        /// <summary>
        /// Converts to base64.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="encoding">The encoding.</param>
        /// <returns></returns>
        public static string ConvertToBase64(this string origin, Encoding? encoding = null)
        {
            if (encoding == null) encoding = Encoding.UTF8;
            var bytes = encoding.GetBytes(origin);
            var result = System.Convert.ToBase64String(bytes);
            return result;
        }

        /// <summary>
        /// Determines if value contains any number
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static bool ContainsAnyNumber(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var numbers = "1234567890".ToCharArray();
            if (value.IndexOfAny(numbers) == -1) return false;
            return true;
        }

        /// <summary>
        /// Removes the numbers contained in the string
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The value without numbers</returns>
        public static string RemoveNumbers(this string value)
        {
            var numbers = "1234567890".ToCharArray();
            var result = value;
            foreach (var number in numbers)
            {
                result = result.Replace(number.ToString(CultureInfo.InvariantCulture), "");
            }

            return result;
        }

        /// <summary>
        /// Gets the numbers.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string GetNumbers(this string value)
        {
            value = Regex.Replace(value, "[^0-9]+", string.Empty);
            return value;
        }

        /// <summary>
        /// Removes the diacritics.
        /// </summary>
        /// <param name="value">The string.</param>
        /// <returns></returns>
        public static string RemoveDiacritics(this string value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var normalizedString = value.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        /// <summary>
        /// REturn the value without diacritics and in lower characters and without the spaces and the begining and at the end.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ToUpperInvariantWithoutDiacritics(this string value)
        {
            return value.RemoveDiacritics().ToUpperInvariant().Trim();
        }

        /// <summary>
        /// To the URL encode.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
        public static string ToUrlEncode(this string value)
        {
            return HttpUtility.UrlEncode(value);
        }

        /// <summary>
        /// Returns only the alfanumeric characters in the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string OnlyAlfaNumberic(this string value)
        {
            var allowed = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            var arrayResult = value.RemoveDiacritics().ToCharArray().Where(x => allowed.Contains(x)).ToArray();
            return new string(arrayResult);
        }

        /// <summary>
        /// Removes the weird character.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string RemoveWeirdChar(this string value)
        {
            if (value == null) return "";

            var allowed = "aàábcdeèéfghiìíjklmnñoòópqrstuùúvwxyzAÀÁBCDEÈÉFGHIÌÍJKLMNÑOÒÓPQRSTUÙÚVWXYZ1234567890 ".ToCharArray();
            var arrayResult = value.ToCharArray().Where(x => allowed.Contains(x)).ToArray();
            return new string(arrayResult);
        }

        /// <summary>
        /// Removes the weird character.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="charsAllowed">The chars allowed.</param>
        /// <returns></returns>
        public static string RemoveWeirdCharExceptSomeChars(this string value, string charsAllowed)
        {
            if (value == null) return "";

            var allowed = (charsAllowed + ("aàábcdeèéfghiìíjklmnñoòópqrstuùúvwxyzAÀÁBCDEÈÉFGHIÌÍJKLMNÑOÒÓPQRSTUÙÚVWXYZ1234567890 ")).ToCharArray();
            var arrayResult = value.ToCharArray().Where(x => allowed.Contains(x)).ToArray();
            return new string(arrayResult);
        }
        
        /// <summary>
        /// Converts the string representation of a number to an integer.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static int? ToInt(this string str)
        {
            if (str == null)
                return null;

            str = Regex.Replace(str, @"[^0-9,.-]+", "");
            int result;
            return int.TryParse(str, out result) ?
                result : (int?)null;
        }

        /// <summary>
        /// Converts a set of string in a sentence concatenating them in one string separating the items by a space.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="wordBetween">The word between the values passed</param>
        /// <returns></returns>
        public static string ToSentence(this IEnumerable<string> value, string? wordBetween = null)
        {
            if (value == null || value.Count() == 0) return string.Empty;
            if (wordBetween == null) wordBetween = " ";
            StringBuilder builder = new StringBuilder();
            bool firstOne = true;
            foreach (var word in value)
            {
                if (firstOne)
                {
                    firstOne = false;
                }
                else
                {
                    builder.Append(wordBetween);
                }

                builder.Append(word.Trim());
            }

            return builder.ToString().Trim();
        }

        /// <summary>
        /// Capitalizes the first letters.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string CapitalizeFirstLetters(this string value)
        {
            if (value == null) return string.Empty;
            if (value == "") return "";
            
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());
        }
    }
}