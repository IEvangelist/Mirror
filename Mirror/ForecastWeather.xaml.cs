using Mirror.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Mirror.Networking;
using System;
using System.Threading.Tasks;

namespace Mirror
{
    public sealed partial class ForecastWeather : UserControl
    {
        DispatcherTimer _timer = new DispatcherTimer();

        public ForecastWeather()
        {
            InitializeComponent();
        }

        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await LoadForecastAsync();

            _timer.Stop();
            _timer.Interval = TimeSpan.FromHours(1);
            _timer.Tick -= OnTimerTick;
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        async void OnTimerTick(object sender, object e) => await LoadForecastAsync();

        async Task LoadForecastAsync()
        {
            _forecastGrid.Opacity = 0;

            var forecast = await WeatherClient.GetForecastAsync();
            if (forecast != null && forecast.Cnt == 6)
            {
                for (int index = 0; index < forecast.List.Count; ++index)
                {
                    var value = forecast.List[index];
                    var weather = value.Weather[0];
                    switch (index)
                    {
                        case 1:
                            UpdateControls(_oneDay, _oneIcon, _oneLow, _oneHigh, value, weather);
                            break;
                        case 2:
                            UpdateControls(_twoDay, _twoIcon, _twoLow, _twoHigh, value, weather);
                            break;
                        case 3:
                            UpdateControls(_threeDay, _threeIcon, _threeLow, _threeHigh, value, weather);
                            break;
                        case 4:
                            UpdateControls(_fourDay, _fourIcon, _fourLow, _fourHigh, value, weather);
                            break;
                        case 5:
                            UpdateControls(_fiveDay, _fiveIcon, _fiveLow, _fiveHigh, value, weather);
                            break;
                    }
                }

                _fadeIn.Begin();
            }
        }

        void UpdateControls(TextBlock day, 
                            TextBlock icon, 
                            TextBlock low, 
                            TextBlock high, 
                            Models.List value, 
                            Models.Weather weather)
        {
            day.Text = $"{value.DateTime:ddd}";
            icon.Text = Weather.Icons[weather.Icon];
            low.Text = $"L {value.Temp.Min:#}°";
            high.Text = $"H {value.Temp.Max:#}°";
        }
    }
}