using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace Mirror.Calendar
{
    class Parser
    {
        internal List<Content> Properties { get; set; }
        internal List<vEvent> Events { get; set; }

        internal Parser(string icsFile)
        {
            Properties = new List<Content>();
            Events = new List<vEvent>();
            foreach (Match Content in Regex.Matches(Regex.Match(icsFile, @"BEGIN:VCALENDAR(.*?)BEGIN:VEVENT", RegexOptions.Singleline).Groups[1].Value, @"(.*?:.*(\n\s.*)*)"))
            {
                Properties.Add(new Content(Content.Groups[1].Value));
            }
            foreach (Match vevent in Regex.Matches(icsFile, @"BEGIN:VEVENT(.*?)END:VEVENT", RegexOptions.Singleline))
            {
                Events.Add(new vEvent(vevent.Groups[1].Value));
            }
        }

        internal Content GetProperty(string name) =>
            Properties.FirstOrDefault(x => x.Name.Equals(name));

        internal static Calendar FromString(string icsBlob)
        {
            var parser = new Parser(icsBlob);
            return new Calendar(parser.Events.Select(e => Event.From(e)));
        }
    }
}