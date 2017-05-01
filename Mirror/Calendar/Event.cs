using System;
using static Mirror.Calendar.Status;


namespace Mirror.Calendar
{
    public class Event
    {
        static Func<Content, DateTime?> ParseDate = content =>
            DateParser.Parse(content.Value, TimeZoneParser.Parse(content.Parameters));

        public string Description { get; private set; }
        public string Summary { get; private set; }
        public string Location { get; private set; }
        public Guid? UniqueId { get; private set; }
        public Status? Status { get; private set; }
        public DateTime? TimeStamp { get; private set; }
        public DateTime? StartDateTime { get; private set; }
        public DateTime? EndDateTime { get; private set; }

        public bool IsConfirmed => Status == Confirmed;

        public static Event From(vEvent vEvent)
        {
            if (vEvent == null || vEvent.Contents == null) return null;

            var result = new Event();
            foreach (var content in vEvent.Contents)
            {
                switch (content.Name)
                {
                    case "DTSTART":
                        result.StartDateTime = ParseDate(content);
                        break;
                    case "DTEND":
                        result.EndDateTime = ParseDate(content);
                        break;
                    case "DTSTAMP":
                        result.TimeStamp = ParseDate(content);
                        break;
                    case "DESCRIPTION":
                        result.Description = content.ToString();
                        break;
                    case "SUMMARY":
                        result.Summary = content.Value;
                        break;
                    case "LOCATION":
                        result.Location = content.Value;
                        break;
                    case "UID":
                        if (Guid.TryParse(content.Value, out var id))
                        {
                            result.UniqueId = id;
                        }
                        break;
                    case "STATUS":
                        if (Enum.TryParse(content.Value, true, out Status status))
                        {
                            result.Status = status;
                        }
                        break;
                }
            }

            return result;
        }
    }
}