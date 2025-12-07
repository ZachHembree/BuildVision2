using System;
using VRageMath;

namespace RichHudFramework
{
	/// <summary>
	/// Mathematical utilities, particularly work-arounds for missing BitConverter functionality in Space Engineers mod API.
	/// </summary>
	public static class MathUtils
    {
        /// <summary>
        /// Approximates a float to IEEE 754 single-precision (32-bit) bit encoding using truncation and clamping.
        /// Lossy conversion, not perfectly reversable with Int32ToFloat(). 
        /// 
        /// Created as an alternative to BitConverter single precision conversions not in Space Engineers mod API whitelist.
        /// 
        /// Does not handle denormalized values, perform rounding (truncates mantissa), or special case handling 
        /// (Inf/NaN approximated).
        /// 
        /// </summary>
        /// <param name="invertSignBit">Set true to flip the sign bit s.t. sign bit is set for positives 
        /// and zero for negatives. Useful for sorting keys.</param>
        /// <returns>Approximate bitwise representation of the float value.</returns>
        public static uint FloatToInt32Bits(float value, bool invertSignBit = false)
        {
            ulong bits = (ulong)BitConverter.DoubleToInt64Bits(value);
            ulong sign = bits >> 63;
            ulong exp = (bits >> 52) & 0x7FFUL;
            ulong mant = bits & 0xFFFFFFFFFFFFFUL;
            int trueExp = (int)exp - 1023;
            
            int clampedTrueExp = Math.Max(-126, Math.Min(trueExp, 127));

            if (invertSignBit)
                sign = ~sign & 1ul;

            ulong singleExp = (ulong)(clampedTrueExp + 127) << 23;
            ulong singleMant = mant >> 29;
            ulong singleSign = sign << 31;
            uint singleBits = (uint)(singleSign | singleExp | singleMant);

            return singleBits;
        }

        /// <summary>
        /// Reconstitutes a float from an approximate single-precision (32-bit) bit encoding. 
        /// Lossy conversion, not perfectly reversable with FloatToInt32Bits().
        /// 
        /// Does not handle denormalized values, perform rounding (truncates mantissa) or special case handling 
        /// (Inf/NaN approximated). 
        /// </summary>
        /// <param name="isSignInverted">Indicates if the sign bit was inverted and needs to be flipped</param>
        /// <returns>Float value from approximate 32bit bitwise representation.</returns>
        public static float Int32ToFloat(uint bits, bool isSignInverted = false)
        {
            // Decompose into sign, exponent and mantissa
            ulong sign = (ulong)(bits >> 31);
            ulong exp = (ulong)(bits >> 23) & 0xFFUL;
            ulong mant = (ulong)(bits & 0x7FFFFFU);

            if (isSignInverted)
                sign = ~sign & 1ul;

            // Reconstitute 64 bit representations of each component
            ulong doubleSign = sign << 63;
            ulong doubleMant = mant << 29;
            uint originalExp = (uint)exp - 127;
            ulong doubleExp = (ulong)(originalExp + 1023) << 52;

            // Rebuild 64bit value and convert
            ulong doubleBits = doubleSign | doubleExp | doubleMant;
            double dValue = BitConverter.Int64BitsToDouble((long)doubleBits);

            // Cast down to fp32
            return (float)dValue;
        }
    }
}
