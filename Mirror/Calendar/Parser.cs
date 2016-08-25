using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace Mirror.Calendar
{
   public class Parser
    {
        public List<Content> Properties { get; set; }
        public List<vEvent> Events { get; set; }

        public Parser(string icsFile)
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

        public Content GetProperty(string name) =>
            Properties.FirstOrDefault(x => x.Name.Equals(name));

        public static Calendar FromString(string icsBlob)
        {
            var parser = new Parser(icsBlob);
            return new Calendar(parser.Events.Select(e => Event.From(e)));
        }
    }
}