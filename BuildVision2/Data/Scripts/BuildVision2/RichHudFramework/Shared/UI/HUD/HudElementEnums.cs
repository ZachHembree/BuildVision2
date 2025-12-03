using System;
using VRage;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Automatic size matching flags for <see cref="HudElementBase.DimAlignment"/>
        /// </summary>
        [Flags]
        public enum DimAlignments : byte
        {
            /// <summary>
            /// No size matching
            /// </summary>
            None = 0x0,

            /// <summary>
            /// Match parent width
            /// </summary>
            Width = 0x1,

            /// <summary>
            /// Match parent height
            /// </summary>
            Height = 0x2,

            /// <summary>
            /// Match parent size
            /// </summary>
            Both = Width | Height,

            /// <summary>
            /// Match parent size
            /// </summary>
            Size = Width | Height,

            /// <summary>
            /// Matches parent size less padding
            /// </summary>
            IgnorePadding = 0x4,

            /// <summary>
            /// Match parent width less padding
            /// </summary>
            UnpaddedWidth = Width | IgnorePadding,

            /// <summary>
            /// Match parent height less padding
            /// </summary>
            UnpaddedHeight = Height | IgnorePadding,

            /// <summary>
            /// Match parent size less padding
            /// </summary>
            UnpaddedSize = Size | IgnorePadding
        }

        /// <summary>
        /// Used to determine the default position of an element relative to its parent via 
        /// <see cref="HudElementBase.ParentAlignment"/>.
        /// </summary>
        [Flags]
        public enum ParentAlignments : byte
        {
            /// <summary>
            /// The element's origin is at the center of its parent.
            /// </summary>
            Center = 0x0,

            /// <summary>
            /// The element will start with its right edge aligned to its parent's left edge.
            /// If the flag InnerH is set, then its left edge will be aligned to its parent's
            /// left edge.
            /// </summary>
            Left = 0x1,

			/// <summary>
			/// The element will start with its top edge aligned to its parent's bottom edge.
			/// If the flag InnerV is set, then its bottom edge will be aligned to its parent's
			/// bottom edge.
			/// </summary>
			Bottom = 0x2,

			/// <summary>
			/// The element will start with its left edge aligned to its parent's right edge.
			/// If the flag InnerH is set, then its right edge will be aligned to its parent's
			/// right edge.
			/// </summary>
			Right = 0x4,

			/// <summary>
			/// The element will start with its bottom edge aligned to its parent's top edge.
			/// If the flag InnerV is set, then its top edge will be aligned to its parent's
			/// top edge.
			/// </summary>
			Top = 0x8,

			/// <summary>
			/// The element will start with its parent's top left corner aligned to its bottom right
			/// corner.
			/// </summary>
			TopLeft = Top | Left,

            /// <summary>
            /// The element will start with its parent's top right corner aligned to its bottom left
            /// corner.
            /// </summary>
            TopRight = Top | Right,

            /// <summary>
            /// The element will start with its parent's bottom left corner aligned to its top right
            /// corner.
            /// </summary>
            BottomLeft = Bottom | Left,

            /// <summary>
            /// The element will start with its parent's bottom right corner aligned to its top left
            /// corner.
            /// </summary>
            BottomRight = Bottom | Right,

            /// <summary>
            /// Modifier flag to be used in conjunction with the Left/Right flags. If this flag is set,
            /// then the element will be horizontally aligned to the interior of its parent.
            /// </summary>
            InnerH = 0x10,

            /// <summary>
            /// Modifier flag to be used in conjunction with the Top/Bottom flags. If this flag is set,
            /// then the element will be vertically aligned to the interior of its parent.
            /// </summary>
            InnerV = 0x20,

            /// <summary>
            /// If set, this flag will cause the element's alignment to be calculated taking the
            /// parent's padding into account.
            /// </summary>
            UsePadding = 0x40,

            /// <summary>
            /// InnerH + InnerV. If this flag is set then the element will be aligned to the interior of
            /// its parent.
            /// </summary>
            Inner = InnerH | InnerV,

            /// <summary>
            /// The element will start with its left edge aligned to its parent's left edge.
            /// </summary>
            InnerLeft = InnerH | Left,

            /// <summary>
            /// The element will start with its top edge aligned to its parent's top edge.
            /// </summary>
            InnerTop = InnerV | Top,

            /// <summary>
            /// The element will start with its right edge aligned to its parent's right edge.
            /// </summary>
            InnerRight = InnerH | Right,

            /// <summary>
            /// The element will start with its parent's top left corner aligned to its top left
            /// corner.
            /// </summary>
            InnerTopLeft = Inner | Top | Left,

            /// <summary>
            /// The element will start with its parent's top right corner aligned to its top right
            /// corner.
            /// </summary>
            InnerTopRight = Inner | Top | Right,

            /// <summary>
            /// The element will start with its parent's bottom left corner aligned to its bottom left
            /// corner.
            /// </summary>
            InnerBottomLeft = Inner | Bottom | Left,

            /// <summary>
            /// The element will start with its parent's bottom right corner aligned to its bottom right
            /// corner.
            /// </summary>
            InnerBottomRight = Inner | Bottom | Right,

            /// <summary>
            /// The element will start with its bottom edge aligned to its parent's bottom edge.
            /// </summary>
            InnerBottom = InnerV | Bottom,

            /// <summary>
            /// The element will start with its left edge aligned to its parent's left edge, while
            /// respecting padding.
            /// </summary>
            PaddedInnerLeft = UsePadding | InnerH | Left,

            /// <summary>
            /// The element will start with its top edge aligned to its parent's top edge, while
            /// respecting padding.
            /// </summary>
            PaddedInnerTop = UsePadding | InnerV | Top,

            /// <summary>
            /// The element will start with its right edge aligned to its parent's right edge, while
            /// respecting padding.
            /// </summary>
            PaddedInnerRight = UsePadding | InnerH | Right,

            /// <summary>
            /// The element will start with its bottom edge aligned to its parent's bottom edge, while
            /// respecting padding.
            /// </summary>
            PaddedInnerBottom = UsePadding | InnerV | Bottom,

            /// <summary>
            /// The element will start with its parent's top left corner aligned to its top left
            /// corner, while respecting padding.
            /// </summary>
            PaddedInnerTopLeft = UsePadding | Inner | Top | Left,

            /// <summary>
            /// The element will start with its parent's top right corner aligned to its top right
            /// corner, while respecting padding.
            /// </summary>
            PaddedInnerTopRight = UsePadding | Inner | Top | Right,

            /// <summary>
            /// The element will start with its parent's bottom left corner aligned to its bottom left
            /// corner, while respecting padding.
            /// </summary>
            PaddedInnerBottomLeft = UsePadding | Inner | Bottom | Left,

            /// <summary>
            /// The element will start with its parent's bottom right corner aligned to its bottom right
            /// corner, while respecting padding.
            /// </summary>
            PaddedInnerBottomRight = UsePadding | Inner | Bottom | Right
        }
    }
}