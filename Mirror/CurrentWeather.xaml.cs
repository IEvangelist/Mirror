using Mirror.Core;
using Mirror.Extensions;
using Mirror.Networking;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Mirror
{
    public sealed partial class CurrentWeather : UserControl
    {
        public CurrentWeather()
        {
            InitializeComponent();            
        }

        void SetInitialState()
        {
            _content.Opacity = 0;
            ToggleContentVisibility(false);
        }

        void ToggleContentVisibility(bool isContentVisible)
        {
            _progressRing.IsActive = !isContentVisible;
            _loading.Visibility = isContentVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        async void OnLoaded(object sender, RoutedEventArgs e) => await LoadWeatherAsync();

        async Task LoadWeatherAsync()
        {
            SetInitialState();

            var current = await WeatherClient.GetCurrentAsync();
            if (current != null)
            {
                try
                {
                    _temperatureLabel.Text = $"{current.Main.Temp:#}°";
                    _lowTempLabel.Text = $"L {current.Main.TempMin:#}°";
                    _highTempLabel.Text = $"H {current.Main.TempMax:#}°";
                    _windLabel.Text = $"{current.Wind.Speed:#} {current.Wind.Deg.ToCardinal()}";

                    DateTime sunrise = current.Sys.SunriseDateTime,
                             sunset = current.Sys.SunsetDateTime;

                    if (DateTime.Now > sunset)
                    {
                        _sunRiseOrSetLabel.Text = $"{sunrise:h:mm tt}";
                    }
                    else
                    {
                        _sunRiseOrSetLabel.Text = $"{sunset:h:mm tt}";
                    }

                    var weather = current.Weather.FirstOrDefault();
                    if (weather != null)
                    {
                        _conditionLabel.Text = weather.Description.ToTitleCase();
                        _locationLabel.Text = current.Name;
                        _weatherIcon.Text = WeatherIcons.Map[weather.Icon].ToString();
                    }
                }
                finally
                {
                    ToggleContentVisibility(true);
                    _fadeIn.Begin();
                }
            }
        }
    }
}