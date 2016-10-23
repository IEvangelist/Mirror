using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Mirror.Extensions
{
    public static class MediaElementExtensions
    {
        public static Task SetVolumeFromCommandAsync(this MediaElement mediaElement, string phrase)
            => mediaElement.ThreadSafeAsync(() =>
            {
                var volume = mediaElement.Volume;
                if (phrase.ContainsIgnoringCase("unmute"))
                {
                    mediaElement.IsMuted = false;
                }
                else if (phrase.ContainsIgnoringCase("mute"))
                {
                    mediaElement.IsMuted = true;
                }
                else if (phrase.ContainsIgnoringCase("up") ||
                         phrase.ContainsIgnoringCase("loud"))
                {
                    mediaElement.Volume = Math.Min(1, volume + .1);
                }
                else if (phrase.ContainsIgnoringCase("down") ||
                         phrase.ContainsIgnoringCase("quiet"))
                {
                    mediaElement.Volume = Math.Max(0, volume - .1);
                }
                else if (phrase.ContainsIgnoringCase("percent"))
                {
                    mediaElement.Volume = GetPercent(phrase);
                }
            });

        static double GetPercent(string phrase)
        {
            int percent;
            int.TryParse(Regex.Match(phrase, @"\d+").Value, out percent);

            return percent / 100d;
        }
    }
}