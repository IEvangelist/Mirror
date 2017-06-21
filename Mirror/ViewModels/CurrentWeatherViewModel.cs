using System;
using System.Linq;
using Mirror.Core;
using Mirror.Extensions;
using Windows.UI.Xaml;

namespace Mirror.ViewModels
{
    public class CurrentWeatherViewModel : BaseViewModel
    {
        bool _isSunrise;

        Models.Current _currentWeather;
        Models.Weather _weather;

        public double Temp => _currentWeather.Main.Temp;

        public double TempLow => _currentWeather.Main.TempMin;

        public double TempHigh => _currentWeather.Main.TempMax;

        public string Wind => $"{_currentWeather.Wind.Speed:#} {_currentWeather.Wind.Deg.ToCardinal()}";

        public string WindVerbose => $"{_currentWeather.Wind.Speed:#} MPH to the {_currentWeather.Wind.Deg.ToVerboseCardinal()}";

        public string SunRiseOrSet { get; private set; }

        public string Conditions => _weather.Description.ToTitleCase();

        public string Location => _currentWeather.Name;

        public int Icon => Weather.ImageIcons[_weather.Icon];

        public CurrentWeatherViewModel(DependencyObject dependency, Models.Current currentWeather) : base(dependency)
        {
            _currentWeather = currentWeather;
            _weather = _currentWeather.Weather.FirstOrDefault();

            DateTime? sunrise = _currentWeather.Sys.SunriseDateTime,
                      sunset = _currentWeather.Sys.SunsetDateTime;

            _isSunrise = DateTime.Now > sunset;
            SunRiseOrSet =
                _isSunrise
                ? $"{sunrise:h:mm tt}"
                : $"{sunset:h:mm tt}";
        }

        public override string ToFormattedString(DateTime? dateContext)
        {
            var sunriseOrSunset = _isSunrise ? "sunrise" : "sunset";
            return $"It's currently {Temp:#}° right now in {Location}, with a low of {TempLow:#}° and a high of {TempHigh:#}°. " +
                   $"If you were to look outside you'd notice some {Conditions}. Winds are blowing {WindVerbose}, and you can expect {sunriseOrSunset} at {SunRiseOrSet}.";
        }
    }
}