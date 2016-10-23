using Mirror.Controls;
using Mirror.Core;
using Mirror.Extensions;
using Mirror.Networking;
using Mirror.Speech;
using Mirror.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static Mirror.Calendar.Calendar;


namespace Mirror
{
    public sealed partial class EventCalendar : UserControl, IAsyncLoader, IContextSynthesizer
    {
        DispatcherTimer _timer;
        ICalendarService _calendarService;

        string UnableToGenerateSpeechMessage { get; } =
            "I'm sorry, but I'm having difficulity retrieving your calendar events right now. Please, try again later.";

        public EventCalendar()
        {
            InitializeComponent();
        }

        Task<string> IContextSynthesizer.GetContextualMessageAsync(DateTime? dateContext)
            => SpeechControlHelper.GetContextualMessageAsync(this,
                                                             DataContext,
                                                             dateContext,
                                                             UnableToGenerateSpeechMessage);

        void OnLoaded(object sender, RoutedEventArgs args)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return;
            }
        }

        async Task IAsyncLoader.LoadAsync()
        {
            _calendarService = Services.Get<ICalendarService>();

            await LoadCalendarEventsAsync();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromHours(1) };
            _timer.Tick += OnTimerTick;
            _timer.Start();
        }

        async void OnTimerTick(object sender, object e) => await LoadCalendarEventsAsync();

        async Task LoadCalendarEventsAsync()
        {
            var calendars = await _calendarService.GetCalendarsAsync();
            if (!calendars.IsNullOrEmpty() && calendars.All(calendar => calendar != Empty))
            {
                var view = ApplicationView.GetForCurrentView();
                var take = view.Orientation == ApplicationViewOrientation.Portrait ? 7 : 5;

                var events =
                    calendars.SelectMany(calendar => calendar?.Events)
                             .Where(e =>
                                    e.StartDateTime > DateTime.Now &&
                                    !string.IsNullOrWhiteSpace(e.Summary) &&
                                    e.Summary.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) == -1)
                             .OrderBy(e => e.StartDateTime)
                             .Take(take)
                             .ToArray();

                if (!events.IsNullOrEmpty())
                {
                    DataContext = new CalendarViewModel(this, events);
                    _fadeIn.Begin();
                }
                else
                {
                    Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}