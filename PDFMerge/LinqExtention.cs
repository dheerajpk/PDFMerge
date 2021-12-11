
using System;
using System.Collections.Generic;

/// <summary>
/// The Helper namespace.
/// </summary>
namespace PDFMerge
{
    /// <summary>
    /// Extention class for Linq
    /// </summary>
    public static class LinqExtention
    {
        /// <summary>
        /// Get Distinct object collection
        /// </summary>
        /// <typeparam name="TSource">The type of the t source.</typeparam>
        /// <typeparam name="TKey">The type of the t key.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="keySelector">The key selector.</param>
        /// <returns>IEnumerable&lt;TSource&gt;.</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
        // ForEach
        public static IEnumerable<TSource> ForEach<TSource>(this IEnumerable<TSource> source,
            Action<TSource> action)
        {
            foreach (TSource item in source)
                action(item);

            return source;
        }

        // Foreach with index
        public static IEnumerable<TSource> ForEach<TSource>(this IEnumerable<TSource> source,
            Action<TSource, int> action)
        {
            int index = 0;
            foreach (TSource item in source)
                action(item, index++);

            return source;
        }
    }
}
