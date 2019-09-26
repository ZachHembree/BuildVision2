using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Generates subarray that starts from a given index and continues to the end.
        /// </summary>
        public static T[] GetSubarray<T>(this T[] arr, int start)
        {
            T[] trimmed = new T[arr.Length - start];

            for (int n = start; n < arr.Length; n++)
                trimmed[n - start] = arr[n];

            return trimmed;
        }

        /// <summary>
        /// Generates subarray that starts from a given index and continues to the end.
        /// </summary>
        public static T[] GetSubarray<T>(this T[] arr, int start, int end)
        {
            T[] trimmed;

            end = Utils.Math.Clamp(end, 0, arr.Length);
            trimmed = new T[end - start];

            for (int n = start; n < end; n++)
                trimmed[n - start] = arr[n];

            return trimmed;
        }

        /// <summary>
        /// Returns an array containing only unique entries from the collection.
        /// </summary>
        public static T[] GetUnique<T>(this System.Collections.Generic.ICollection<T> original)
        {
            List<T> unique = new List<T>(original.Count);

            foreach (T item in original)
            {
                if (!unique.Contains(item))
                    unique.Add(item);
            }

            return unique.ToArray();
        }
    }

    public static class VectorExtensions
    {
        /// <summary>
        /// Converts a <see cref="Vector2"/> to a <see cref="Vector2D"/>
        /// </summary>
        public static Vector2D ToDouble(this Vector2 vec) =>
            new Vector2D(vec.X, vec.Y);

        /// <summary>
        /// Converts a <see cref="Vector2D"/> to a <see cref="Vector2"/>
        /// </summary>
        public static Vector2 ToSingle(this Vector2D vec) =>
            new Vector2((float)vec.X, (float)vec.Y);
    }
}
