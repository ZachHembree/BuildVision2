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
    }
}
