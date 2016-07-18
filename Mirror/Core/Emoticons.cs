using Mirror.Emotion;
using System.Collections.Generic;


namespace Mirror.Core
{
    class Emoticons
    {
        static Dictionary<Emotions, string> Map => 
            new Dictionary<Emotions, string>
            {
                [Emotions.Anger] = "X",
                [Emotions.Contempt] = "B",
                [Emotions.Disgust] = "a",
                [Emotions.Fear] = "o",
                [Emotions.Happiness] = "A",
                [Emotions.Neutral] = "C",
                [Emotions.Sadness] = "k",
                [Emotions.Surprise] = "v",
            };

        /// <summary>
        /// Intended to function with the emoticons.ttf.
        /// </summary>
        internal static string From(Emotions emotion) => Map[emotion];
    }
}