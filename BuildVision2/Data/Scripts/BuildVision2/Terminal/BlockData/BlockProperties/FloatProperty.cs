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
        private class FloatProperty : NumericPropertyBase<float>, IBlockNumericValue<float>
        {
            /// <summary>
            /// Maximum allowable value
            /// </summary>
            public float MaxValue { get; private set; }

            /// <summary>
            /// Minimum allowable value
            /// </summary>
            public float MinValue { get; private set; }

            /// <summary>
            /// Standard increment
            /// </summary>
            public float Increment { get; private set; }

            /// <summary>
            /// Retrieves the value as a <see cref="StringBuilder"/> using formatting specific to the member.
            /// </summary>
            public override StringBuilder FormattedValue
            {
                get 
                {
                    if (SliderWriter != null)
                    {
                        return GetFormattedValue();
                    }
                    else
                        return ValueText;
                } 
            }

            /// <summary>
            /// Retrieves the current value of the block member as an unformatted <see cref="StringBuilder"/>
            /// </summary>
            public override StringBuilder ValueText 
            {
                get 
                {
                    valueBuilder.Clear();
                    valueBuilder.AppendFormat("{0:G6}", Math.Round(Value, 4));

                    return valueBuilder;
                }
            }

            /// <summary>
            /// Additional information following the value of the member.
            /// </summary>
            public override StringBuilder StatusText 
            {
                get 
                {
                    if (GetStatusFunc != null)
                    {
                        GetStatusFunc(statusBuilder);
                        return statusBuilder;
                    }
                    else
                        return null;
                }
            }

            private static readonly Func<float> GetDefaultScaleFunc;

            private readonly StringBuilder fmtValueBuilder, valueBuilder, statusBuilder, unitBuilder;
            private readonly Action<StringBuilder> GetPistonExtensionFunc, GetRotorAngleFunc;
            private readonly Func<float> GetThrustEffectFunc;

            private Action<IMyTerminalBlock, StringBuilder> SliderWriter;
            private Action<StringBuilder> GetStatusFunc;
            private Func<float> GetScaleFunc;
            private BvPropPool<FloatProperty> poolParent;

            static FloatProperty()
            {
                GetDefaultScaleFunc = () => 1f;
            }

            public FloatProperty()
            {
                fmtValueBuilder = new StringBuilder();
                valueBuilder = new StringBuilder();
                statusBuilder = new StringBuilder();
                unitBuilder = new StringBuilder();

                GetPistonExtensionFunc = GetPistonExtension;
                GetRotorAngleFunc = GetRotorAngle;
                GetThrustEffectFunc = GetThrustEffect;
                ValueType = BlockMemberValueTypes.Float;
            }

            public override void SetProperty(StringBuilder name, ITerminalProperty<float> property, PropertyBlock block)
            {
                base.SetProperty(name, property, block);

                if (poolParent == null)
                    poolParent = block.floatPropPool;

                var slider = control as IMyTerminalControlSlider;
                SliderWriter = slider?.Writer;

                Flags = BlockPropertyFlags.None;

                // Why the hell is this so damn broken?
                if (isBuildAndRepair)
                {
                    MinValue = -1000f;
                    MaxValue = 1000f;
                    Increment = 1f;
                }
                else
                {
                    MinValue = property.GetMinimum(block.TBlock);
                    MaxValue = property.GetMaximum(block.TBlock);
                    Increment = GetIncrement();
                }

                if (block.SubtypeId.UsesSubtype(TBlockSubtypes.Thruster) && PropName == "Override")
                    GetScaleFunc = GetThrustEffectFunc;
                else
                    GetScaleFunc = GetDefaultScaleFunc;

                if (property.Id == "Velocity")
                {
                    if (block.SubtypeId.UsesSubtype(TBlockSubtypes.Rotor))
                        GetStatusFunc = GetRotorAngleFunc;
                    else if (block.SubtypeId.UsesSubtype(TBlockSubtypes.Piston))
                        GetStatusFunc = GetPistonExtensionFunc;
                }

                if (property.Id == "X" || property.Id == "Y" || property.Id == "Z" || property.Id.StartsWith("Rot"))
                    Flags |= BlockPropertyFlags.IsIntegral;
            }

            public override void Reset()
            {
                base.Reset();

                GetStatusFunc = null;
                GetScaleFunc = null;
                fmtValueBuilder.Clear();
            }

            public override void Return()
            {
                poolParent.Return(this);
            }

            public static FloatProperty GetProperty(StringBuilder name, ITerminalProperty<float> property, PropertyBlock block)
            {
                FloatProperty prop = block.floatPropPool.Get();
                prop.SetProperty(name, property, block);

                return prop;
            }

            protected override float GetValue()
            {
                float value = base.GetValue() * GetScaleFunc();
                return value != float.NaN ? value : 0f;
            }

            protected override void SetValue(float value)
            {
                if (value != float.NaN)
                {
                    float effectiveness = GetScaleFunc(),
                        rcpEffectiveness = effectiveness > 0f ? (1f / effectiveness) : 1f;

                    value = (float)Math.Round(value * rcpEffectiveness, 5);
                    base.SetValue((float)MathHelper.Clamp(value, MinValue, MaxValue));
                }
            }

            public override bool TryParseValue(string text, out float value) =>
                float.TryParse(text, out value);

            private StringBuilder GetFormattedValue()
            {
                int numLength = 0;
                bool nonCharPostfix = false;

                // Get formatted text
                fmtValueBuilder.Clear();
                SliderWriter(block.TBlock, fmtValueBuilder);

                // Parse out num length and postfix, if they exist
                unitBuilder.Clear();

                for (int i = 0; i < fmtValueBuilder.Length; i++)
                {
                    if (fmtValueBuilder[i] > ' ')
                    {
                        if (unitBuilder.Length == 0 && fmtValueBuilder[i].IsNumeric())
                        {
                            numLength++;
                        }
                        else
                        {
                            unitBuilder.Append(fmtValueBuilder[i]);

                            if (!nonCharPostfix && !fmtValueBuilder[i].IsAlphabetical())
                                nonCharPostfix = true;
                        }
                    }
                }

                if (numLength > 0)
                {
                    float value = GetValue(),
                        magnitude = -1f;

                    if (unitBuilder.Length > 0)
                    {
                        // Assumes metric-prefixed units, meaning at least two alphabetical characters (sans spacing),
                        // where the first char is the prefix and the characters following are the unit type
                        bool isMetricPrefixValue = !nonCharPostfix && unitBuilder.Length > 1 && unitBuilder[1].IsAlphabetical() 
                            && TerminalUtilities.MetricPrefixMagTable.TryGetValue(unitBuilder[0], out magnitude);

                        if (!isMetricPrefixValue)
                        {
                            foreach (string prefix in TerminalUtilities.SpecialPrefixes)
                            {
                                if (unitBuilder.IsTextEqual(prefix))
                                {
                                    magnitude = 1f;
                                    break;
                                }
                            }
                        }
                    }
                    else
                        magnitude = 1f;

                    if (magnitude > 0f)
                    {
                        fmtValueBuilder.Clear();
                        fmtValueBuilder.AppendFormat("{0:G5}", Math.Round(value / magnitude, 3));
                        fmtValueBuilder.Append(' ');
                        fmtValueBuilder.Append(unitBuilder);
                    }
                }

                return fmtValueBuilder;
            }

            private float GetThrustEffect() =>
                block.Thruster.ThrustEffectiveness;

            private void GetPistonExtension(StringBuilder sb)
            {
                sb.Clear();
                sb.Append('(');
                sb.Append(Math.Round(block.Piston.ExtensionDist, 2));
                sb.Append("m)");
            }

            private void GetRotorAngle(StringBuilder sb)
            {
                sb.Clear();
                sb.Append('(');
                sb.Append(Math.Round(MathHelper.Clamp(block.Rotor.Angle.RadiansToDegrees(), -360, 360), 2));
                sb.Append("°)");
            }

            private float GetIncrement()
            {
                float increment;

                if (property.Id.StartsWith("Rot"))
                {
                    increment = 90f;
                }
                else
                {
                    Flags |= BlockPropertyFlags.CanUseMultipliers;

                    if (float.IsInfinity(MinValue) || float.IsInfinity(MaxValue))
                        increment = 1f;
                    else
                    {
                        double range = Math.Abs(MaxValue - MinValue), exp;

                        if (MinValue != 0f && MaxValue != 0f)
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
        }
    }
}