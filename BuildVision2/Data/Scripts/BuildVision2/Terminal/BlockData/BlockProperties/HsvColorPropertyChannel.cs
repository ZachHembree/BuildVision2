using RichHudFramework;
using System;
using System.Text;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        /// <summary>
        /// Float color channel for <see cref="HsvColorProperty"/>
        /// </summary>
        private class HsvColorPropertyChannel : IBlockNumericValue<float>
        {
            /// <summary>
            /// Gets/sets the member's value
            /// </summary>
            public float Value
            {
                get { return parent.Value.GetDim(channel); }
                set
                {
                    Vector3 fullColor = parent.Value;
                    fullColor.SetDim(channel, value);
                    parent.Value = fullColor;
                }
            }

            /// <summary>
            /// Minimum allowable value
            /// </summary>
            public float MinValue => parent.MinValue.GetDim(channel);

            /// <summary>
            /// Maximum allowable value
            /// </summary>
            public float MaxValue => parent.MaxValue.GetDim(channel);

            /// <summary>
            /// Standard increment
            /// </summary>
            public float Increment => parent.Increment.GetDim(channel);

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
                    valueSB.AppendFormat("{0:G6}", Math.Round(Value, 1));

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

            private readonly HsvColorProperty parent;
            private readonly string suffix;
            private readonly int channel;
            private readonly StringBuilder nameSB, valueSB;

            public HsvColorPropertyChannel(HsvColorProperty parent, string suffix, int channel)
            {
                nameSB = new StringBuilder();
                valueSB = new StringBuilder();
                this.parent = parent;
                this.suffix = suffix;
                this.channel = channel;

                Flags = BlockPropertyFlags.CanUseMultipliers;
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
                float value;

                if (float.TryParse(text, out value))
                {
                    Value = value;
                }
            }
        }
    }
}