using RichHudFramework;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRageMath;
using System.Text;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        /// <summary>
        /// Block Terminal Property for individual color channels of a VRageMath.Color
        /// </summary>
        private class ColorProperty : NumericPropertyBase<Color>, IBlockValue<Color>
        {
            public Color Value { get { return GetValue(); } set { SetValue(value); } }

            public override StringBuilder FormattedValue 
            {
                get 
                {
                    dispBuilder.Clear();
                    Color value = GetValue();
                    dispBuilder.Append(value.R);
                    dispBuilder.Append(", ");
                    dispBuilder.Append(value.G);
                    dispBuilder.Append(", ");
                    dispBuilder.Append(value.B);

                    return dispBuilder;
                }
            }

            public override StringBuilder StatusText => null;

            private static int incrX, incrY, incrZ, incr0;
            private BvPropPool<ColorProperty> poolParent;
            protected readonly StringBuilder dispBuilder;

            public ColorProperty()
            {
                dispBuilder = new StringBuilder();
                ValueType = BlockMemberValueTypes.Color;
            }

            public override void SetProperty(StringBuilder name, ITerminalProperty<Color> property, PropertyBlock block)
            {
                base.SetProperty(name, property, block);

                if (poolParent == null)
                    poolParent = block.colorPropPool;

                incr0 = 1;
                incrZ = (incr0 * Cfg.colorMult.Z); // x64
                incrY = (incr0 * Cfg.colorMult.Y); // x16
                incrX = (incr0 * Cfg.colorMult.X); // x8
            }

            public override void Return()
            {
                poolParent.Return(this);
            }

            public static ColorProperty GetProperty(StringBuilder name, ITerminalProperty<Color> property, PropertyBlock block)
            {
                ColorProperty prop = block.colorPropPool.Get();
                prop.SetProperty(name, property, block);

                return prop;
            }

            public override void ScrollDown() =>
                SetPropValue(false);

            public override void ScrollUp() =>
                SetPropValue(true);

            public override bool TryImportData(PropertyData data)
            {
                Color value;

                if (Utils.ProtoBuf.TryDeserialize(data.valueData, out value) == null)
                {
                    SetValue(value);
                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Retrieves the property data for the color channel associated with the control.
            /// </summary>
            public override PropertyData GetPropertyData()
            {
                byte[] valueData;

                if (Utils.ProtoBuf.TrySerialize(GetValue(), out valueData) == null)
                {
                    return new PropertyData(PropName.ToString(), valueData);
                }
                else
                    return default(PropertyData);
            }

            /// <summary>
            /// Parses color channel data and applies it to the corresponding color channel in the output value.
            /// </summary>
            public override bool TryParseValue(string valueData, out Color value)
            {
                return Utils.Color.TryParseColor(valueData, out value, true);
            }

            /// <summary>
            /// Changes property color value based on given color delta.
            /// </summary>
            private void SetPropValue(bool increment, int channel)
            {
                Color current = GetValue();
                int value = current.GetChannel(channel),
                    mult = increment ? GetIncrement() : -GetIncrement();

                current = current.SetChannel(channel, (byte)MathHelper.Clamp(value + mult, 0, 255));
                SetValue(current);
            }

            /// <summary>
            /// Changes property color value based on given color delta.
            /// </summary>
            private void SetPropValue(bool increment)
            {
                Color current = GetValue();
                int mult = increment ? GetIncrement() : -GetIncrement();

                current.R = (byte)MathHelper.Clamp(current.R + mult, 0, 255);
                current.G = (byte)MathHelper.Clamp(current.G + mult, 0, 255);
                current.B = (byte)MathHelper.Clamp(current.B + mult, 0, 255);
                SetValue(current);
            }

            /// <summary>
            /// Gets value to add or subtract from the property based on multipliers used.
            /// </summary>
            private int GetIncrement()
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