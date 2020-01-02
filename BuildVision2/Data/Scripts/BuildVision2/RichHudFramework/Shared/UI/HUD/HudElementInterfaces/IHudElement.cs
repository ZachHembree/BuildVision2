using System;
using VRage;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Interface for all hud elements with definite size and position.
        /// </summary>
        public interface IHudElement : IHudNode
        {
            /// <summary>
            /// Scales the size and offset of an element. Any offset or size set at a given
            /// be increased or decreased with scale. Defaults to 1f. Includes parent scale.
            /// </summary>
            float Scale { get; set; }

            /// <summary>
            /// Size of the hud element in pixels.
            /// </summary>
            Vector2 Size { get; set; }

            /// <summary>
            /// Height of the hud element in pixels.
            /// </summary>
            float Height { get; set; }

            /// <summary>
            /// Width of the hud element in pixels.
            /// </summary>
            float Width { get; set; }

            /// <summary>
            /// Starting position of the hud element on the screen in pixels.
            /// </summary>
            Vector2 Origin { get; }

            /// <summary>
            /// Position of the hud element relative to its origin.
            /// </summary>
            Vector2 Offset { get; set; }

            /// <summary>
            /// Determines the starting position of the hud element relative to its parent.
            /// </summary>
            ParentAlignments ParentAlignment { get; set; }

            DimAlignments DimAlignment { get; set; }

            /// <summary>
            /// If set to true the hud element will be allowed to capture the cursor.
            /// </summary>
            bool CaptureCursor { get; set; }

            /// <summary>
            /// If set to true the hud element will share the cursor with its child elements.
            /// </summary>
            bool ShareCursor { get; set; }

            /// <summary>
            /// Indicates whether or not the cursor is currently over the element. The element must
            /// be set to capture the cursor for this to work.
            /// </summary>
            bool IsMousedOver { get; }
        }
    }
}