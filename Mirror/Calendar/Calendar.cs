using System.Collections.Generic;


namespace Mirror.Calendar
{
    class Calendar
    {
        internal static Calendar Empty => new Calendar();

        internal IEnumerable<Event> Events { get; }

        internal Calendar(IEnumerable<Event> events)
        {
            Events = events;
        }

        Calendar() { }
    }
}