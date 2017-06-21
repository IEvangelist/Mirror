using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mirror.Core;
using Mirror.Extensions;
using Windows.UI.Xaml;

namespace Mirror.ViewModels
{
    public class ForecastViewModel : BaseViewModel
    {
        Models.Forecast _forecast;

        public List<ForecastDay> Days { get; private set; }

        public ForecastViewModel(DependencyObject dependency, Models.Forecast forecast) : base(dependency)
        {
            _forecast = forecast;
            if (_forecast.Cnt > 0)
            {
                var now = DateTime.Now;
                Days = 
                    _forecast.List
                             .Where(listing => listing.DateTime.Date > now.Date)
                             .Select(listing => new ForecastDay(dependency, listing, listing.Weather[0]))
                             .ToList();
            }
        }

        public override string ToFormattedString(DateTime? dateContext)
        {
            var builder = new StringBuilder();
            foreach (var day in Days.Where(forecastDay => 
                                           dateContext.HasValue 
                                               ? forecastDay.DateTime.Date == dateContext.Value.Date
                                               : true))
            {
                builder.AppendLine(day.ToFormattedString(dateContext));
            }

            return builder.ToString();
        }
    }

    public class ForecastDay : BaseViewModel
    {
        Models.List _listing;
        Models.Weather _weather;

        public DateTime DateTime => _listing.DateTime;

        public string Day => $"{_listing.DateTime:ddd}";

        public string FullDay => $"{_listing.DateTime:dddd}";

        public string Conditions => $"{_weather.Description}";

        public string Icon => Weather.Icons[_weather.Icon];

        public double Low => _listing.Temp.Min;

        public double High => _listing.Temp.Max;

        public ForecastDay(DependencyObject dependency, Models.List listing, Models.Weather weather) : base(dependency)
        {
            _listing = listing;
            _weather = weather;
        }

        public override string ToFormattedString(DateTime? dateContext) 
            => $"On {_listing.DateTime:dddd}, the {_listing.DateTime.Day.ToOrdinalString()} expect {Conditions} with a low of {_listing.Temp.Min:#}° and a high of {_listing.Temp.Max:#}°.";
    }
}