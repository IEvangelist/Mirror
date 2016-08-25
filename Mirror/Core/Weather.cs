using System;
using System.Collections.Generic;


namespace Mirror.Core
{
    public enum KnownIcons
    {
        SunRiseOrSet = 'A',
        Eclipse = 'D',
        Wind = 'F',
        Thermometer = '\'',
        Compass = '(',
        NotApplicable = ')',
        Celsius = '*',
        Fahrenheit = '+'
    };

    /// <summary>
    /// Weather icons represent character constants that map to the "Meteocons" font, http://www.alessioatzeni.com/meteocons/.
    /// The <see cref="Icons"/> dictionary maps http://openweathermap.org/weather-conditions to icons.
    /// </summary>
    public class Weather
    {
        public static Map Icons => new Map();

        public class Map
        {
            Dictionary<string, char> Lookup =>
                new Dictionary<string, char>(StringComparer.OrdinalIgnoreCase)
                {
                    // Day            Night
                    { "01d", 'B' }, { "01n", 'C' },
                    { "02d", 'H' }, { "02n", 'I' },
                    { "03d", 'N' }, { "03n", 'N' },
                    { "04d", 'Y' }, { "04n", 'Y' },
                    { "09d", 'R' }, { "09n", 'R' },
                    { "10d", 'Q' }, { "10n", 'Q' },
                    { "11d", '0' }, { "11n", '0' },
                    { "13d", 'W' }, { "13n", 'W' },
                    { "50d", 'J' }, { "50n", 'K' }
                };

            public string this[string key] => Lookup[key].ToString();
        }
    }
}