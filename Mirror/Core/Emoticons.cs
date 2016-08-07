using Mirror.Emotion;
using Mirror.Extensions;
using System.Collections.Generic;


namespace Mirror.Core
{
    class Emoticons
    {
        static Dictionary<Emotions, string[]> Map => 
            new Dictionary<Emotions, string[]>
            {
                [Emotions.Anger] = new[] { "Q", "R", "X", "y" },
                [Emotions.Contempt] = new[] { "B", "b", "u" },
                [Emotions.Disgust] = new[] { "a", "P", "M", "S" },
                [Emotions.Fear] = new[] { "o", "u", "n", "e" },
                [Emotions.Happiness] = new[] { "A", "j", "m" },
                [Emotions.Neutral] = new[] { "C", "J", "l", "Z" },
                [Emotions.Sadness] = new[] { "k", "d", "I" },
                [Emotions.Surprise] = new[] { "v", "w", "h" },
            };

        /// <summary>
        /// Intended to function with the emoticons.ttf.
        /// </summary>
        internal static string From(Emotions emotion) => Map[emotion].RandomElement();
    }
}