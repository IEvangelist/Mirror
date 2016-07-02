using Mirror.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Mirror.Networking;


namespace Mirror
{
    public sealed partial class ForecastWeather : UserControl
    {
        public ForecastWeather()
        {
            InitializeComponent();
        }

        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            _forecastGrid.Opacity = 0;

            var forecast = await WeatherClient.GetForecastAsync();
            if (forecast != null && forecast.Cnt == 6)
            {
                for (int index = 0; index < forecast.List.Count; ++ index)
                {
                    var value = forecast.List[index];
                    var weateher = value.Weather[0];
                    switch (index)
                    {
                        case 1:
                            _oneDay.Text = $"{value.DateTime:ddd}";
                            _oneIcon.Text = WeatherIcons.Map[weateher.Icon].ToString();
                            _oneLow.Text = $"L {value.Temp.Min:#}°";
                            _oneHigh.Text = $"H {value.Temp.Max:#}°";
                            break;
                        case 2:
                            _twoDay.Text = $"{value.DateTime:ddd}";
                            _twoIcon.Text = WeatherIcons.Map[weateher.Icon].ToString();
                            _twoLow.Text = $"L {value.Temp.Min:#}°";
                            _twoHigh.Text = $"H {value.Temp.Max:#}°";
                            break;
                        case 3:
                            _threeDay.Text = $"{value.DateTime:ddd}";
                            _threeIcon.Text = WeatherIcons.Map[weateher.Icon].ToString();
                            _threeLow.Text = $"L {value.Temp.Min:#}°";
                            _threeHigh.Text = $"H {value.Temp.Max:#}°";
                            break;
                        case 4:
                            _fourDay.Text = $"{value.DateTime:ddd}";
                            _fourIcon.Text = WeatherIcons.Map[weateher.Icon].ToString();
                            _fourLow.Text = $"L {value.Temp.Min:#}°";
                            _fourHigh.Text = $"H {value.Temp.Max:#}°";
                            break;
                        case 5:
                            _fiveDay.Text = $"{value.DateTime:ddd}";
                            _fiveIcon.Text = WeatherIcons.Map[weateher.Icon].ToString();
                            _fiveLow.Text = $"L {value.Temp.Min:#}°";
                            _fiveHigh.Text = $"H {value.Temp.Max:#}°";
                            break;
                    }
                }

                _fadeIn.Begin();
            }
        }
    }
}