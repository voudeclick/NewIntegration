using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace VDC.Integration.Domain.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveAccents(this string text)
        {
            var sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (var letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }

        public static bool IsMatchRegex(this string text, string pattern)
        {
            var regexOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
            return Regex.IsMatch(text.RemoveAccents(), pattern.RemoveAccents(), regexOptions);
        }
    }
}
