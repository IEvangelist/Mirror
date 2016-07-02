using System;
using System.Collections.Generic;


namespace Mirror.Core
{
    internal enum Icons
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
    /// The <see cref="WeatherIcons.Map"/> dictionary maps http://openweathermap.org/weather-conditions to icons.
    /// </summary>
    class WeatherIcons
    {
        internal static Dictionary<string, char> Map =
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
    }
}