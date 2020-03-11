using System;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework
{
    public static class MathExtensions
    {
        /// <summary>
        /// Rounds a double-precision floating-point value to a specified number of fractional 
        /// digits, and rounds midpoint values to the nearest even number.
        /// </summary>
        public static double Round(this double value, int digits = 0) =>
            Math.Round(value, digits);

        /// <summary>
        /// Rounds a single-precision floating-point value to a specified number of fractional 
        /// digits, and rounds midpoint values to the nearest even number.
        /// </summary>
        public static float Round(this float value, int digits = 0) =>
            (float)Math.Round(value, digits);

        /// <summary>
        /// Returns the absolute value of a single-precision floating-point number.
        /// </summary>
        public static float Abs(this float value) =>
            Math.Abs(value);

        /// <summary>
        /// Converts a floating point value given in radians to an fp value in degrees.
        /// </summary>
        public static float RadiansToDegrees(this float value) =>
            (value / (float)Math.PI) * 180f;

        /// <summary>
        /// Converts a floating point value given in degrees to an fp value in radians.
        /// </summary>
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

            end = MathHelper.Clamp(end, 0, arr.Length);
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

        /// <summary>
        /// Calculates the alpha of the color based on a float value between 0 and 1 and returns the new color.
        /// </summary>
        public static Color SetAlphaPct(this Color color, float alphaPercent) =>
            new Color(color.R, color.G, color.B, (byte)(alphaPercent * 255f));

        /// <summary>
        /// Retrieves the channel of a given <see cref="Color"/> by its index. R = 0, G = 1, B = 2, A = 3.
        /// </summary>
        public static byte GetChannel(this Color color, int channel)
        {
            switch (channel)
            {
                case 0:
                    return color.R;
                case 1:
                    return color.G;
                case 2:
                    return color.B;
                case 3:
                    return color.A;
            }

            return 0;
        }

        /// <summary>
        /// Sets the channel of a given <see cref="Color"/> by its index to the given value. R = 0, G = 1, B = 2, A = 3.
        /// </summary>
        public static Color SetChannel(this Color color, int channel, byte value)
        {
            switch(channel)
            {
                case 0:
                    color.R = value;
                    break;
                case 1:
                    color.G = value;
                    break;
                case 2:
                    color.B = value;
                    break;
                case 3:
                    color.A = value;
                    break;
            }

            return color;
        }
    }
}
