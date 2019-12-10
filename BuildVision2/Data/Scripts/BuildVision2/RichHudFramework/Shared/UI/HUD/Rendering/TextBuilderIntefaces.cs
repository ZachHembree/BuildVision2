using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;

namespace DarkHelmet
{
    namespace UI
    {
        namespace Rendering
        {
            public enum RichCharAccessors : int
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

            public enum LineAccessors : int
            {
                Count = 1,
                Size = 2
            }

            public interface ILine : IIndexedCollection<IRichChar>
            {
                Vector2 Size { get; }
            }

            public enum TextBuilderAccessors : int
            {
                LineWrapWidth = 1, // out: Bool
                WordWrapping = 2, // out: Bool
                GetRange = 3, // in: Vector2I, Vector2I, out: RichText
                SetFormatting = 4, // in: GlyphFormat
                RemoveRange = 5 // in: Vector2I, Vector2I
            }

            public interface ITextBuilder : IReadOnlyCollection<ILine>
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
                /// Determines whether or not word wrapping is enabled.
                /// </summary>
                bool WordWrapping { get; }

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

            public enum TextBoardAccessors : int
            {
                AutoResize = 129,
                VertAlign = 130,
                MoveToChar = 131,
                GetCharAtOffset = 132
            }

            public interface ITextBoard : ITextBuilder
            {
                /// <summary>
                /// Text size
                /// </summary>
                float Scale { get; set; }
                /// <summary>
                /// Size of the text box
                /// </summary>
                Vector2 Size { get; }
                Vector2 TextSize { get; }
                Vector2 FixedSize { get; set; }
                bool AutoResize { get; set; }
                bool VertCenterText { get; set; }

                void MoveToChar(Vector2I index);
                Vector2I GetCharAtOffset(Vector2 localPos);
            }
        }
    }
}