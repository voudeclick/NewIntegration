using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace VDC.Integration.Application.Tools
{
    public static class StringExtensions
    {
        public static string CleanDocument(this string document)
        {
            return document.Replace(".", "").Replace("-", "").Replace("/", "").Trim();
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static string HtmlDecode(this string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return System.Net.WebUtility.HtmlDecode(value);
        }
        public static string Capitalize(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            input = input.ToLower();
            return input.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) +
                input.Substring(1, input.Length - 1);
        }
        public static string ToHashMD5(this string input)
        {
            if (string.IsNullOrEmpty(input))
                input = $"{new Random().Next(int.MinValue, int.MaxValue)}";

            var bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();

        }

        public static string IfIsNullOrWhiteSpace(this string str, string value = "") => string.IsNullOrWhiteSpace(str) ? value : str;
    }
}
