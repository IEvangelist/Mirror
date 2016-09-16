using Mirror.Core;
using Mirror.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace Mirror.Calendar
{
    public class Content
    {
        public string Name { get; private set; }
        public string Value { get; private set; }
        public Dictionary<string, List<string>> Parameters { get; } = new Dictionary<string, List<string>>();

        public Content(string contentline)
        {
            contentline = contentline.Trim();
            Name = Regex.Match(contentline, @"(.*?)[;:]").Groups[1].Value;
            Value = Regex.Match(contentline, @".*?:(.*(\n\s.*)*)").Groups[1].Value;

            foreach (Match paramValue in Regex.Matches(contentline, @"^.*?;(.*:)"))
            {
                foreach (Match paramValueSplit in Regex.Matches(paramValue.Groups[1].Value, @"(.+?)=(.+?)[;:]"))
                {
                    Parameters.Add(paramValueSplit.Groups[1].Value, paramValueSplit.Groups[2].Value.Split(',').ToList());
                }
            }
        }

        public bool HasParameterAndValue(string key, string value)
        {
            try
            {
                return Parameters[key].Contains(value);
            }
            catch (Exception ex) when (DebugHelper.IsHandled<Content>(ex))
            {
                return false;
            }
        }

        public override string ToString()
        {
            var replaced =
                Regex.Replace(
                    Value.Replace(Environment.NewLine + "\t", string.Empty)
                         .Replace(Environment.NewLine + " ", string.Empty)
                         .Replace(Environment.NewLine + "	", string.Empty)
                         .Replace(@"\n\r", Environment.NewLine)
                         .Replace(@"\n", Environment.NewLine)
                         .Replace(@"\r", Environment.NewLine), @"\\(.)", "$1")
                         .Trim();

            return Regex.Unescape(replaced);
        }
    }
}