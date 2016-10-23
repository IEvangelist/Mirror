using Mirror.Controls;
using Mirror.Core;
using Mirror.Networking;
using Mirror.Speech;
using Mirror.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Mirror
{
    public sealed partial class CurrentWeather : UserControl, IAsyncLoader, IContextSynthesizer
    {
        DispatcherTimer _timer;
        IWeatherService _weatherService;

        string UnableToGenerateSpeechMessage { get; } = 
            "I'm sorry, but I'm having difficulity retrieving the current weather right now. Please, try again later.";

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

        async Task IAsyncLoader.LoadAsync()
        {
            _weatherService = Services.Get<IWeatherService>();

            await LoadWeatherAsync();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(15) };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return;
            }            
        }

        async void OnTimerTick(object sender, object e) => await LoadWeatherAsync();

        async Task LoadWeatherAsync()
        {
            SetInitialState();

            var currentWeather = await _weatherService.GetCurrentAsync();
            if (currentWeather != null)
            {
                DataContext = new CurrentWeatherViewModel(this, currentWeather);
                ToggleContentVisibility(true);
                _fadeIn.Begin();
            }
        }

        Task<string> IContextSynthesizer.GetContextualMessageAsync(DateTime? dateContext) 
            => SpeechControlHelper.GetContextualMessageAsync(this, 
                                                             DataContext, 
                                                             dateContext, 
                                                             UnableToGenerateSpeechMessage);
    }
}