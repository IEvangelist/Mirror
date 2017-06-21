using System;
using System.Threading.Tasks;
using Mirror.Controls;
using Mirror.Core;
using Mirror.Networking;
using Mirror.Speech;
using Mirror.ViewModels;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Mirror
{
    public sealed partial class ForecastWeather : UserControl, IAsyncLoader, IContextSynthesizer
    {
        DispatcherTimer _timer;
        IWeatherService _weatherService;

        string UnableToGenerateSpeechMessage { get; } =
            "I'm sorry, but I'm having difficulty retrieving the forecast right now. Please, try again later.";

        public ForecastWeather()
        {
            InitializeComponent();
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return;
            }
        }

        async Task IAsyncLoader.LoadAsync()
        {
            _weatherService = Services.Get<IWeatherService>();

            await LoadForecastAsync();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromHours(1) };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        async void OnTimerTick(object sender, object e) => await LoadForecastAsync();

        async Task LoadForecastAsync()
        {
            try
            {
                _forecastStackPanel.Opacity = 0;

                var forecast = await _weatherService.GetForecastAsync();
                DataContext = new ForecastViewModel(this, forecast);

                _fadeIn.Begin();
            }
            catch (Exception ex) when (DebugHelper.IsHandled<ForecastWeather>(ex))
            {
                // If we're unable to load, this is probably a configuration issue.
            }
        }

        Task<string> IContextSynthesizer.GetContextualMessageAsync(DateTime? dateContext)
            => SpeechControlHelper.GetContextualMessageAsync(this,
                                                             DataContext,
                                                             dateContext,
                                                             UnableToGenerateSpeechMessage);
    }
}