using System;
using System.Text.RegularExpressions;

namespace RichHudFramework
{
	public static partial class Utils
	{
		/// <summary>
		/// Utilities for parsing and formatting <see cref="VRageMath.Color"/> values from/to strings.
		/// Supports common formats such as "255,255,255", " 100,  50, 0 ", or "128,64,32,255".
		/// </summary>
		public static class Color
		{
			private static readonly Regex colorParser = new Regex(@"(\s*,?(\d{1,3})\s*,?){3,4}");

			/// <summary>
			/// Determines whether the given string can be successfully parsed as a color (3–4 byte components).
			/// </summary>
			/// <param name="colorData">The string to test.</param>
			/// <returns>True if the string contains 3 or 4 valid byte values; otherwise false.</returns>
			public static bool CanParseColor(string colorData)
			{
				if (string.IsNullOrEmpty(colorData))
					return false;

				Match match = colorParser.Match(colorData);
				CaptureCollection captures = match.Groups[2].Captures;

				if (captures.Count < 3)
					return false;

				for (int i = 0; i < Math.Min(4, captures.Count); i++)
				{
					byte value;

					if (!byte.TryParse(captures[i].Value, out value))
						return false;
				}

				return true;
			}

			/// <summary>
			/// Attempts to parse a string into a <see cref="VRageMath.Color"/>. Returns true on success.
			/// </summary>
			/// <param name="colorData">String containing 3–4 numeric components.</param>
			/// <param name="value">Receives the parsed color on success; white on failure.</param>
			/// <param name="ignoreAlpha">If true and only 3 components are provided, the alpha is defaulted to 255.</param>
			/// <returns>True if parsing succeeded.</returns>
			public static bool TryParseColor(string colorData, out VRageMath.Color value, bool ignoreAlpha = false)
			{
				try
				{
					value = ParseColor(colorData, ignoreAlpha);
					return true;
				}
				catch
				{
					value = VRageMath.Color.White;
					return false;
				}
			}

			/// <summary>
			/// Parses a string into a <see cref="VRageMath.Color"/>. Throws on invalid input.
			/// </summary>
			/// <param name="colorData">String containing 3–4 numeric components separated by commas.</param>
			/// <param name="ignoreAlpha">If true and only 3 components are provided, the alpha is defaulted to 255.</param>
			/// <returns>The parsed color.</returns>
			/// <exception cref="Exception">Thrown when the string is malformed or contains invalid values.</exception>
			public static VRageMath.Color ParseColor(string colorData, bool ignoreAlpha = false)
			{
				if (string.IsNullOrEmpty(colorData))
					throw new ArgumentException("Color string cannot be null or empty.");

				Match match = colorParser.Match(colorData);
				CaptureCollection captures = match.Groups[2].Captures;

				if (captures.Count < 3)
					throw new Exception("Color string must contain at least 3 values (R,G,B).");

				VRageMath.Color value = new VRageMath.Color
				{
					R = byte.Parse(captures[0].Value),
					G = byte.Parse(captures[1].Value),
					B = byte.Parse(captures[2].Value)
				};

				if (captures.Count > 3)
					value.A = byte.Parse(captures[3].Value);
				else if (!ignoreAlpha)
					value.A = 255; // default opaque when alpha omitted

				return value;
			}

			/// <summary>
			/// Converts a <see cref="VRageMath.Color"/> to a simple comma-separated string.
			/// </summary>
			/// <param name="color">The color to format.</param>
			/// <param name="includeAlpha">If false, the alpha component is omitted.</param>
			/// <returns>A string like "255,255,255" or "255,255,255,128".</returns>
			public static string GetColorString(VRageMath.Color color, bool includeAlpha = true)
			{
				return includeAlpha
					? $"{color.R},{color.G},{color.B},{color.A}"
					: $"{color.R},{color.G},{color.B}";
			}
		}
	}
}