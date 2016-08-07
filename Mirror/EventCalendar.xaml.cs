using Mirror.Extensions;
using Mirror.Networking;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Mirror
{
    public sealed partial class EventCalendar : UserControl
    {
        public EventCalendar()
        {
            InitializeComponent();
        }

        private async void OnLoaded(object sender, RoutedEventArgs args)
        {
            var calendars = await CalendarClient.GetCalendarsAsync();
            if (calendars != null)
            {
                var events =
                    calendars.SelectMany(calendar => calendar.Events)
                             .Where(e =>
                                    e.StartDateTime > DateTime.Now &&
                                    e.Summary.IndexOf("cancel", StringComparison.OrdinalIgnoreCase) == -1)
                             .OrderBy(e => e.StartDateTime)
                             .Take(10)
                             .ToArray();

                if (events != null)
                {
                    for (int index = 0; index < events.Length; ++ index)
                    {
                        switch (index)
                        {
                            case 0:
                                await UpdateEventDetailsAsync(_eventDayOne, _eventTimeOne, _eventTitleOne, events[index]);
                                break;
                            case 1:
                                await UpdateEventDetailsAsync(_eventDayTwo, _eventTimeTwo, _eventTitleTwo, events[index]);
                                break;
                            case 2:
                                await UpdateEventDetailsAsync(_eventDayThree, _eventTimeThree, _eventTitleThree, events[index]);
                                break;
                            case 3:
                                await UpdateEventDetailsAsync(_eventDayFour, _eventTimeFour, _eventTitleFour, events[index]);
                                break;
                            case 4:
                                await UpdateEventDetailsAsync(_eventDayFive, _eventTimeFive, _eventTitleFive, events[index]);
                                break;
                        }
                    }

                    _fadeIn.Begin();
                }
            }
        }

        async Task UpdateEventDetailsAsync(TextBlock day, 
                                           TextBlock hours, 
                                           TextBlock title, 
                                           Calendar.Event e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var startDate = e.StartDateTime.GetValueOrDefault();
                var endDate = e.EndDateTime.GetValueOrDefault();

                day.Text = $"{startDate:ddd}, {startDate:MMMM} {startDate.Day.ToOrdinalString()}";
                hours.Text = $"{startDate:h:mm}-{endDate:h:mm tt}";
                title.Text = e.Summary;
            });
        }
    }
}