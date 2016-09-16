using Microsoft.ProjectOxford.Emotion.Contract;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using RawEmotion = Microsoft.ProjectOxford.Emotion.Contract.Emotion;

namespace Mirror.Emotion
{
    public enum Emotions
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

    public static class EmotionMessages
    {
        public static Dictionary<Emotions, IEnumerable<string>> Messages =
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

    static class EmotionExtensions
    {
        public static IEnumerable<Result> ToResults(this IEnumerable<RawEmotion> emotions)
        {
            Contract.Assert(emotions != null);

            foreach (var emotion in emotions)
            {
                yield return emotion.Scores.ToMostProbable();
            }
        }

        public static Result ToMostProbable(this Scores scores)
        {
            Contract.Assert(scores != null);

            var first = scores.ToRankedList().FirstOrDefault();
            if (first.Equals(default(KeyValuePair<string, float>)))
            {
                return Result.Empty;
            }

            return Result.FromScore(first.Key, first.Value);
        }

        public static List<Result> ToResults(this Scores scores)
        {
            Contract.Assert(scores != null);

            // While the ranked list is ordered, when we 'select' into a result object the order is not guaranteed. 
            return scores.ToRankedList()
                         .Select(kvp => Result.FromScore(kvp.Key, kvp.Value))
                         .OrderByDescending(result => result.Score)
                         .ToList();
        }
    }
}