using System;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework
{
    public static class MathExtensions
    {
        public static double Round(this double value, int digits = 0) =>
            Math.Round(value, digits);

        public static float Round(this float value, int digits = 0) =>
            (float)Math.Round(value, digits);

        public static float Abs(this float value) =>
            Math.Abs(value);

        public static float RadiansToDegrees(this float value) =>
            (value / (float)Math.PI) * 180f;

        public static float DegreesToRadians(this float value) =>
            (value * (float)Math.PI) / 180f;
    }

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
        public static T[] GetUnique<T>(this ICollection<T> original)
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

        public static Color SetAlpha(this Color color, byte alpha) =>
            new Color(color.R, color.G, color.B, alpha);

        /// <summary>
        /// Retrieves the channel of a given <see cref="Color"/> by its index. R = 0, G = 1, B = 2, A = 3.
        /// </summary>
        public static byte GetChannel(this Color color, int channel)
        {
            if (channel == 0)
                return color.R;
            else if (channel == 1)
                return color.G;
            else if (channel == 2)
                return color.B;
            else
                return color.A;
        }

        /// <summary>
        /// Sets the channel of a given <see cref="Color"/> by its index to the given value. R = 0, G = 1, B = 2, A = 3.
        /// </summary>
        public static Color SetChannel(this Color color, int channel, byte value)
        {
            if (channel == 0)
                color.R = value;
            else if (channel == 1)
                color.G = value;
            else if (channel == 2)
                color.B = value;
            else
                color.A = value;

            return color;
        }
    }
}
