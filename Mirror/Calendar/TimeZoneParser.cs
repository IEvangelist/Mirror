using System;
using System.Collections.Generic;


namespace Mirror.Calendar
{
    class TimeZoneParser
    {
        const string TimeZoneId = "TZID";

        internal static TimeZoneInfo Parse(Dictionary<string, List<string>> parameters)
        {
            if (parameters == null)
            {
                return null;
            }

            if (parameters.ContainsKey(TimeZoneId) && parameters[TimeZoneId].Count == 1)
            {
                var scrubbedValue = ScrubTimeZone(parameters[TimeZoneId][0]);
                return TimeZoneInfo.FindSystemTimeZoneById(scrubbedValue);
            }

            return null;
        }

        static string ScrubTimeZone(string timezone)
        {
            var value =
                timezone.Replace("\"", string.Empty)
                        .Replace("(", string.Empty)
                        .Replace(")", string.Empty);

            return value.Contains("-")
                ? value.Substring(0, 3)
                : value;
        }                       
    }
}