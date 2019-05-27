using VRageMath;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Sandbox.ModAPI;

namespace DarkHelmet
{
    /// <summary>
    /// Generic singleton.
    /// </summary>
    public class Singleton<T> where T : Singleton<T>, new()
    {
        public static T Instance
        {
            get { if (instance == null) Init(); return instance; }
            protected set { instance = value; }
        }

        protected static bool canInstantiate = false;
        private static T instance;

        protected Singleton()
        {
            if (!canInstantiate)
                throw new Exception("Tyes of Singleton<T> cannot be instantiated externally.");

            canInstantiate = false;
        }

        public static void Init()
        {
            if (instance == null)
            {
                canInstantiate = true;
                instance = new T();
                instance.AfterInit();
            }
        }

        protected virtual void AfterInit() { }

        protected virtual void BeforeClose() { }

        public static void Close()
        {
            if (Instance != null)
            {
                Instance.BeforeClose();
                Instance = null;
            }
        }
    }

    internal static class Utilities
    {
        private static readonly Regex colorParser = new Regex(@"(\s*,?(\d{1,3})\s*,?){3,4}");

        /// <summary>
        /// Determines whether a string can be parsed into a <see cref="VRageMath.Color"/> and returns true if so.
        /// </summary>
        public static bool CanParseColor(string colorData)
        {
            Match match = colorParser.Match(colorData);
            CaptureCollection captures = match.Groups[2].Captures;
            byte r, g, b, a;

            if (captures.Count > 2)
            {
                if (!byte.TryParse(captures[0].Value, out r))
                    return false;

                if (!byte.TryParse(captures[1].Value, out g))
                    return false;

                if (!byte.TryParse(captures[2].Value, out b))
                    return false;

                if (captures.Count > 3)
                {
                    if (!byte.TryParse(captures[3].Value, out a))
                        return false;
                }

                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Tries to convert a string of color values to its <see cref="VRageMath.Color"/> equivalent.
        /// </summary>
        public static bool TryParseColor(string colorData, out Color value, bool ignoreAlpha = false)
        {
            bool successful;

            try
            {
                value = ParseColor(colorData, ignoreAlpha);
                successful = true;
            }
            catch
            {
                value = Color.White;
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Converts a string of color values to its <see cref="VRageMath.Color"/> equivalent.
        /// </summary>
        public static Color ParseColor(string colorData, bool ignoreAlpha = false)
        {
            Match match = colorParser.Match(colorData);
            CaptureCollection captures = match.Groups[2].Captures;
            Color value = new Color();

            if (captures.Count > 2)
            {
                value.R = byte.Parse(captures[0].Value);
                value.G = byte.Parse(captures[1].Value);
                value.B = byte.Parse(captures[2].Value);

                if (captures.Count > 3 || ignoreAlpha)
                    value.A = byte.Parse(captures[3].Value);
                else
                    value.A = 255;

                return value;
            }
            else
                throw new Exception("Color string must contain at least 3 values.");
        }

        public static string GetColorString(Color color, bool includeAlpha = true)
        {
            if (includeAlpha)
                return $"{color.R},{color.G},{color.B},{color.A}";
            else
                return $"{color.R},{color.G},{color.B}";
        }

        public static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new Exception(message);
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

        /// <summary>
        /// Gets list containing only unique items.
        /// </summary>
        public static List<T> GetUniqueList<T>(IList<T> original)
        {
            List<T> unique = new List<T>(original.Count);

            for (int n = 0; n < original.Count; n++)
            {
                if (!unique.Contains(original[n]))
                    unique.Add(original[n]);
            }

            return unique;
        }
    }
}
