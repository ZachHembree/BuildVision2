using System;
using System.Collections.Generic;
using System.Text;
using VRage;
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
                /// Size of the glyph as rendered
                /// </summary>
                Vector2 Size { get; }

                /// <summary>
                /// Position of the glyph relative to the center of its parent text element. Does not include the 
                /// parent's TextOffset.
                /// </summary>
                Vector2 Offset { get; }
            }

            public enum LineAccessors : int
            {
                /// <summary>
                /// out: int
                /// </summary>
                Count = 1,

                /// <summary>
                /// out: Vector2
                /// </summary>
                Size = 2
            }

            public interface ILine : IIndexedCollection<IRichChar>
            {
                Vector2 Size { get; }
            }

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
            }

            public interface ITextBoard : ITextBuilder
            {
                /// <summary>
                /// Invoked whenever a change is made to the text.
                /// </summary>
                event Action OnTextChanged;

                /// <summary>
                /// Scale of the text board. Applied after scaling specified in GlyphFormat.
                /// </summary>
                float Scale { get; set; }

                /// <summary>
                /// Size of the text box as rendered
                /// </summary>
                Vector2 Size { get; }

                /// <summary>
                /// Full text size including any text outside the visible range.
                /// </summary>
                Vector2 TextSize { get; }

                /// <summary>
                /// Used to change the position of the text within the text element. AutoResize must be disabled for this to work.
                /// </summary>
                Vector2 TextOffset { get; set; }

                /// <summary>
                /// Size of the text box when AutoResize is set to false. Does nothing otherwise.
                /// </summary>
                Vector2 FixedSize { get; set; }

                /// <summary>
                /// If true, the text board will automatically resize to fit the text.
                /// </summary>
                bool AutoResize { get; set; }

                /// <summary>
                /// If true, the text will be vertically aligned to the center of the text board.
                /// </summary>
                bool VertCenterText { get; set; }

                /// <summary>
                /// Calculates and applies the minimum offset needed to ensure that the character at the specified index
                /// is within the visible range.
                /// </summary>
                void MoveToChar(Vector2I index);

                /// <summary>
                /// Returns the index of the character at the given offset.
                /// </summary>
                Vector2I GetCharAtOffset(Vector2 localPos);
            }
        }
    }
}