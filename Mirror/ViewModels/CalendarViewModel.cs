using Mirror.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Windows.UI.Xaml;

namespace Mirror.ViewModels
{
    public class CalendarViewModel : BaseViewModel
    {
        public List<Event> Events { get; private set; }

        public CalendarViewModel(DependencyObject dependency, Calendar.Event[] events) : base(dependency)
        {
            Events = events.Select(e => new Event(dependency, e)).ToList();
        }

        public override string ToFormattedString(DateTime? dateContext)
        {
            var builder = new StringBuilder();
            foreach (var e in Events.Where(@event => 
                                           dateContext.HasValue 
                                               ? @event.StartDateTime.Date == dateContext.Value.Date
                                               : true))
            {
                builder.AppendLine(e.ToFormattedString(dateContext));
            }
            return builder.ToString();
        }
    }

    public class Event : BaseViewModel
    {
        Calendar.Event _event;

        public DateTime StartDateTime => _event.StartDateTime.GetValueOrDefault();

        public string Day { get; private set; }

        public string Hours { get; private set; }

        public string Title => _event.Summary;

        public string Details => _event.Description;

        public Event(DependencyObject dependency, Calendar.Event e) : base(dependency)
        {
            _event = e;

            var startDate = e.StartDateTime.GetValueOrDefault();
            var endDate = e.EndDateTime.GetValueOrDefault();

            Day = $"{startDate:ddd}, {startDate:MMM} {startDate.Day.ToOrdinalString()}";
            Hours = $"{startDate:h:mm}-{endDate:h:mm tt}";
        }

        public override string ToFormattedString(DateTime? dateContext)
        {
            return $"On {StartDateTime:dddd} the {StartDateTime.Day.ToOrdinalString()}, at {StartDateTime:h:mm tt} you have a {Title} scheduled. " + 
                   $"The details for this event are as follows: {Details}.";
        }
    }
}