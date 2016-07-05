using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Mirror
{
    public sealed partial class SystemClock : UserControl
    {
        DispatcherTimer _timer = new DispatcherTimer();

        public SystemClock()
        {
            InitializeComponent();

            _content.Opacity = 0;
            UpdateClock();
            _fadeIn.Begin();
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick -= OnTimerTick;
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        void OnTimerTick(object sender, object e) => UpdateClock();

        void UpdateClock()
        {
            var now = DateTime.Now;
            _timeLabel.Text = now.ToString("h:mm");
            _secondsLabel.Text = now.ToString("ss");
            _dayAndDateLabel.Text = $"{now:dddd}, {now:MMMM} {now:dd}";
        }

        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _timer.Tick -= OnTimerTick;
            _timer.Stop();
        }
    }
}