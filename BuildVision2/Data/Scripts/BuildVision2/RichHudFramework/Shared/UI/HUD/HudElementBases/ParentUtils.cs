using System;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;
        using Client;

        public abstract partial class HudParentBase
        {
            /// <summary>
            /// Utilities used internally to access parent node members
            /// </summary>
            protected static class ParentUtils
            {
                /// <summary>
                /// Calculates the full z-offset using the public offset and inner offset.
                /// </summary>
                public static ushort GetFullZOffset(HudParentBase element, HudParentBase parent = null)
                {
                    byte outerOffset = (byte)(element._zOffset - sbyte.MinValue);
                    ushort innerOffset = (ushort)(element.zOffsetInner << 8);

                    if (parent != null)
                    {
                        outerOffset += (byte)((parent.fullZOffset & 0x00FF) + sbyte.MinValue);
                        innerOffset += (ushort)(parent.fullZOffset & 0xFF00);
                    }

                    return (ushort)(innerOffset | outerOffset);
                }

                /// <summary>
                /// Returns the visibility set for the given <see cref="HudParentBase"/> without including
                /// parent visibility.
                /// </summary>
                public static bool IsSetVisible(HudParentBase node)
                {
                    return node._visible && node._registered;
                }
            }
        }
    }
}