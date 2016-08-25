using System;
using System.Text.RegularExpressions;


namespace Mirror.Calendar
{
   public class DateParser
    {
        const string DateTimeExpression =
            "^((\\d{4})(\\d{2})(\\d{2}))T((\\d{2})(\\d{2})(\\d{2})(Z)?)$";

        const string DateExpression =
            "^((\\d{4})(\\d{2})(\\d{2}))?$";

        public static DateTime? Parse(string text, TimeZoneInfo timeZone = null)
        {
            var match = Regex.Match(text, DateTimeExpression, RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                match = Regex.Match(text, DateExpression, RegexOptions.IgnoreCase);
            }

            if (!match.Success)
            {
                return null;
            }

            var now = DateTime.Now;
            int count = match.Groups.Count,
                year = now.Year,
                month = now.Month,
                day = now.Day,
                hour = 0,
                minute = 0,
                second = 0;

            if (match.Groups[1].Success)
            {
                year = Convert.ToInt32(match.Groups[2].Value);
                month = Convert.ToInt32(match.Groups[3].Value);
                day = Convert.ToInt32(match.Groups[4].Value);
            }

            if (count >= 9 && match.Groups[5].Success)
            {
                hour = Convert.ToInt32(match.Groups[6].Value);
                minute = Convert.ToInt32(match.Groups[7].Value);
                second = Convert.ToInt32(match.Groups[8].Value);
            }
            var isUtc = count == 10 && match.Groups[9].Success;
            return CoerceDateTime(year, 
                                  month,
                                  day, 
                                  hour, 
                                  minute, 
                                  second, 
                                  isUtc ? DateTimeKind.Utc : DateTimeKind.Local,
                                  timeZone);
        }

        static DateTime CoerceDateTime(int year, 
                                       int month, 
                                       int day, 
                                       int hour, 
                                       int minute, 
                                       int second, 
                                       DateTimeKind kind, 
                                       TimeZoneInfo timeZone = null)
        {
            var result = DateTime.MinValue;
            try
            {
                if (year > 9999)
                {
                    result = DateTime.MaxValue;
                }
                else if (year > 0)
                {
                    result = new DateTime(year, month, day, hour, minute, second, kind);
                }
            }
            catch
            {
                // Do nothing...
            }

            return timeZone == null
                ? result
                : TimeZoneInfo.ConvertTime(result, timeZone);
        }
    }
}