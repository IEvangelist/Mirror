using System.Text;


namespace Mirror.Extensions
{
    static class StringExtension
    {
        internal static string ToTitleCase(this string str) => ToTitleCaseIpml(str);

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
    }
}