using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace NoSlimes.Utils
{
    public static class CollectionUtils
    {
        /// <summary>
        /// Returns a shuffled copy of the given collection.
        /// </summary>
        /// <typeparam name="T">Type of elements in the collection.</typeparam>
        /// <param name="source">The collection to shuffle.</param>
        /// <returns>A new <see cref="IEnumerable{T}"/> with the elements randomly shuffled.</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            List<T> buffer = source.ToList();

            for (int i = buffer.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }

            return buffer;
        }

        /// <summary>
        /// Removes duplicate elements from a collection, preserving the original order.
        /// </summary>
        /// <typeparam name="T">Type of elements in the collection.</typeparam>
        /// <param name="source">The collection to remove duplicates from.</param>
        /// <param name="comparer">Optional equality comparer to use for comparing elements.</param>
        /// <returns>A new <see cref="IEnumerable{T}"/> with duplicates removed.</returns>
        public static IEnumerable<T> RemoveDuplicates<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            HashSet<T> seen = new(comparer);
            foreach (T item in source)
            {
                if (seen.Add(item))
                    yield return item;
            }
        }

        /// <summary>
        /// Returns a random element from the collection.
        /// </summary>
        /// <typeparam name="T">Type of elements in the collection.</typeparam>
        /// <param name="source">The collection to pick a random element from.</param>
        /// <returns>A random element from the collection.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the collection is null or empty.</exception>
        public static T TakeRandom<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new InvalidOperationException("Source collection is null.");

            // Use IList<T> for O(1) access if possible, otherwise convert to a list
            IList<T> list = source as IList<T> ?? source.ToList();

            if (list.Count == 0)
                throw new InvalidOperationException("Source collection is empty.");

            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Returns the index of the first element matching the predicate, or -1 if not found.
        /// </summary>
        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            int index = 0;
            foreach (T item in source)
            {
                if (predicate(item)) return index;
                index++;
            }
            return -1;
        }

        /// <summary>
        /// Returns a random subset of the collection with up to 'count' elements.
        /// </summary>
        public static IEnumerable<T> RandomSubset<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }
    }
}