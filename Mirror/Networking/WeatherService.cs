using Mirror.Core;
using Mirror.Models;
using System;
using System.Threading.Tasks;
using static Mirror.Core.Settings;


namespace Mirror.Networking
{
    public interface IWeatherService
    {
        Task<Current> GetCurrentAsync();

        Task<Forecast> GetForecastAsync();
    }

    public class WeatherService : IWeatherService
    {
        const string BaseUrl = "http://api.openweathermap.org/data/2.5/";

        static string CurrentUrl =>
            $"{BaseUrl}weather?zip={Instance.City}&cnt=5&appid={Instance.OpenWeatherApiKey}&units={Instance.WeatherUom}&mode=json";

        static string ForecastUrl =>
            $"{BaseUrl}forecast/daily?zip={Instance.City}&cnt=6&appid={Instance.OpenWeatherApiKey}&units={Instance.WeatherUom}&mode=json";

        Task<Current> IWeatherService.GetCurrentAsync() =>
            OnErrorContinueAsync(() =>
                ApiClient.GetAsync<Current>(CurrentUrl));

        Task<Forecast> IWeatherService.GetForecastAsync() =>
            OnErrorContinueAsync(() =>
                ApiClient.GetAsync<Forecast>(ForecastUrl));

        static async Task<T> OnErrorContinueAsync<T>(Func<Task<T>> apiAsync)
        {
            try
            {
                return await apiAsync();
            }
            catch (Exception ex) when (DebugHelper.IsNotHandled<WeatherService>(ex))
            {
                return await Task.FromResult(default(T));
            }
        }
    }
}