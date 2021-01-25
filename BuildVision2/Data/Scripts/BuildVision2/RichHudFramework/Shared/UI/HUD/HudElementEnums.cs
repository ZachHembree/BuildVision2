using System;
using VRage;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        [Flags]
        public enum DimAlignments : int
        {
            None = 0,

            /// <summary>
            /// Match parent width
            /// </summary>
            Width = 1,

            /// <summary>
            /// Match parent height
            /// </summary>
            Height = 2,

            /// <summary>
            /// Match parent size
            /// </summary>
            Both = 3,

            /// <summary>
            /// Matches parent size less padding
            /// </summary>
            IgnorePadding = 4,
        }

        /// <summary>
        /// Used to determine the default position of an element relative to its parent.
        /// </summary>
        [Flags]
        public enum ParentAlignments : int
        {
            /// <summary>
            /// The element's origin is at the center of its parent.
            /// </summary>
            Center = 0,

            /// <summary>
            /// The element will start with its right edge aligned to its parent's left edge.
            /// If the flag InnerH is set, then its left edge will be aligned to its parent's
            /// left edge.
            /// </summary>
            Left = 1,

            /// <summary>
            /// The element will start with its bottom edge aligned to its parent's top edge.
            /// If the flag InnerV is set, then its top edge will be aligned to its parent's
            /// top edge.
            /// </summary>
            Top = 2,

            /// <summary>
            /// The element will start with its left edge aligned to its parent's right edge.
            /// If the flag InnerH is set, then its right edge will be aligned to its parent's
            /// right edge.
            /// </summary>
            Right = 4,

            /// <summary>
            /// The element will start with its top edge aligned to its parent's bottom edge.
            /// If the flag InnerV is set, then its bottom edge will be aligned to its parent's
            /// bottom edge.
            /// </summary>
            Bottom = 8,

            /// <summary>
            /// Modifier flag to be used in conjunction with the Left/Right flags. If this flag is set,
            /// then the element will be horizontally aligned to the interior of its parent.
            /// </summary>
            InnerH = 16,

            /// <summary>
            /// Modifier flag to be used in conjunction with the Top/Bottom flags. If this flag is set,
            /// then the element will be vertically aligned to the interior of its parent.
            /// </summary>
            InnerV = 32,

            /// <summary>
            /// InnerH + InnerV. If this flag is set then the element will be aligned to the interior of
            /// its parent.
            /// </summary>
            Inner = 48,

            /// <summary>
            /// If set, this flag will cause the element's alignment to be calculated taking the
            /// parent's padding into account.
            /// </summary>
            UsePadding = 64,
        }

        /// <summary>
        /// Used to determine text alignment.
        /// </summary>
        public enum TextAlignment : byte
        {
            Left = 0,
            Center = 1,
            Right = 2,
        }
    }
}