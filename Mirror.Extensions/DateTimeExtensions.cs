using System;

namespace Mirror.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime Next(this DateTime value, DayOfWeek day)
        {
            do
            {
                value = value + TimeSpan.FromDays(1);
            }
            while (value.DayOfWeek != day);

            return value;
        }
    }
}