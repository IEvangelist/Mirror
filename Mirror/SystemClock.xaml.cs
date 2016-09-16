using Mirror.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Mirror
{
    public sealed partial class SystemClock : UserControl
    {
        DispatcherTimer _timer;

        public SystemClock()
        {
            InitializeComponent();

            _content.Opacity = 0;
            UpdateClock();
            _fadeIn.Begin();
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        void OnTimerTick(object sender, object e) => UpdateClock();

        void UpdateClock()
        {
            DataContext = new ClockViewModel(this, GetLocalTime());
        }

        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _timer.Tick -= OnTimerTick;
            _timer.Stop();
        }

        DateTime GetLocalTime()
        {
            var utcNow = DateTime.UtcNow;
            var explicitlyUtc = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

            return explicitlyUtc.ToLocalTime();
        }
    }
}