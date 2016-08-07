using System.Collections.Generic;

namespace Mirror.Emotion
{
    enum Emotions
    {
        None,
        Anger,
        Contempt,
        Disgust,
        Fear,
        Happiness,
        Neutral,
        Sadness,
        Surprise
    };

    internal static class EmotionMessages
    {
        internal static Dictionary<Emotions, IEnumerable<string>> Messages =
            new Dictionary<Emotions, IEnumerable<string>>
            {
                [Emotions.Anger] = new[]
                {
                    "Phew, watch out!",
                    "Wow, you looked pissed...",
                    "Why so angry?",
                    "Really, c'mon now - cheer up",
                    "Temper, temper!"
                },
                [Emotions.Contempt] = new[]
                {
                    "Feeling contempt?",
                    "..."
                },
                [Emotions.Disgust] = new[]
                {
                    "Feeling disgusted?",
                    "Was it something I said?"
                },
                [Emotions.Fear] = new[]
                {
                    "Are you scared?",
                    "IT'S A GHOST!",
                    "Bloody Mary...x3 times"
                },
                [Emotions.Happiness] = new[]
                {
                    "You look happy!",
                    "Don't you look chipper?",
                    "That's the spirit!",
                    "Stay positive!",
                    "That smile is inspiring!"
                },
                [Emotions.Neutral] = new[]
                {
                    "Show me your smile",
                    "Fine, don't say \"Hello\"",
                    "Why so smug?"
                },
                [Emotions.Sadness] = new[]
                {
                    "Feeling a bit down, are we?",
                    "Things can only get better",
                    "We'll pull through this!",
                    "Be strong!",
                    "Hang in there"
                },
                [Emotions.Surprise] = new[]
                {
                    "WOW!",
                    "Did I miss something?",
                    "Surprised to see me?",
                    "Lower your eyebrows"
                }
            };
    }
}