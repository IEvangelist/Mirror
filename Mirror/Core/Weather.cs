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

        public static ImageMap ImageIcons => new ImageMap();

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

        public class ImageMap
        {
            Dictionary<string, int> ImageLookup =>
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    // Day            Night
                    { "01d",  1 }, { "01n", 33 },
                    { "02d",  3 }, { "02n", 35 },
                    { "03d",  6 }, { "03n", 38 },
                    { "04d",  7 }, { "04n",  8 },
                    { "09d", 12 }, { "09n", 12 },
                    { "10d", 13 }, { "10n", 39 },
                    { "11d", 15 }, { "11n", 41 },
                    { "13d", 22 }, { "13n", 44 },
                    { "50d",  5 }, { "50n", 35 }
                };

            public int this[string key] => ImageLookup[key];
        }
    }
}