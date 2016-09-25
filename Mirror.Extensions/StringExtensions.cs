using Newtonsoft.Json;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Mirror.Extensions
{
    public static class StringExtensions
    {
        static char[] EscapeSequences = new char[] { ' ', '\n', '\t', '\r', '\f', '\v', '\\' };

        public static string ToTitleCase(this string str) => ToTitleCaseIpml(str);

        static string ToTitleCaseIpml(string value)
        {
            if (value == null) return null;
            if (value.Length == 0) return value;

            var result = new StringBuilder(value);
            result[0] = char.ToUpper(result[0]);
            for (int i = 1; i < result.Length; ++ i)
            {
                if (char.IsWhiteSpace(result[i - 1]))
                {
                    result[i] = char.ToUpper(result[i]);
                }
                else
                {
                    result[i] = char.ToLower(result[i]);
                }
            }

            return result.ToString();
        }

        public static bool ContainsIgnoringCase(this string value, string search)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(search))
            {
                return false;
            }

            return value.IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1;
        }

        public static string NullIfEmpty(this string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value;        

        public static string Coalesce(this string root, params string[] others)
        {
            string value = root.NullIfEmpty();
            if (value != null) return value;

            for (var i = 0; i < others.Length; ++ i)
            {
                value = others[i].NullIfEmpty();
                if (value != null) return value;
            }

            return string.Empty;
        }

        public static string SplitCamelCase(this string value)
            => Regex.Replace(value, "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))", "$1 ");

        public static T ToEnum<T>(this string value) => (T)Enum.Parse(typeof(T), value);

        public static T Deserialize<T>(this string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch
            {
                return default(T);
            }
        }
    }
}