using RichHudFramework;
using System;
using System.Text;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        /// <summary>
        /// Byte color channel for <see cref="ColorProperty"/>
        /// </summary>
        private class ColorPropertyChannel : IBlockNumericValue<byte>
        {
            public byte MinValue => 0;

            public byte MaxValue => 255;

            public byte Increment => 1;

            public byte Value
            {
                get { return parent.Value.GetChannel(channel); }
                set
                {
                    Color fullColor = parent.Value;
                    parent.Value = fullColor.SetChannel(channel, value);
                }
            }

            public bool CanUseMultipliers { get; }

            public bool IsInteger { get; }

            public Func<char, bool> CharFilterFunc => parent.CharFilterFunc;

            public string PropName
            {
                get
                {
                    nameSB.Clear();
                    nameSB.Append(parent.PropName);
                    nameSB.Append('_');
                    nameSB.Append(suffix);

                    return nameSB.ToString();
                }
            }

            public StringBuilder Name
            {
                get
                {
                    nameSB.Clear();
                    nameSB.Append(parent.Name);
                    nameSB.Append(": ");
                    nameSB.Append(suffix);

                    return nameSB;
                }
            }

            public StringBuilder FormattedValue => ValueText;

            public StringBuilder ValueText
            {
                get
                {
                    valueSB.Clear();
                    valueSB.Append(Value);
                    return valueSB;
                }
            }

            public StringBuilder StatusText => null;

            public bool Enabled => parent.Enabled;

            public BlockMemberValueTypes ValueType => BlockMemberValueTypes.ColorChannel;

            private readonly ColorProperty parent;
            private readonly string suffix;
            private readonly int channel;
            private readonly StringBuilder nameSB, valueSB;

            public ColorPropertyChannel(ColorProperty parent, string suffix, int channel)
            {
                nameSB = new StringBuilder();
                valueSB = new StringBuilder();
                this.parent = parent;
                this.suffix = suffix;
                this.channel = channel;
                CanUseMultipliers = true;
                IsInteger = false;
            }

            public PropertyData? GetPropertyData()
            {
                byte[] valueData;

                if (Utils.ProtoBuf.TrySerialize(Value, out valueData) == null)
                {
                    return new PropertyData(PropName.ToString(), valueData, Enabled, ValueType);
                }
                else
                    return default(PropertyData);
            }

            public void SetValueText(string text)
            {
                byte value;

                if (byte.TryParse(text, out value))
                {
                    Value = value;
                }
            }
        }
    }
}