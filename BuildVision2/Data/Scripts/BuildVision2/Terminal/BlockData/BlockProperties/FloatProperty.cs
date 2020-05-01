using RichHudFramework;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Text;
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
            public override string Display
            {
                get 
                {
                    if (SliderWriter != null)
                    {
                        writerText.Clear();
                        SliderWriter(block.TBlock, writerText);
                        return writerText.ToString();
                    }
                    else
                        return Value;
                } 
            }

            public override string Value => GetValue().Round(4).ToString("G6");

            public override string Status => GetStatusFunc?.Invoke() ?? "";

            private readonly Func<string> GetStatusFunc;
            private readonly Action<IMyTerminalBlock, StringBuilder> SliderWriter;
            private readonly StringBuilder writerText;
            private readonly float minValue, maxValue, increment;
            private readonly Func<float> GetScaleFunc;

            public FloatProperty(string name, ITerminalProperty<float> property, IMyTerminalControl control, SuperBlock block) : base(name, property, control, block)
            {
                var slider = control as IMyTerminalControlSlider;
                SliderWriter = slider?.Writer;
                writerText = new StringBuilder();

                minValue = property.GetMinimum(block.TBlock);
                maxValue = property.GetMaximum(block.TBlock);
                increment = GetIncrement();

                if (block.SubtypeId.UsesSubtype(TBlockSubtypes.Thruster) && PropName == "Override")
                    GetScaleFunc = () => block.Thruster.ThrustEffectiveness;
                else
                    GetScaleFunc = () => 1f;

                if (property.Id == "UpperLimit")
                {
                    if (block.SubtypeId.UsesSubtype(TBlockSubtypes.Piston))
                        GetStatusFunc = () => $"({block.Piston.ExtensionDist.Round(2)}m)";
                    else if (block.SubtypeId.UsesSubtype(TBlockSubtypes.Rotor))
                        GetStatusFunc = () => $"({MathHelper.Clamp(block.Rotor.Angle.RadiansToDegrees(), -360, 360).Round(2)}°)";
                }
            }

            public override void ScrollDown() =>
                ChangePropValue(-GetStep());

            public override void ScrollUp() =>
                ChangePropValue(+GetStep());

            public override float GetValue() =>
                base.GetValue() * GetScaleFunc();

            public override void SetValue(float value) =>
                base.SetValue((value / GetScaleFunc()).Round(6));

            public override bool TryParseValue(string text, out float value) =>
                float.TryParse(text, out value);

            private float GetIncrement()
            {
                float increment;

                if (property.Id.StartsWith("Rot")) // Increment exception for projectors
                    increment = 90f;
                else
                {
                    if (float.IsInfinity(minValue) || float.IsInfinity(maxValue))
                        increment = 1f;
                    else
                    {
                        double range = Math.Abs(maxValue - minValue), exp;

                        if (minValue != 0f && maxValue != 0f)
                            exp = Math.Truncate(Math.Log10(1.1 * range));
                        else
                            exp = Math.Truncate(Math.Log10(2.2 * range));

                        increment = (float)(Math.Pow(10d, exp) / Cfg.floatDiv);
                    }

                    if (increment == 0)
                        increment = 1f;
                }

                return increment;
            }

            /// <summary>
            /// Gets value to add or subtract from the property based on multipliers used.
            /// </summary>
            private float GetStep()
            {
                float inc;

                if (BvBinds.MultZ.IsPressed)
                    inc = increment * Cfg.floatMult.Z;
                else if (BvBinds.MultY.IsPressed)
                    inc = increment * Cfg.floatMult.Y;
                else if (BvBinds.MultX.IsPressed)
                    inc = increment * Cfg.floatMult.X;
                else
                    inc = increment;

                return inc.Round(3);
            }

            /// <summary>
            /// Changes property float value based on given delta.
            /// </summary>
            private void ChangePropValue(float value)
            {
                float current = GetValue();

                if (float.IsInfinity(current))
                    current = 0f;

                SetValue((float)Math.Round(MathHelper.Clamp((current + value), minValue, maxValue), 4));
            }
        }
    }
}