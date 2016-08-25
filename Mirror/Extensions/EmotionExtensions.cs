using Microsoft.ProjectOxford.Emotion.Contract;
using Mirror.Emotion;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using RawEmotion = Microsoft.ProjectOxford.Emotion.Contract.Emotion;


namespace Mirror.Extensions
{
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