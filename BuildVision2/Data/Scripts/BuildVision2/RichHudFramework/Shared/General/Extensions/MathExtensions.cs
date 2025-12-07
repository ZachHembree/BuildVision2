﻿using System;

namespace RichHudFramework
{
    public static class MathExtensions
    {
        /// <summary>
        /// Rounds a double to the given number of decimal places using banker's rounding (MidpointRounding.ToEven).
        /// </summary>
        public static double Round(this double value, int digits = 0) => Math.Round(value, digits);

        /// <summary>
        /// Rounds a float to the given number of decimal places using banker's rounding.
        /// </summary>
        public static float Round(this float value, int digits = 0) => (float)Math.Round(value, digits);

        /// <summary>
        /// Returns the absolute value of a float.
        /// </summary>
        public static float Abs(this float value) => Math.Abs(value);

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        public static float RadiansToDegrees(this float value) => value * 180f / (float)Math.PI;

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        public static float DegreesToRadians(this float value) => value * (float)Math.PI / 180f;
    }
}