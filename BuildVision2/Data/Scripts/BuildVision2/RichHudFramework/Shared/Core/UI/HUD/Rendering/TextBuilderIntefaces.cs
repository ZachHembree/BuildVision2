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
            internal enum RichCharAccessors : int
            {
                Ch = 1,
                Format = 2,
                Size = 3,
                Offset = 4
            }

            public interface IRichChar
            {
                char Ch { get; }
                GlyphFormat Format { get; }
                Vector2 Size { get; }
                Vector2 Offset { get; }
            }

            internal enum LineAccessors : int
            {
                Count = 1,
                Size = 2
            }

            public interface ILine : IIndexedCollection<IRichChar>
            {
                Vector2 Size { get; }
            }

            internal enum TextBuilderAccessors : int
            {
                LineWrapWidth = 1, // out: Bool
                BuilderMode = 2, // out: Bool
                GetRange = 3, // in: Vector2I, Vector2I, out: RichText
                SetFormatting = 4, // in: GlyphFormat
                RemoveRange = 5, // in: Vector2I, Vector2I
                Format = 6,
            }

            public interface ITextBuilder : IIndexedCollection<ILine>
            {
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
                void SetText(string text);

                /// <summary>
                /// Clears current text and appends the text given.
                /// </summary>
                void SetText(RichString text);

                /// <summary>
                /// Clears current text and appends the text given.
                /// </summary>
                void SetText(RichText text);

                /// <summary>
                /// Appends the given <see cref="string"/> to the end of the text using the default <see cref="GlyphFormat"/>.
                /// </summary>
                void Append(string text);

                /// <summary>
                /// Appends the given text to the end of the text using the <see cref="GlyphFormat"/>ting specified in the <see cref="RichString"/>.
                /// </summary>
                void Append(RichString text);

                /// <summary>
                /// Appends the given text to the end of the text using the <see cref="GlyphFormat"/>ting specified in the <see cref="RichText"/>.
                /// </summary>
                void Append(RichText text);

                /// <summary>
                /// Inserts the given <see cref="string"/> at the given starting index using the default <see cref="GlyphFormat"/>.
                /// </summary>
                void Insert(string text, Vector2I start);

                /// <summary>
                /// Inserts the given text to the end of the text at the specified starting index using the <see cref="GlyphFormat"/>ting specified in the <see cref="RichString"/>.
                /// </summary>
                void Insert(RichString text, Vector2I start);

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

            internal enum TextBoardAccessors : int
            {
                AutoResize = 129,
                VertAlign = 130,
                MoveToChar = 131,
                GetCharAtOffset = 132,

                /// <summary>
                /// Action event
                /// </summary>
                OnTextChanged = 133,
            }

            public interface ITextBoard : ITextBuilder
            {
                event Action OnTextChanged;

                /// <summary>
                /// Text size
                /// </summary>
                float Scale { get; set; }

                /// <summary>
                /// Size of the text box as rendered
                /// </summary>
                Vector2 Size { get; }

                /// <summary>
                /// Full text size beginning with the StartLine
                /// </summary>
                Vector2 TextSize { get; }

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