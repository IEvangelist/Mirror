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
        DispatcherTimer _timer = new DispatcherTimer();
        IWeatherService _weatherService = Services.Get<IWeatherService>();

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

        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await LoadWeatherAsync();

            _timer.Stop();
            _timer.Interval = TimeSpan.FromHours(1);
            _timer.Tick -= OnTimerTick;
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        async void OnTimerTick(object sender, object e) => await LoadWeatherAsync();

        async Task LoadWeatherAsync()
        {
            SetInitialState();

            var current = await _weatherService.GetCurrentAsync();
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
                        _weatherIcon.Text = Weather.Icons[weather.Icon];
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