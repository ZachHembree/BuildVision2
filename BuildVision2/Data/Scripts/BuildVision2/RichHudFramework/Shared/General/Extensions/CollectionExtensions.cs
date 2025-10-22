using System.Collections.Generic;
using VRageMath;
using System;

namespace RichHudFramework
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Inserts a span of a given source list/array at the given index
        /// </summary>
        public static void InsertSpan<T>(this List<T> dst, int dstIndex, IReadOnlyList<T> src, int srcIndex = 0, int srcCount = -1)
        {
            if (srcCount == -1)
                srcCount = src.Count;

            if (srcCount > 0)
            {
                dst.EnsureCapacity(dst.Count + srcCount);

                int dstOflEnd = dst.Count - 1,
                    dstOflStart = Math.Max(dstIndex, dstOflEnd - srcCount),
                    dstOflCount = dstOflEnd - dstOflStart + 1,

                    srcOflCount = Math.Max(0, srcCount - dstOflCount),
                    srcOflEnd = srcIndex + srcCount - 1,
                    srcOflStart = srcOflEnd - srcOflCount + 1,
                    dstOvrCount = srcCount - srcOflCount;

                // Append src overflow                
                for (int i = srcOflStart; i <= srcOflEnd; i++)
                    dst.Add(src[i]);

                // Append dst overflow
                for (int i = dstOflStart; i <= dstOflEnd; i++)
                    dst.Add(dst[i]);

                // Move displaced range
                for (int i = dstIndex; i < dstOflStart; i++)
                    dst[i + srcCount] = dst[i];

                // Overwrite displaced range
                for (int i = 0; i < dstOvrCount; i++)
                    dst[i + dstOflStart] = src[i + srcIndex];
            }
        }

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

            end = MathHelper.Clamp(end, 0, arr.Length);
            trimmed = new T[end - start];

            for (int n = start; n < end; n++)
                trimmed[n - start] = arr[n];

            return trimmed;
        }

        /// <summary>
        /// Returns an array containing only unique entries from the collection.
        /// </summary>
        public static T[] GetUnique<T>(this IReadOnlyList<T> original)
        {
            var unique = new List<T>(original.Count);

            foreach (T item in original)
            {
                if (!unique.Contains(item))
                    unique.Add(item);
            }

            return unique.ToArray();
        }
    }
}
