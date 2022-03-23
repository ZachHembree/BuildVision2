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
        /// Block Terminal Property for <see cref="VRageMath.Vector3"/>
        /// </summary>
        private class HsvColorProperty : NumericPropertyBase<Vector3>, IBlockColorHSV
        {
            /// <summary>
            /// Get/sets the value associated with the property
            /// </summary>
            public override Vector3 Value
            {
                set
                {
                    if (_value.X != value.X || _value.Y != value.Y || _value.Z != value.Z)
                        valueChanged = true;

                    _value = Vector3.Clamp(Vector3.Round(value, 2), MinValue, MaxValue);
                }
            }

            public IReadOnlyList<IBlockNumericValue<float>> ColorChannels { get; }

            public Vector3 MinValue { get; }

            public Vector3 MaxValue { get; }

            public Vector3 Increment { get; }

            /// <summary>
            /// Retrieves the value as a <see cref="StringBuilder"/> using formatting specific to the member.
            /// </summary>
            public override StringBuilder FormattedValue
            {
                get
                {
                    dispBuilder.Clear();
                    Vector3 value = Value;
                    dispBuilder.Append(value.X.Round(1));
                    dispBuilder.Append(", ");
                    dispBuilder.Append(value.Y.Round(1));
                    dispBuilder.Append(", ");
                    dispBuilder.Append(value.Z.Round(1));

                    return dispBuilder;
                }
            }

            /// <summary>
            /// Additional information following the value of the member.
            /// </summary>
            public override StringBuilder StatusText => null;

            private BvPropPool<HsvColorProperty> poolParent;
            private readonly StringBuilder dispBuilder;

            public HsvColorProperty()
            {
                MinValue = Vector3.Zero;
                MaxValue = new Vector3(360f, 100f, 100f);
                Increment = Vector3.One;

                Flags = BlockPropertyFlags.CanUseMultipliers;
                ValueType = BlockMemberValueTypes.ColorHSV;
                dispBuilder = new StringBuilder();
                ColorChannels = new IBlockNumericValue<float>[3]
                {
                    new HsvColorPropertyChannel(this, "H", 0),
                    new HsvColorPropertyChannel(this, "S", 1),
                    new HsvColorPropertyChannel(this, "V", 2),
                };
            }

            public override void SetProperty(StringBuilder name, ITerminalProperty<Vector3> property, PropertyBlock block)
            {
                base.SetProperty(name, property, block);

                if (poolParent == null)
                    poolParent = block.hsvPropPool;
            }

            public override void Return()
            {
                poolParent.Return(this);
            }

            public static HsvColorProperty GetProperty(StringBuilder name, ITerminalProperty<Vector3> property, PropertyBlock block)
            {
                HsvColorProperty prop = block.hsvPropPool.Get();
                prop.SetProperty(name, property, block);

                return prop;
            }

            public override bool TryParseValue(string valueData, out Vector3 value)
            {
                value = -Vector3.One;
                return false;
            }
        }
    }
}
