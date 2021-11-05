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
        private class ColorProperty : NumericPropertyBase<Color>
        {
            public override StringBuilder Display 
            {
                get 
                {
                    dispBuilder.Clear();
                    return dispBuilder.Append(GetValue().GetChannel(channel));
                }
            }

            public override StringBuilder Status => null;

            private int channel;
            private static int incrX, incrY, incrZ, incr0;
            private BvPropPool<ColorProperty> poolParent;
            protected readonly StringBuilder dispBuilder;

            public ColorProperty()
            {
                dispBuilder = new StringBuilder();
            }

            public void SetProperty(StringBuilder name, string suffix, ITerminalProperty<Color> property, PropertyBlock block, int channel)
            {
                base.SetProperty(name, property, block);
                Name.Append(suffix);

                PropName.Append('_');
                PropName.Append(channel);

                if (poolParent == null)
                    poolParent = block.colorPropPool;

                incr0 = 1;
                incrZ = (incr0 * Cfg.colorMult.Z); // x64
                incrY = (incr0 * Cfg.colorMult.Y); // x16
                incrX = (incr0 * Cfg.colorMult.X); // x8

                this.channel = channel;
            }

            public override void Return()
            {
                poolParent.Return(this);
            }

            /// <summary>
            /// Returns a scrollable property for each color channel in an ITerminalProperty<Color> object
            /// </summary>
            public static void AddColorProperties(StringBuilder name, ITerminalProperty<Color> property, PropertyBlock block)
            {
                ColorProperty r = block.colorPropPool.Get(), 
                    g = block.colorPropPool.Get(),
                    b = block.colorPropPool.Get();

                r.SetProperty(name, $": R", property, block, 0);
                g.SetProperty(name, $": G", property, block, 1);
                b.SetProperty(name, $": B", property, block, 2);

                block.blockProperties.Add(r);
                block.blockProperties.Add(g);
                block.blockProperties.Add(b);
            }

            public override void ScrollDown() =>
                SetPropValue(false);

            public override void ScrollUp() =>
                SetPropValue(true);

            public override bool TryImportData(PropertyData data)
            {
                byte value;

                if (Utils.ProtoBuf.TryDeserialize(data.valueData, out value) == null)
                {
                    SetValue(GetValue().SetChannel(channel, value));
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

                if (Utils.ProtoBuf.TrySerialize(GetValue().GetChannel(channel), out valueData) == null)
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
                int result;
                value = GetValue();

                if (int.TryParse(valueData, out result))
                {
                    byte newValue = (byte)MathHelper.Clamp(result, 0, 255);

                    value = value.SetChannel(channel, newValue);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// Changes property color value based on given color delta.
            /// </summary>
            private void SetPropValue(bool increment)
            {
                Color current = GetValue();
                int value = current.GetChannel(channel),
                    mult = increment ? GetIncrement() : -GetIncrement();

                current = current.SetChannel(channel, (byte)MathHelper.Clamp(value + mult, 0, 255));
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