using System;
using Windows.ApplicationModel.Resources;


namespace Mirror.Core
{
    public class Settings
    {
        static readonly Lazy<Settings> _settings = new Lazy<Settings>(() => new Settings());

        public static Settings Instance { get; } = _settings.Value;

        public string AzureEmotionApiKey { get;  private set; }
        public string CalendarPassword { get; private set; }
        public string CalendarUsername { get; private set; }
        public string CentareCalendarUrl { get; private set; }
        public string City { get; private set; }
        public string JohnsonControlsCalendarUrl { get; private set; }
        public string OpenWeatherApiKey { get; private set; }
        public string WeatherUom { get; private set; }

        Settings()
        {
            var resourceLoader = ResourceLoader.GetForViewIndependentUse("Configuration");
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