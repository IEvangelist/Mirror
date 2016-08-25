using Mirror.Extensions;
using TagLib;

namespace Mirror.Models
{
    public class Song
    {
        public string Artist { get; private set; }

        public string Title { get; private set; }

        public Song(Tag tag)
        {
            Artist = GetArtist(tag);
            Title = GetTitle(tag);
        }

        public override string ToString() => $"{Title} by {Artist}";

        static string GetArtist(Tag tag)
        {
            if (tag == null) return string.Empty;

            return tag.FirstAlbumArtist
                      .Coalesce(tag.JoinedAlbumArtists,
                                tag.FirstPerformer,
                                tag.JoinedPerformers);
        }

        static string GetTitle(Tag tag)
        {
            if (tag == null) return string.Empty;

            return tag.Title;
        }
    }
}