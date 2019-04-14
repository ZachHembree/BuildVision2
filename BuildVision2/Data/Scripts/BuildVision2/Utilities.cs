using VRageMath;
using System;
using System.Text.RegularExpressions;
using Sandbox.ModAPI;

namespace DarkHelmet.BuildVision2
{
    internal static class Utilities
    {
        private static readonly Regex colorParser = new Regex(@"(\s*,?(\d{1,3})\s*,?){3,4}");

        public static bool TryParseColor(string colorData, out Color value)
        {
            Match match = colorParser.Match(colorData);
            CaptureCollection captures = match.Groups[2].Captures;
            value = new Color();
            byte r, g, b, a;

            if (captures.Count > 2)
            {
                if (!byte.TryParse(captures[0].Value, out r))
                    return false;

                if (!byte.TryParse(captures[1].Value, out g))
                    return false;

                if (!byte.TryParse(captures[2].Value, out b))
                    return false;

                value.R = r;
                value.G = g;
                value.B = b;

                if (captures.Count > 3)
                {
                    if (!byte.TryParse(captures[3].Value, out a))
                        return false;

                    value.A = a;
                }

                return true;
            }
            else
                return false;
        }

        public static Color ParseColor(string colorData)
        {
            Match match = colorParser.Match(colorData);
            CaptureCollection captures = match.Groups[2].Captures;
            Color value = new Color();

            if (captures.Count > 2)
            {
                value.R = byte.Parse(captures[0].Value);
                value.G = byte.Parse(captures[1].Value);
                value.B = byte.Parse(captures[2].Value);

                if (captures.Count > 3)
                    value.A = byte.Parse(captures[3].Value);

                return value;
            }
            else
                return Color.White;
        }

        public static string GetColorString(Color color, bool includeAlpha = true)
        {
            if (includeAlpha)
                return $"{color.R},{color.G},{color.B},{color.A}";
            else
                return $"{color.R},{color.G},{color.B}";
        }

        /// <summary>
        /// Clamps an int between two values.
        /// </summary>
        public static double Clamp(double value, double min, double max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }

        /// <summary>
        /// Clamps a float between two values.
        /// </summary>
        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }

        /// <summary>
        /// Clamps an int between two values.
        /// </summary>
        public static int Clamp(int value, int min, int max)
        {
            if (value < min)
                return min;
            else if (value > max)
                return max;
            else
                return value;
        }

        /// <summary>
        /// Clamps vector members between two values.
        /// </summary>
        public static Vector2D Clamp(Vector2D value, double min, double max)
        {
            value.X = Clamp(value.X, min, max);
            value.Y = Clamp(value.Y, min, max);

            return value;
        }

        /// <summary>
        /// Rounds vector elements to a specified number of digits.
        /// </summary>
        public static Vector2D Round(Vector2D value, int digits)
        {
            value.X = Math.Round(value.X, digits);
            value.Y = Math.Round(value.Y, digits);
            return value;
        }

        /// <summary>
        /// Rounds vector elements to a specified number of digits.
        /// </summary>
        public static Vector3D Round(Vector3D value, int digits)
        {
            value.X = Math.Round(value.X, digits);
            value.Y = Math.Round(value.Y, digits);
            value.Z = Math.Round(value.Z, digits);
            return value;
        }

        /// <summary>
        /// Finds the absolute value of the components of a vector.
        /// </summary>
        public static Vector2I Abs(Vector2I value)
        {
            value.X = Math.Abs(value.X);
            value.Y = Math.Abs(value.Y);
            return value;
        }

        /// <summary>
        /// Generates subarray that starts from a given index and continues to the end.
        /// </summary>
        public static T[] GetSubarray<T>(T[] arr, int i)
        {
            T[] trimmed = new T[arr.Length - i];

            for (int n = i; n < arr.Length; n++)
                trimmed[n - i] = arr[n];

            return trimmed;
        }
    }
}
