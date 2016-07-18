namespace Mirror.Models
{
    public class Song
    {
        internal string Artist { get; private set; }

        internal string Title { get; private set; }

        internal Song(string artist, string title)
        {
            Artist = artist;
            Title = title;
        }

        public override string ToString() => $"{Title} by {Artist}";
    }
}