using RichHudFramework;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System.Text;
using VRageMath;

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
            }

            public override void SetProperty(StringBuilder name, ITerminalProperty<Color> property, PropertyBlock block)
            {
                base.SetProperty(name, property, block);

                if (poolParent == null)
                    poolParent = block.colorPropPool;
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

            public override bool TryImportData(PropertyData data)
            {
                Color value;

                if (Utils.ProtoBuf.TryDeserialize(data.valueData, out value) == null)
                {
                    Value = value;
                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Retrieves the property data for the color channel associated with the control.
            /// </summary>
            public override PropertyData? GetPropertyData()
            {
                byte[] valueData;

                if (Utils.ProtoBuf.TrySerialize(Value, out valueData) == null)
                {
                    return new PropertyData(PropName, valueData, Enabled, ValueType);
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
        }
    }
}