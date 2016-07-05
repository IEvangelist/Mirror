using System.Collections.Generic;
using System.Linq;


namespace Mirror.Extensions
{
    static class EnumerableExtensions
    {
        internal static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable) => enumerable == null || !enumerable.Any();
    }
}