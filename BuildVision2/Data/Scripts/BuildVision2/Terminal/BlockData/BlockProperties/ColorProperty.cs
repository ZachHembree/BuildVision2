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
            public Color Value { get { return GetValue(); } set { SetValue(value); } }

            public Color MinValue => Color.Black;

            public Color MaxValue => Color.White;

            public Color Increment => new Color(1, 1, 1);

            public IReadOnlyList<IBlockNumericValue<byte>> ColorChannels { get; }

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

            private BvPropPool<ColorProperty> poolParent;

            private readonly StringBuilder dispBuilder;

            public ColorProperty()
            {
                dispBuilder = new StringBuilder();
                ValueType = BlockMemberValueTypes.Color;

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
                    SetValue(value);
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
        }
    }
}