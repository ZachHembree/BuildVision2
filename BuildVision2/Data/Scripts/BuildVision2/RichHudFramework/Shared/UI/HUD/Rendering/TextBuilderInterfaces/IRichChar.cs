using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {

        namespace Rendering
        {
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

            public interface IRichChar
            {
                /// <summary>
                /// Character assocated with the glyph
                /// </summary>
                char Ch { get; }

                /// <summary>
                /// Text format used by the character
                /// </summary>
                GlyphFormat Format { get; }

				/// <summary>
				/// Size of the glyph as rendered. 
				/// X = Advance width (cursor movement after this character, including kerning).
				/// Y = Line height.
				/// </summary>
				Vector2 Size { get; }

				/// <summary>
				/// Position of the glyph **center** relative to the parent text element.
				/// Updated during layout. Use with Size.X/2 to get left/right edges.
				/// </summary>
				Vector2 Offset { get; }
			}
        }
    }
}