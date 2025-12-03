using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {

        namespace Rendering
        {
            /// <summary>
            /// Internal API accessor indices for querying rendering information for an individual character
            /// </summary>
            /// <exclude/>
            public enum RichCharAccessors : int
            {
                /// <summary>
                /// out: char
                /// </summary>
                Ch = 1,

                /// <summary>
                /// out: GlyphFormatMembers
                /// </summary>
                Format = 2,

                /// <summary>
                /// out: Vector2
                /// </summary>
                Size = 3,

                /// <summary>
                /// out: Vector2
                /// </summary>
                Offset = 4
            }

			/// <summary>
			/// Represents a single glyph within a rich text element.
			/// <para>
			/// This interface exposes the character content, its specific formatting (font/color), 
			/// and its calculated layout geometry (size/offset) relative to the container.
			/// </para>
			/// <para>
			/// <strong>Warning:</strong> This object is a transient flyweight. It points to an index 
			/// in a mutable buffer. Do not store references to <see cref="IRichChar"/> across text updates.
			/// If the parent text changes, this interface may point to stale or recycled data.
			/// </para>
			/// </summary>
			public interface IRichChar
            {
                /// <summary>
                /// The character associated with this glyph.
                /// </summary>
                char Ch { get; }

                /// <summary>
                /// The styling and formatting configuration used by this character.
                /// </summary>
                GlyphFormat Format { get; }

                /// <summary>
                /// The dimensions of the glyph for layout purposes. 
                /// <para>X = Advance width (cursor movement, includes kerning).</para>
                /// <para>Y = Line height used by the font.</para>
                /// </summary>
                Vector2 Size { get; }

                /// <summary>
                /// The position of the glyph's **center** relative to the parent text element's origin.
                /// <para>This is updated during the layout pass.</para>
                /// </summary>
                Vector2 Offset { get; }
            }
        }
    }
}