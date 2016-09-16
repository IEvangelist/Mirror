using System;


namespace Mirror.Extensions
{
    public static class DoubleExtensions
    {
        static string[] Caridnals = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };

        static string[] VerboseCaridnals = { "North", "North East", "East", "South East", "South", "South West", "West", "North West", "North" };

        static DateTime EpochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static string ToCardinal(this double degrees) =>
            Caridnals[(int)Math.Round(degrees % 360 / 45)];

        public static string ToVerboseCardinal(this double degrees) =>
            VerboseCaridnals[(int)Math.Round(degrees % 360 / 45)];

        public static DateTime FromUnixTimeStamp(this double unixTimeStamp) =>
            EpochDateTime.AddSeconds(unixTimeStamp).ToLocalTime();

        public static string ToOrdinalString(this int number)
        {
            if ((number % 100 > 10 && number % 100 < 20))
            {
                return number + "th";
            }
            else
            {
                switch (number % 10)
                {
                    case 1:
                        return number + "st";

                    case 2:
                        return number + "nd";

                    case 3:
                        return number + "rd";

                    default:
                        return number + "th";
                }
            }
        }
    }
}