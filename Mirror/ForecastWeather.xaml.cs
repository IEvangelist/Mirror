using Mirror.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Mirror.Networking;
using System;
using System.Threading.Tasks;
using Mirror.ViewModels;
using Mirror.Speech;
using Windows.ApplicationModel;
using Mirror.Controls;

namespace Mirror
{
    public sealed partial class ForecastWeather : UserControl, IContextSynthesizer
    {
        DispatcherTimer _timer;
        IWeatherService _weatherService;

        string UnableToGenerateSpeechMessage { get; } =
            "I'm sorry, but I'm having difficulity retrieving the forecast right now. Please, try again later.";

        public ForecastWeather()
        {
            InitializeComponent();
        }

        async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return;
            }

            _weatherService = Services.Get<IWeatherService>();

            await LoadForecastAsync();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromHours(1) };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        async void OnTimerTick(object sender, object e) => await LoadForecastAsync();

        async Task LoadForecastAsync()
        {
            _forecastStackPanel.Opacity = 0;

            var forecast = await _weatherService.GetForecastAsync();
            DataContext = new ForecastViewModel(this, forecast);

            _fadeIn.Begin();
        }

        Task<string> IContextSynthesizer.GetContextualMessageAsync(DateTime? dateContext)
            => SpeechControlHelper.GetContextualMessageAsync(this,
                                                             DataContext,
                                                             dateContext,
                                                             UnableToGenerateSpeechMessage);
    }
}