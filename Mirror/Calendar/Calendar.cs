using System.Collections.Generic;


namespace Mirror.Calendar
{
    public class Calendar
    {
        public static Calendar Empty { get; } = new Calendar();

        public IEnumerable<Event> Events { get; }

        public Calendar(IEnumerable<Event> events)
        {
            Events = events;
        }

        Calendar() { }
    }
}