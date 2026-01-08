using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace NoSlimes.UnityUtils.Common
{
    public static class CollectionUtils
    {
        /// <summary>
        /// Returns a shuffled copy of the given collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The collection to shuffle.</param>
        /// <returns>
        /// A new <see cref="IEnumerable{T}"/> containing the elements of
        /// <paramref name="source"/> in random order.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            List<T> buffer = source.ToList();

            for (int i = buffer.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
            }

            return buffer;
        }

        /// <summary>
        /// Returns a random element from the collection.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The collection to pick a random element from.</param>
        /// <returns>A randomly selected element from the collection.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the collection is empty.
        /// </exception>
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            IList<T> list = source as IList<T> ?? source.ToList();

            if (list.Count == 0)
                throw new InvalidOperationException("Source collection is empty.");

            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Returns a random subset of the collection containing up to the specified number of elements.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The collection to select elements from.</param>
        /// <param name="count">
        /// The maximum number of elements to include in the returned subset.
        /// </param>
        /// <returns>
        /// A new <see cref="IEnumerable{T}"/> containing up to <paramref name="count"/>
        /// randomly selected elements.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="count"/> is less than zero.
        /// </exception>
        public static IEnumerable<T> PickRandomSubset<T>(this IEnumerable<T> source, int count)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            return source.Shuffle().Take(count);
        }

        /// <summary>
        /// Removes duplicate elements from a collection while preserving the original order.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The collection to remove duplicates from.</param>
        /// <param name="comparer">
        /// An optional equality comparer used to compare elements.
        /// If <c>null</c>, the default equality comparer is used.
        /// </param>
        /// <returns>
        /// A new <see cref="IEnumerable{T}"/> containing only the first occurrence
        /// of each element in the original collection.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static IEnumerable<T> RemoveDuplicates<T>(
            this IEnumerable<T> source,
            IEqualityComparer<T> comparer = default)
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
        /// Returns the zero-based index of the first element that matches the specified predicate,
        /// or -1 if no such element is found.
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <param name="source">The collection to search.</param>
        /// <param name="predicate">The predicate used to match elements.</param>
        /// <returns>
        /// The zero-based index of the first matching element, or -1 if no match is found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="source"/> or <paramref name="predicate"/> is <c>null</c>.
        /// </exception>
        public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            int index = 0;
            foreach (T item in source)
            {
                if (predicate(item))
                    return index;
                index++;
            }

            return -1;
        }
    }
}
