using System;
using System.Collections.Generic;
using System.Linq;


namespace Mirror.Extensions
{
    static class EnumerableExtensions
    {
        static Random Random => new Random((int)DateTime.Now.Ticks);

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable) => enumerable == null || !enumerable.Any();

        public static T RandomElement<T>(this IEnumerable<T> enumerable) =>
            enumerable.ElementAt(Random.Next(0, enumerable.Count()));
    }
}