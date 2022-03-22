using RichHudFramework;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI.Ingame;
using VRageMath;
using IMyCubeBlock = VRage.Game.ModAPI.Ingame.IMyCubeBlock;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        /// <summary>
        /// Block Terminal Property for <see cref="VRageMath.Color"/>
        /// </summary>
        private class ColorProperty : NumericPropertyBase<Color>, IBlockColor
        {
            /// <summary>
            /// Minimum allowable value
            /// </summary>
            public Color MinValue { get; }

            /// <summary>
            /// Maximum allowable value
            /// </summary>
            public Color MaxValue { get; }

            /// <summary>
            /// Standard increment
            /// </summary>
            public Color Increment { get; }

            /// <summary>
            /// Associated color channels as byte-value block members
            /// </summary>
            public IReadOnlyList<IBlockNumericValue<byte>> ColorChannels { get; }

            /// <summary>
            /// Retrieves the value as a <see cref="StringBuilder"/> using formatting specific to the member.
            /// </summary>
            public override StringBuilder FormattedValue
            {
                get
                {
                    dispBuilder.Clear();
                    Color value = Value;
                    dispBuilder.Append(value.R);
                    dispBuilder.Append(", ");
                    dispBuilder.Append(value.G);
                    dispBuilder.Append(", ");
                    dispBuilder.Append(value.B);

                    return dispBuilder;
                }
            }

            /// <summary>
            /// Additional information following the value of the member.
            /// </summary>
            public override StringBuilder StatusText => null;

            private BvPropPool<ColorProperty> poolParent;
            private readonly StringBuilder dispBuilder;
            private readonly HsvToRgbPropWrapper hsvWrapper;

            public ColorProperty()
            {
                MinValue = Color.Black;
                MaxValue = Color.White;
                Increment = new Color(1, 1, 1);
                Flags = BlockPropertyFlags.IsIntegral | BlockPropertyFlags.CanUseMultipliers;
                ValueType = BlockMemberValueTypes.Color;
                dispBuilder = new StringBuilder();
                ColorChannels = new IBlockNumericValue<byte>[3]
                {
                    new ColorPropertyChannel(this, "R", 0),
                    new ColorPropertyChannel(this, "G", 1),
                    new ColorPropertyChannel(this, "B", 2),
                };

                hsvWrapper = new HsvToRgbPropWrapper();
            }

            public override void SetProperty(StringBuilder name, ITerminalProperty<Color> property, PropertyBlock block)
            {
                base.SetProperty(name, property, block);

                if (poolParent == null)
                    poolParent = block.colorPropPool;
            }

            public void SetProperty(StringBuilder name, ITerminalProperty<Vector3> property, PropertyBlock block)
            {
                hsvWrapper.HsvProp = property;
                SetProperty(name, hsvWrapper, block);
            }

            public override void Reset()
            {
                hsvWrapper.HsvProp = null;
                base.Reset();
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

            public static ColorProperty GetProperty(StringBuilder name, ITerminalProperty<Vector3> property, PropertyBlock block)
            {
                ColorProperty prop = block.colorPropPool.Get();
                prop.SetProperty(name, property, block);

                return prop;
            }

            /// <summary>
            /// Parses color channel data and applies it to the corresponding color channel in the output value.
            /// </summary>
            public override bool TryParseValue(string valueData, out Color value)
            {
                return Utils.Color.TryParseColor(valueData, out value, true);
            }
        }

        /// <summary>
        /// Compatibility layer for HSV colors
        /// </summary>
        protected class HsvToRgbPropWrapper : ITerminalProperty<Color>
        {
            public string Id => HsvProp.Id;

            public string TypeName => "Color";

            public ITerminalProperty<Vector3> HsvProp { get; set; }

            public Color GetDefaultValue(IMyCubeBlock block)
            {
                return Color.Black;
            }

            public Color GetMaximum(IMyCubeBlock block)
            {
                return Color.White;
            }

            public Color GetMinimum(IMyCubeBlock block)
            {
                return Color.Black;
            }

            public Color GetMininum(IMyCubeBlock block)
            {
                return Color.Black;
            }

            public Color GetValue(IMyCubeBlock block)
            {
                return HsvProp.GetValue(block).HSVtoColor();
            }

            public void SetValue(IMyCubeBlock block, Color value)
            {
                HsvProp.SetValue(block, value.ColorToHSV());
            }
        }
    }
}