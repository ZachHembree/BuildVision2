using RichHudFramework;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    internal partial class PropertyBlock
    {
        /// <summary>
        /// Block Terminal Property for individual color channels of a VRageMath.Color
        /// </summary>
        private class ColorProperty : NumericPropertyBase<Color>
        {
            public override string PropName => $"{property.Id}_{channel}";
            public override string Value => GetValue().GetChannel(channel).ToString();
            public override string Postfix => null;

            private readonly int channel;
            private static int incrX, incrY, incrZ, incr0;

            public ColorProperty(string name, ITerminalProperty<Color> property, IMyTerminalControl control, IMyTerminalBlock block, int channel)
                : base(name, property, control, block)
            {
                incr0 = 1;
                incrZ = (incr0 * Cfg.colorMult.Z); // x64
                incrY = (incr0 * Cfg.colorMult.Y); // x16
                incrX = (incr0 * Cfg.colorMult.X); // x8

                this.channel = channel;
            }

            /// <summary>
            /// Returns a scrollable property for each color channel in an ITerminalProperty<Color> object
            /// </summary>
            public static ColorProperty[] GetColorProperties(string name, ITerminalProperty<Color> property, IMyTerminalControl control, IMyTerminalBlock block)
            {
                return new ColorProperty[]
                {
                    new ColorProperty($"{name}: R", property, control, block, 0),
                    new ColorProperty($"{name}: G", property, control, block, 1),
                    new ColorProperty($"{name}: B", property, control, block, 2)
                };
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
                byte channelValue;
                value = GetValue();

                if (byte.TryParse(valueData, out channelValue))
                {
                    value = value.SetChannel(channel, channelValue);
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