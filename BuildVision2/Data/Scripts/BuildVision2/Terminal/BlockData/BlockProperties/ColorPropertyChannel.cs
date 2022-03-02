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
            /// <summary>
            /// Gets/sets the member's value
            /// </summary>
            public byte Value
            {
                get { return parent.Value.GetChannel(channel); }
                set
                {
                    Color fullColor = parent.Value;
                    parent.Value = fullColor.SetChannel(channel, value);
                }
            }

            /// <summary>
            /// Minimum allowable value
            /// </summary>
            public byte MinValue { get; }

            /// <summary>
            /// Maximum allowable value
            /// </summary>
            public byte MaxValue { get; }

            /// <summary>
            /// Standard increment
            /// </summary>
            public byte Increment { get; }

            /// <summary>
            /// Returns flags associated with the property
            /// </summary>
            public virtual BlockPropertyFlags Flags { get; protected set; }

            /// <summary>
            /// Delegate used for filtering text input. Returns true if a given character is in the accepted range.
            /// </summary>
            public Func<char, bool> CharFilterFunc => parent.CharFilterFunc;

            /// <summary>
            /// Unique identifier associated with the property
            /// </summary>
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

            /// <summary>
            /// Retrieves the name of the block property
            /// </summary>
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

            /// <summary>
            /// Retrieves the value as a <see cref="StringBuilder"/> using formatting specific to the member.
            /// </summary>
            public StringBuilder FormattedValue => ValueText;

            /// <summary>
            /// Retrieves the current value of the block member as an unformatted <see cref="StringBuilder"/>
            /// </summary>
            public StringBuilder ValueText
            {
                get
                {
                    valueSB.Clear();
                    valueSB.Append(Value);
                    return valueSB;
                }
            }

            /// <summary>
            /// Additional information following the value of the member.
            /// </summary>
            public StringBuilder StatusText => null;

            /// <summary>
            /// Indicates whether or not a given <see cref="IBlockMember"/> should be shown in the terminal.
            /// </summary>
            public bool Enabled => parent.Enabled;

            /// <summary>
            /// Returns the type of data stored by this member, if any.
            /// </summary>
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

                Flags = BlockPropertyFlags.IsIntegral | BlockPropertyFlags.CanUseMultipliers;
                MinValue = 0;
                MaxValue = 1;
                Increment = 1;
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