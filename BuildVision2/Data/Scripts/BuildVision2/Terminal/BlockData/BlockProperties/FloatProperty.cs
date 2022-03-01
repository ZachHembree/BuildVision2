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
            public float Value { get { return GetValue(); } set { SetValue(value); } }

            public float MaxValue { get; private set; }

            public float MinValue { get; private set; }

            public float Increment { get; private set; }

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

            public override StringBuilder ValueText 
            {
                get 
                {
                    valueBuilder.Clear();
                    valueBuilder.AppendFormat("{0:G6}", Math.Round(GetValue(), 4));

                    return valueBuilder;
                }
            }

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
                
                MinValue = property.GetMinimum(block.TBlock);
                MaxValue = property.GetMaximum(block.TBlock);
                Increment = GetIncrement();

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

            public override float GetValue()
            {
                float value = base.GetValue() * GetScaleFunc();
                return value != float.NaN ? value : 0f;
            }

            public override void SetValue(float value)
            {
                if (value != float.NaN)
                {
                    float effectiveness = GetScaleFunc(),
                        rcpEffectiveness = effectiveness > 0f ? (1f / effectiveness) : 1f;

                    base.SetValue(MathHelper.Clamp((value * rcpEffectiveness).Round(6), MinValue, MaxValue));
                }
            }

            public override bool TryParseValue(string text, out float value) =>
                float.TryParse(text, out value);

            private StringBuilder GetFormattedValue()
            {
                // Get formatted text
                fmtValueBuilder.Clear();
                SliderWriter(block.TBlock, fmtValueBuilder);

                int numLength = 0;
                bool nonCharPostfix = false;
                unitBuilder.Clear();

                // Parse out num length and postfix, if they exist
                for (int i = 0; i < fmtValueBuilder.Length; i++)
                {
                    if (fmtValueBuilder[i] > ' ')
                    {
                        if (unitBuilder.Length == 0 && ((fmtValueBuilder[i] >= '0' && fmtValueBuilder[i] <= '9') ||
                            fmtValueBuilder[i] == 'E' || fmtValueBuilder[i] == 'e' || fmtValueBuilder[i] == '-' ||
                            fmtValueBuilder[i] == '+' || fmtValueBuilder[i] == '.' || fmtValueBuilder[i] == ':')
                        )
                        {
                            numLength++;
                        }
                        else
                        {
                            unitBuilder.Append(fmtValueBuilder[i]);

                            if (!((fmtValueBuilder[i] >= 'A' && fmtValueBuilder[i] <= 'Z') ||
                                (fmtValueBuilder[i] >= 'a' && fmtValueBuilder[i] <= 'z'))
                            )
                            {
                                nonCharPostfix = true;
                            }
                        }
                    }
                }

                if (numLength > 0 && unitBuilder.Length > 0)
                {
                    float value = GetValue(),
                        magnitude = -1f;

                    // Assumes metric-prefixed units, meaning at least two alphabetical characters (sans spacing),
                    // where the first char is the prefix and the characters following are the unit type
                    bool isMetricPrefixValue = !nonCharPostfix && unitBuilder.Length > 1 &&
                        ((unitBuilder[1] >= 'A' && unitBuilder[1] <= 'Z') ||
                        (unitBuilder[1] >= 'a' && unitBuilder[1] <= 'z')) &&
                        TerminalUtilities.MetricPrefixMagTable.TryGetValue(unitBuilder[0], out magnitude);

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

                if (property.Id.StartsWith("Rot")) // Increment exception for projectors
                    increment = 90f;
                else
                {
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