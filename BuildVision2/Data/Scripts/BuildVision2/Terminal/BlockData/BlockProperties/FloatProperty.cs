using RichHudFramework;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using VRage;
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
            public override string PropName { get; }

            public override string Value => GetValue().ToString("G6");

            public override string Postfix { get; }

            public override string Status => GetStatusFunc?.Invoke() ?? "";

            private readonly float minValue, maxValue, incrX, incrY, incrZ, incr0;
            private readonly Func<string> GetStatusFunc;
            private float scale;

            public FloatProperty(string name, ITerminalProperty<float> property, IMyTerminalControl control, SuperBlock block) : base(name, property, control, block)
            {
                PropName = $"{block.SubtypeId.GetLargestSubtype()}_{property.Id}";

                FloatValueUnits units = GetUnitType();
                Postfix = TerminalExtensions.FloatUnitPostfixes[units];
                scale = GetScale(units);

                minValue = property.GetMinimum(block.TBlock) * scale;
                maxValue = property.GetMaximum(block.TBlock) * scale;

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
                    if (block.SubtypeId.UsesSubtype(TBlockSubtypes.Piston))
                        GetStatusFunc = () => $"({Math.Round(block.Piston.ExtensionDist, 2)}m)";
                    else if (block.SubtypeId.UsesSubtype(TBlockSubtypes.Rotor))
                        GetStatusFunc = () => $"({Math.Round(MathHelper.Clamp(block.Rotor.Angle.RadiansToDegrees(), -360, 360), 2)}°)";
                }
            }

            public override void ScrollDown() =>
                ChangePropValue(-GetIncrement());

            public override void ScrollUp() =>
                ChangePropValue(+GetIncrement());

            public override float GetValue() =>
                base.GetValue() * scale;

            public override void SetValue(float value) =>
                base.SetValue(value / scale);

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
                
                SetValue((float)Math.Round(MathHelper.Clamp((current + delta), minValue, maxValue), 4));
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

            private FloatValueUnits GetUnitType()
            {
                FloatValueUnits unit = FloatValueUnits.None;
                TerminalExtensions.FloatPropertySubtypes.TryGetValue(PropName, out unit);

                return unit;
            }

            private static float GetScale(FloatValueUnits units)
            {
                if (units == FloatValueUnits.Ratio)
                    return 100f;
                else
                    return 1f;
            }
        }
    }
}