using RichHudFramework;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        /// <summary>
        /// Block Terminal Property for individual color channels of a VRageMath.Color
        /// </summary>
        private class ColorProperty : NumericPropertyBase<Color>
        {
            public override string PropName => $"{property.Id}_{channel}";
            public override string Display => GetValue().GetChannel(channel).ToString();
            public override string Status => null;

            private int channel;
            private static int incrX, incrY, incrZ, incr0;
            protected BvPropPool<ColorProperty> poolParent;

            public void SetProperty(string name, ITerminalProperty<Color> property, IMyTerminalControl control, PropertyBlock block, int channel)
            {
                base.SetProperty(name, property, control, block);

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
            public static void AddColorProperties(string name, ITerminalProperty<Color> property, IMyTerminalControl control, PropertyBlock block)
            {
                ColorProperty r = block.colorPropPool.Get(), 
                    g = block.colorPropPool.Get(),
                    b = block.colorPropPool.Get();

                r.SetProperty($"{name}: R", property, control, block, 0);
                g.SetProperty($"{name}: G", property, control, block, 1);
                b.SetProperty($"{name}: B", property, control, block, 2);

                block.blockProperties.Add(r);
                block.blockProperties.Add(g);
                block.blockProperties.Add(b);
            }

            public override void ScrollDown() =>
                SetPropValue(false);

            public override void ScrollUp() =>
                SetPropValue(true);

            /// <summary>
            /// Retrieves the property data for the color channel associated with the control.
            /// </summary>
            public override PropertyData GetPropertyData() =>
                new PropertyData(PropName, ID, GetValue().GetChannel(channel).ToString());

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