using Sandbox.ModAPI;
using System;
using System.Text.RegularExpressions;
using VRageMath;

namespace RichHudFramework
{
    public static partial class Utils
    {
        /// <summary>
        /// Simple stopwatch class in lieu of <see cref="System.Diagnostics.Stopwatch"/>.
        /// </summary>
        public class Stopwatch
        {
            public long ElapsedTicks { get { return Enabled ? (DateTime.Now.Ticks - startTime) : (stopTime - startTime); } }
            public long ElapsedMilliseconds { get { return ElapsedTicks / TimeSpan.TicksPerMillisecond; } }
            public bool Enabled { get; private set; }

            private long startTime, stopTime;

            public Stopwatch()
            {
                startTime = long.MaxValue;
                stopTime = long.MaxValue;
                Enabled = false;
            }

            public void Start()
            {
                Reset();
                Enabled = true;
            }

            public void Stop()
            {
                stopTime = DateTime.Now.Ticks;
                Enabled = false;
            }

            public void Reset()
            {
                startTime = DateTime.Now.Ticks;
            }
        }

        public static class Xml
        {
            /// <summary>
            /// Attempts to serialize an object to an Xml string.
            /// </summary>
            public static KnownException TrySerialize<T>(T obj, out string xmlOut)
            {
                KnownException exception = null;
                xmlOut = null;

                try
                {
                    xmlOut = MyAPIGateway.Utilities.SerializeToXML(obj);
                }
                catch (Exception e)
                {
                    exception = new KnownException("IO Error. Failed to generate XML.", e);
                }

                return exception;
            }

            /// <summary>
            /// Attempts to deserialize an Xml string to an object of a given type.
            /// </summary>
            public static KnownException TryDeserialize<T>(string xmlIn, out T obj)
            {
                KnownException exception = null;
                obj = default(T);

                try
                {
                    obj = MyAPIGateway.Utilities.SerializeFromXML<T>(xmlIn);
                }
                catch (Exception e)
                {
                    exception = new KnownException("IO Error. Unable to interpret XML.", e);
                }

                return exception;
            }
        }

        public static class Color
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
            public static bool TryParseColor(string colorData, out VRageMath.Color value, bool ignoreAlpha = false)
            {
                bool successful;

                try
                {
                    value = ParseColor(colorData, ignoreAlpha);
                    successful = true;
                }
                catch
                {
                    value = VRageMath.Color.White;
                    successful = false;
                }

                return successful;
            }

            /// <summary>
            /// Converts a string of color values to its <see cref="VRageMath.Color"/> equivalent.
            /// </summary>
            public static VRageMath.Color ParseColor(string colorData, bool ignoreAlpha = false)
            {
                Match match = colorParser.Match(colorData);
                CaptureCollection captures = match.Groups[2].Captures;
                VRageMath.Color value = new VRageMath.Color();

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

            public static string GetColorString(VRageMath.Color color, bool includeAlpha = true)
            {
                if (includeAlpha)
                    return $"{color.R},{color.G},{color.B},{color.A}";
                else
                    return $"{color.R},{color.G},{color.B}";
            }
        }

        public static class Debug
        {
            public static void AssertNotNull<T>(T obj, string message = "")
            {
                Assert(obj != null, $"Object of type {typeof(T).ToString()} is null. " + message);
            }

            public static void Assert(bool condition, string message = "")
            {
                if (!condition)
                    throw new Exception("Assertion failed. " + message);
            }
        }

        public static class Math
        {
            /// <summary>
            /// Clamps a <see cref="double"/> between two values.
            /// </summary>
            public static double Clamp(double value, double min, double max)
            {
                if (value <= min)
                    return min;
                else if (value > max)
                    return max;
                else
                    return value;
            }

            /// <summary>
            /// Clamps a <see cref="float"/> between two values.
            /// </summary>
            public static float Clamp(float value, float min, float max)
            {
                if (value <= min)
                    return min;
                else if (value > max)
                    return max;
                else
                    return value;
            }

            /// <summary>
            /// Clamps an <see cref="byte"/> between two values.
            /// </summary>
            public static byte Clamp(byte value, byte min, byte max)
            {
                if (value <= min)
                    return min;
                else if (value > max)
                    return max;
                else
                    return value;
            }

            /// <summary>
            /// Clamps an <see cref="int"/> between two values.
            /// </summary>
            public static int Clamp(int value, int min, int max)
            {
                if (value <= min)
                    return min;
                else if (value > max)
                    return max;
                else
                    return value;
            }

            /// <summary>
            /// Clamps a <see cref="long"/> between two values.
            /// </summary>
            public static long Clamp(long value, long min, long max)
            {
                if (value <= min)
                    return min;
                else if (value > max)
                    return max;
                else
                    return value;
            }

            /// <summary>
            /// Returns a <see cref="Vector2I"/> of the largest members between the two.
            /// </summary>
            public static Vector2I Min(Vector2I val1, Vector2I val2)
            {
                return new Vector2I()
                {
                    X = System.Math.Min(val1.X, val2.X),
                    Y = System.Math.Min(val1.Y, val2.Y)
                };
            }

            /// <summary>
            /// Returns a <see cref="Vector2D"/> of the largest members between the two.
            /// </summary>
            public static Vector2D Min(Vector2D val1, Vector2D val2)
            {
                return new Vector2D()
                {
                    X = System.Math.Min(val1.X, val2.X),
                    Y = System.Math.Min(val1.Y, val2.Y)
                };
            }

            /// <summary>
            /// Returns a <see cref="Vector2"/> of the largest members between the two.
            /// </summary>
            public static Vector2 Min(Vector2 val1, Vector2 val2)
            {
                return new Vector2()
                {
                    X = System.Math.Min(val1.X, val2.X),
                    Y = System.Math.Min(val1.Y, val2.Y)
                };
            }

            /// <summary>
            /// Returns a <see cref="Vector2I"/> of the largest members between the two.
            /// </summary>
            public static Vector2I Max(Vector2I val1, Vector2I val2)
            {
                return new Vector2I()
                {
                    X = System.Math.Max(val1.X, val2.X),
                    Y = System.Math.Max(val1.Y, val2.Y)
                };
            }

            /// <summary>
            /// Returns a <see cref="Vector2D"/> of the largest members between the two.
            /// </summary>
            public static Vector2D Max(Vector2D val1, Vector2D val2)
            {
                return new Vector2D()
                {
                    X = System.Math.Max(val1.X, val2.X),
                    Y = System.Math.Max(val1.Y, val2.Y)
                };
            }

            /// <summary>
            /// Returns a <see cref="Vector2"/> of the largest members between the two.
            /// </summary>
            public static Vector2 Max(Vector2 val1, Vector2 val2)
            {
                return new Vector2()
                {
                    X = System.Math.Max(val1.X, val2.X),
                    Y = System.Math.Max(val1.Y, val2.Y)
                };
            }

            /// <summary>
            /// Clamps a <see cref="Vector2I"/> members between two values.
            /// </summary>
            public static Vector2I Clamp(Vector2I value, int min, int max)
            {
                value.X = Clamp(value.X, min, max);
                value.Y = Clamp(value.Y, min, max);

                return value;
            }

            /// <summary>
            /// Clamps a <see cref="Vector2D"/> members between two values.
            /// </summary>
            public static Vector2D Clamp(Vector2D value, double min, double max)
            {
                value.X = Clamp(value.X, min, max);
                value.Y = Clamp(value.Y, min, max);

                return value;
            }

            /// <summary>
            /// Clamps a <see cref="Vector2"/> members between two values.
            /// </summary>
            public static Vector2 Clamp(Vector2 value, float min, float max)
            {
                value.X = Clamp(value.X, min, max);
                value.Y = Clamp(value.Y, min, max);

                return value;
            }

            /// <summary>
            /// Clamps a <see cref="Vector2I"/> members between two values.
            /// </summary>
            public static Vector2I Clamp(Vector2I value, Vector2I min, Vector2I max)
            {
                value.X = Clamp(value.X, min.X, max.X);
                value.Y = Clamp(value.Y, min.Y, max.Y);

                return value;
            }

            /// <summary>
            /// Clamps a <see cref="Vector2D"/> members between two values.
            /// </summary>
            public static Vector2D Clamp(Vector2D value, Vector2D min, Vector2D max)
            {
                value.X = Clamp(value.X, min.X, max.X);
                value.Y = Clamp(value.Y, min.Y, max.Y);

                return value;
            }

            /// <summary>
            /// Clamps a <see cref="Vector2"/> members between two values.
            /// </summary>
            public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
            {
                value.X = Clamp(value.X, min.X, max.X);
                value.Y = Clamp(value.Y, min.Y, max.Y);

                return value;
            }

            /// <summary>
            /// Rounds <see cref="Vector2D"/> elements to a specified number of digits.
            /// </summary>
            public static Vector2D Truncate(Vector2D value)
            {
                value.X = System.Math.Truncate(value.X);
                value.Y = System.Math.Truncate(value.Y);
                return value;
            }

            /// <summary>
            /// Rounds <see cref="Vector2"/> elements to a specified number of digits.
            /// </summary>
            public static Vector2 Round(Vector2 value, int digits = 0)
            {
                value.X = (float)System.Math.Round(value.X, digits);
                value.Y = (float)System.Math.Round(value.Y, digits);
                return value;
            }

            /// <summary>
            /// Rounds <see cref="Vector2D"/> elements to a specified number of digits.
            /// </summary>
            public static Vector2D Round(Vector2D value, int digits = 0)
            {
                value.X = System.Math.Round(value.X, digits);
                value.Y = System.Math.Round(value.Y, digits);
                return value;
            }

            /// <summary>
            /// Rounds <see cref="Vector3D"/> elements to a specified number of digits.
            /// </summary>
            public static Vector3D Round(Vector3D value, int digits = 0)
            {
                value.X = System.Math.Round(value.X, digits);
                value.Y = System.Math.Round(value.Y, digits);
                value.Z = System.Math.Round(value.Z, digits);
                return value;
            }

            /// <summary>
            /// Finds the absolute value of the components of a <see cref="Vector2I"/>.
            /// </summary>
            public static Vector2I Abs(Vector2I value)
            {
                value.X = System.Math.Abs(value.X);
                value.Y = System.Math.Abs(value.Y);
                return value;
            }

            /// <summary>
            /// Finds the absolute value of the components of a <see cref="Vector2"/>.
            /// </summary>
            public static Vector2 Abs(Vector2 value)
            {
                value.X = System.Math.Abs(value.X);
                value.Y = System.Math.Abs(value.Y);
                return value;
            }

            /// <summary>
            /// Finds the absolute value of the components of a <see cref="Vector2D"/>.
            /// </summary>
            public static Vector2D Abs(Vector2D value)
            {
                value.X = System.Math.Abs(value.X);
                value.Y = System.Math.Abs(value.Y);
                return value;
            }
        }
    }
}
