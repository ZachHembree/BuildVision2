using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        public enum TextBuilderModes : int
        {
            /// <summary>
            /// In this mode, all text in the <see cref="Rendering.ITextBuilder"/> will all be on the same line.
            /// Line breaks are ignored and filtered from the text.
            /// </summary>
            Unlined = 1,

            /// <summary>
            /// In this mode, <see cref="Rendering.ITextBuilder"/> text can be separated into multiple lines with line
            /// breaks ('\n').
            /// </summary>
            Lined = 2,

            /// <summary>
            /// In this mode, <see cref="Rendering.ITextBuilder"/> text will be split into multiple lines as needed to
            /// ensure proper wrapping (in addition to manual line breaks).
            /// </summary>
            Wrapped = 3
        }

        namespace Rendering
        {
            public enum TextBuilderAccessors : int
            {
                /// <summary>
                /// in/out: float
                /// </summary>
                LineWrapWidth = 1,

                /// <summary>
                /// in/out: int (TextBuilderModes)
                /// </summary>
                BuilderMode = 2,

                /// <summary>
                /// in: Vector2I, Vector2I, out: List<RichStringMembers>
                /// </summary>
                GetRange = 3,

                /// <summary>
                /// int: GlyphFormatMembers
                /// </summary>
                SetFormatting = 4,

                /// <summary>
                /// in: Vector2I, Vector2I
                /// </summary>
                RemoveRange = 5,

                /// <summary>
                /// in/out: GlyphFormatMembers
                /// </summary>
                Format = 6,

                /// <summary>
                /// out: string
                /// </summary>
                ToString = 7,
            }

            public interface ITextBuilder : IIndexedCollection<ILine>
            {
                /// <summary>
                /// Returns the character at the index specified.
                /// </summary>
                IRichChar this[Vector2I index] { get; }

                /// <summary>
                /// Default text format. Applied to strings added without any other formatting specified.
                /// </summary>
                GlyphFormat Format { get; set; }

                /// <summary>
                /// Gets or sets the maximum line width before text will wrap to the next line. Word wrapping must be enabled for
                /// this to apply.
                /// </summary>
                float LineWrapWidth { get; set; }

                /// <summary>
                /// Determines the formatting mode of the text.
                /// </summary>
                TextBuilderModes BuilderMode { get; set; }

                /// <summary>
                /// Clears current text and appends the text given.
                /// </summary>
                void SetText(RichText text);

                /// <summary>
                /// Appends the given text to the end of the text using the <see cref="GlyphFormat"/>ting specified in the <see cref="RichText"/>.
                /// </summary>
                void Append(RichText text);

                /// <summary>
                /// Inserts the given text to the end of the text at the specified starting index using the <see cref="GlyphFormat"/>ting specified in the <see cref="RichText"/>.
                /// </summary>
                void Insert(RichText text, Vector2I start);

                /// <summary>
                /// Changes the formatting for the whole text to the given format.
                /// </summary>
                void SetFormatting(GlyphFormat format);

                /// <summary>
                /// Changes the formatting for the text within the given range to the given format.
                /// </summary>
                void SetFormatting(Vector2I start, Vector2I end, GlyphFormat format);

                /// <summary>
                /// Returns the contents of the text as <see cref="RichText"/>.
                /// </summary>
                RichText GetText();

                /// <summary>
                /// Returns the specified range of characters from the text as <see cref="RichText"/>.
                /// </summary>
                RichText GetTextRange(Vector2I start, Vector2I end);

                /// <summary>
                /// Removes the character at the specified index.
                /// </summary>
                void RemoveAt(Vector2I index);

                /// <summary>
                /// Removes all text within the specified range.
                /// </summary>
                void RemoveRange(Vector2I start, Vector2I end);

                /// <summary>
                /// Clears all existing text.
                /// </summary>
                void Clear();
            }
        }
    }
}