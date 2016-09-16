using Mirror.Extensions;
using System;
using Windows.UI.Xaml;

namespace Mirror.ViewModels
{
    public class ClockViewModel : BaseViewModel
    {
        DateTime _dateTime;

        public string HoursAndMinutes => $"{_dateTime:h:mm}";

        public string Seconds => $"{_dateTime:ss}";

        public string Date => $"{_dateTime:dddd}, {_dateTime:MMMM} {_dateTime.Day.ToOrdinalString()}";

        public ClockViewModel(DependencyObject dependency, DateTime dateTime) : base(dependency)
        {
            _dateTime = dateTime;
        }

        public override string ToFormattedString(DateTime? dateContext) => $"Today is {Date} and it is {HoursAndMinutes}.";
    }
}