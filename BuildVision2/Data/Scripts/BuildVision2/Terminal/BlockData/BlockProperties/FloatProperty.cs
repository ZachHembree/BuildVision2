using RichHudFramework;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        /// <summary>
        /// Block Terminal Property of a Float
        /// </summary>
        private class FloatProperty : NumericPropertyBase<float>
        {
            public override string Value
            {
                get
                {
                    float value = GetValue();

                    if ((value.Abs() >= 1000000f || value.Abs() <= .0000001f) && value != 0f)
                        return value.ToString("0.##E+0");
                    else
                        return value.ToString("0.##");
                }
            }
            public override string Postfix => GetPostfixFunc != null ? GetPostfixFunc() : null;

            private readonly float minValue, maxValue, incrX, incrY, incrZ, incr0;
            private readonly Func<string> GetPostfixFunc;

            public FloatProperty(string name, ITerminalProperty<float> property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            {
                minValue = property.GetMinimum(block);
                maxValue = property.GetMaximum(block);

                if (property.Id.StartsWith("Rot")) // Increment exception for projectors
                    incr0 = 90f;
                else
                {
                    if (float.IsInfinity(minValue) || float.IsInfinity(maxValue))
                        incr0 = 1f;
                    else
                    {
                        double range = Math.Abs(maxValue - minValue), exp;

                        if (range > maxValue)
                            exp = Math.Truncate(Math.Log10(range));
                        else
                            exp = Math.Truncate(Math.Log10(2 * range));

                        incr0 = (float)(Math.Pow(10d, exp) / Cfg.floatDiv);
                    }

                    if (incr0 == 0)
                        incr0 = 1f;
                }

                incrZ = incr0 * Cfg.floatMult.Z; // x10
                incrY = incr0 * Cfg.floatMult.Y; // x5
                incrX = incr0 * Cfg.floatMult.X; // x0.1

                if (property.Id == "UpperLimit")
                {
                    if (block is IMyPistonBase)
                    {
                        var piston = (IMyPistonBase)block;
                        GetPostfixFunc = () => $"({Math.Round(piston.CurrentPosition, 1)}m)";
                    }
                    else if (block is IMyMotorStator)
                    {
                        var rotor = (IMyMotorStator)block;
                        GetPostfixFunc = () => $"({Math.Round(MathHelper.Clamp(rotor.Angle.RadiansToDegrees(), -360, 360))})";
                    }
                }
            }

            public override void ScrollDown() =>
                ChangePropValue(-GetIncrement());

            public override void ScrollUp() =>
                ChangePropValue(+GetIncrement());

            public override bool TryParseValue(string text, out float value) =>
                float.TryParse(text, out value);

            /// <summary>
            /// Changes property float value based on given delta.
            /// </summary>
            private void ChangePropValue(float delta)
            {
                float current = GetValue();

                if (float.IsInfinity(current))
                    current = 0f;

                SetValue((float)Math.Round(MathHelper.Clamp((current + delta), minValue, maxValue), 3));
            }

            /// <summary>
            /// Gets value to add or subtract from the property based on multipliers used.
            /// </summary>
            private float GetIncrement()
            {
                if (BvBinds.MultZ.IsPressed)
                    return incrZ;
                else if (BvBinds.MultY.IsPressed)
                    return incrY;
                else if (BvBinds.MultX.IsPressed)
                    return incrX;
                else
                    return incr0;
            }
        }
    }
}