using System;


namespace Mirror.Extensions
{
    static class DoubleExtensions
    {
        static string[] Caridnals = { "N", "NE", "E", "SE", "S", "SW", "W", "NW", "N" };

        static DateTime EpochDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        internal static string ToCardinal(this double degrees) =>
            Caridnals[(int)Math.Round(((double)degrees % 360) / 45)];

        internal static DateTime FromUnixTimeStamp(this double unixTimeStamp) =>
            EpochDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
    }
}