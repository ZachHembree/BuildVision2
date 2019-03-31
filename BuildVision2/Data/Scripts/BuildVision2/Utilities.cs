
namespace DarkHelmet.BuildVision2
{
    internal static class Utilities
    {
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
    }
}
