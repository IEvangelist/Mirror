using System;
using Windows.ApplicationModel.Resources;


namespace Mirror.Core
{
    class Settings
    {
        private static readonly Lazy<Settings> _settings = new Lazy<Settings>(() => new Settings());

        internal static Settings Instance { get; } = _settings.Value;

        internal string AzureEmotionApiKey { get;  private set; }
        internal string CalendarPassword { get; private set; }
        internal string CalendarUsername { get; private set; }
        internal string CentareCalendarUrl { get; private set; }
        internal string City { get; private set; }
        internal string JohnsonControlsCalendarUrl { get; private set; }
        internal string OpenWeatherApiKey { get; private set; }
        internal string WeatherUom { get; private set; }

        Settings()
        {
            var resourceLoader = ResourceLoader.GetForViewIndependentUse();
            AzureEmotionApiKey = resourceLoader.GetString(nameof(AzureEmotionApiKey));
            CalendarPassword = resourceLoader.GetString(nameof(CalendarPassword));
            CalendarUsername = resourceLoader.GetString(nameof(CalendarUsername));
            CentareCalendarUrl = resourceLoader.GetString(nameof(CentareCalendarUrl));
            City = resourceLoader.GetString(nameof(City));
            JohnsonControlsCalendarUrl = resourceLoader.GetString(nameof(JohnsonControlsCalendarUrl));
            OpenWeatherApiKey = resourceLoader.GetString(nameof(OpenWeatherApiKey));
            WeatherUom = resourceLoader.GetString(nameof(WeatherUom));
        }
    }
}