using Mirror.Models;
using System;
using System.Threading.Tasks;
using static Mirror.Core.Settings;


namespace Mirror.Networking
{
    static class WeatherClient
    {
        const string BaseUrl = "http://api.openweathermap.org/data/2.5/";

        internal static string CurrentUrl =>
            $"{BaseUrl}weather?zip={Instance.City}&cnt=5&appid={Instance.OpenWeatherApiKey}&units={Instance.WeatherUom}&mode=json";

        internal static string ForecastUrl =>
            $"{BaseUrl}forecast/daily?zip={Instance.City}&cnt=6&appid={Instance.OpenWeatherApiKey}&units={Instance.WeatherUom}&mode=json";
        internal static Task<Current> GetCurrentAsync() =>
            OnErrorContinueAsync(() =>
                ApiClient.GetAsync<Current>(CurrentUrl));

        internal static Task<Forecast> GetForecastAsync() =>
            OnErrorContinueAsync(() =>
                ApiClient.GetAsync<Forecast>(ForecastUrl));

        static async Task<T> OnErrorContinueAsync<T>(Func<Task<T>> apiAsync)
        {
            try
            {
                return await apiAsync();
            }
            catch
            {
                return await Task.FromResult(default(T));
            }
        }
    }
}