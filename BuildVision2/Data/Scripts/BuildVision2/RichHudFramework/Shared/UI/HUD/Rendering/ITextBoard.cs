using System;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        namespace Rendering
        {
            /// <summary>
            /// Internal API accessor indices for TextBoard configuration
            /// </summary>
            /// <exclude/>
            public enum TextBoardAccessors : int
            {
                /// <summary>
                /// in/out: bool
                /// </summary>
                AutoResize = 129,

                /// <summary>
                /// in/out: bool
                /// </summary>
                VertAlign = 130,

                /// <summary>
                /// in: Vector2I
                /// </summary>
                MoveToChar = 131,

                /// <summary>
                /// out: Vector2I
                /// </summary>
                GetCharAtOffset = 132,

                /// <summary>
                /// Action event
                /// </summary>
                OnTextChanged = 133,

                /// <summary>
                /// in/out: Vector2
                /// </summary>
                TextOffset = 134,

                /// <summary>
                /// out: Vector2I
                /// </summary>
                VisibleLineRange = 135,
            }

            /// <summary>
            /// Represents a renderable text element that supports rich text formatting, scrolling, and advanced layout.
            /// <para>
            /// This interface combines text manipulation (via <see cref="ITextBuilder"/>) with rendering logic. 
            /// It handles text clipping, auto-resizing, text alignment, and coordinate-based character lookups.
            /// </para>
            /// </summary>
            public interface ITextBoard : ITextBuilder
            {
                /// <summary>
                /// Invoked whenever a change is made to the text. 
                /// This event is rate-limited and invokes once every 500ms at most.
                /// </summary>
                event Action TextChanged;

                /// <summary>
                /// The base visual scale of the text board. 
                /// This is applied multiplicatively after the scaling specified in <see cref="GlyphFormat"/>.
                /// </summary>
                float Scale { get; set; }

                /// <summary>
                /// The current boundaries of the text box as rendered. 
                /// If <see cref="AutoResize"/> is true, this matches <see cref="TextSize"/>.
                /// If false, this returns <see cref="FixedSize"/>.
                /// </summary>
                Vector2 Size { get; }

                /// <summary>
                /// The total dimensions of the text content, including text currently outside the visible range. 
                /// This value updates immediately upon modification.
                /// </summary>
                Vector2 TextSize { get; }

                /// <summary>
                /// The render offset of the text content (scrolling/panning). 
                /// <para>Note: <see cref="AutoResize"/> must be disabled for this to take effect.</para>
                /// <para>The value is automatically clamped to ensure the text remains within visible bounds.</para>
                /// </summary>
                Vector2 TextOffset { get; set; }

                /// <summary>
                /// Returns the index range of the lines currently visible within the text board's bounds.
                /// X = Start Line Index, Y = End Line Index.
                /// </summary>
                Vector2I VisibleLineRange { get; }

                /// <summary>
                /// The fixed dimensions of the text box used when <see cref="AutoResize"/> is false. 
                /// This property is ignored if AutoResize is enabled.
                /// </summary>
                Vector2 FixedSize { get; set; }

                /// <summary>
                /// If true, the text board's <see cref="Size"/> will automatically expand or contract to fit the <see cref="TextSize"/>.
                /// </summary>
                bool AutoResize { get; set; }

                /// <summary>
                /// If true, the text content will be vertically aligned to the center of the text board.
                /// </summary>
                bool VertCenterText { get; set; }

				/// <summary>
				/// Calculates and applies the minimum scroll offset required to bring the character at the specified index into view.
				/// </summary>
				/// <param name="index">The index of the character (X: Line/Char, Y: Column).</param>
				void MoveToChar(Vector2I index);

                /// <summary>
                /// Retrieves the index of the character located at the specified local offset.
                /// </summary>
                /// <param name="localPos">Position relative to the center of the TextBoard.</param>
                /// <returns>The index of the character (X: Line/Char, Y: Column).</returns>
                Vector2I GetCharAtOffset(Vector2 localPos);

                /// <summary>
                /// Renders the text board on the X/Y (Right/Up) plane of the given matrix tranform
                /// </summary>
                /// <param name="box">The bounding box defining the drawing area.</param>
                /// <param name="mask">The masking box for clipping text.</param>
                /// <param name="matrix">The orientation matrix. The text is drawn on the XY plane, facing +Z.</param>
                /// <exclude/>
                void Draw(BoundingBox2 box, BoundingBox2 mask, MatrixD[] matrix);
            }
        }
    }
}