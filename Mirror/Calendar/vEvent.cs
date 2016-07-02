using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace Mirror.Calendar
{
    class vEvent
    {
        internal List<Content> Contents { get; set; }

        internal vEvent(string vevent)
        {
            Contents = new List<Content>();
            foreach (Match Content in Regex.Matches(vevent, @"(.*?:.*(\n\s.*)*)"))
            {
                Contents.Add(new Content(Content.Groups[1].Value));
            }
        }

        internal Content GetContent(string name) =>
            Contents.FirstOrDefault(x => x.Name.Equals(name));

        internal string GetContentValueOrDefault(string name)
        {
            Content Content = Contents.FirstOrDefault(x => x.Name.Equals(name));
            return Content != null ? Content.ToString() : string.Empty;
        }

    }
}