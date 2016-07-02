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
                return TimeZoneInfo.FindSystemTimeZoneById(parameters[TimeZoneId][0]);
            }

            return null;
        }
    }
}