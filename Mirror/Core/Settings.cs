using Mirror.Extensions;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;


namespace Mirror.Core
{
    public class Settings
    {
        const string Configuration = nameof(Configuration);
        static readonly Lazy<Settings> _settings = new Lazy<Settings>(() => new Settings());

        public static Settings Instance { get; } = _settings.Value;

        public string AzureEmotionApiKey { get; private set; }
        public string City { get; private set; }
        public string OpenWeatherApiKey { get; private set; }
        public string WeatherUom { get; private set; }

        public List<CalendarConfig> Calendars { get; private set; }

        Settings()
        {
            var resourceLoader = ResourceLoader.GetForViewIndependentUse(Configuration);
            AzureEmotionApiKey = resourceLoader.GetString(nameof(AzureEmotionApiKey));
            City = resourceLoader.GetString(nameof(City));
            OpenWeatherApiKey = resourceLoader.GetString(nameof(OpenWeatherApiKey));
            WeatherUom = resourceLoader.GetString(nameof(WeatherUom));
            Calendars = resourceLoader.GetString(nameof(Calendars)).Deserialize<List<CalendarConfig>>();
        }
    }
    
    public class CalendarConfig
    {
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsUsingCredentials 
            => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
    }
}