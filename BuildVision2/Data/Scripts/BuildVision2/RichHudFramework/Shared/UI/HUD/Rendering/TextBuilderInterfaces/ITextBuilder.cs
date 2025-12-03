using VRageMath;
using System.Text;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Line breaking modes for <see cref="Rendering.ITextBuilder"/> 
        /// </summary>
        public enum TextBuilderModes : int
        {
            /// <summary>
            /// Forces all text onto a single line. 
            /// Line break characters ('\n') are filtered out or ignored.
            /// </summary>
            Unlined = 1,

            /// <summary>
            /// Allows text to be separated into multiple lines using manual line breaks ('\n').
            /// Automatic wrapping is disabled.
            /// </summary>
            Lined = 2,

            /// <summary>
            /// Text is split into multiple lines automatically based on <see cref="Rendering.ITextBuilder.LineWrapWidth"/>, 
            /// in addition to manual line breaks ('\n').
            /// </summary>
            Wrapped = 3
        }

        namespace Rendering
        {
            /// <summary>
            /// Internal API accessor indices for TextBuilder/TextBoard
            /// </summary>
            /// <exclude/>
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
                /// in: Vector2I, Vector2I, out: List{RichStringMembers}
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

            /// <summary>
            /// A mutable collection of lines and characters that supports RichText formatting.
            /// <para>
            /// Functioning similarly to a <see cref="StringBuilder"/>, this interface allows for appending, 
            /// inserting, and removing text while maintaining formatting data (colors, fonts) and handling 
            /// line wrapping modes (Unlined, Lined, Wrapped).
            /// </para>
            /// </summary>
            public interface ITextBuilder : IIndexedCollection<ILine>
            {
				/// <summary>
				/// Retrieves the <see cref="IRichChar"/> at the specified index.
				/// <para>
				/// Note: This creates a new wrapper object on every call. Avoid iterating this collection 
				/// frequently in tight loops. Reference equality checks between calls will fail.
				/// </para>
				/// </summary> 
				/// <param name="index">X: Line Index, Y: Column Index</param>
				IRichChar this[Vector2I index] { get; }

                /// <summary>
                /// The default text format applied to strings added without explicit formatting.
                /// </summary>
                GlyphFormat Format { get; set; }

                /// <summary>
                /// The maximum width of a line before text wraps to the next line.
                /// <para>Only applies when <see cref="BuilderMode"/> is set to <see cref="TextBuilderModes.Wrapped"/>.</para>
                /// </summary>
                float LineWrapWidth { get; set; }

				/// <summary>
				/// Controls the line-breaking behavior of the text builder (Unlined, Lined, or Wrapped).
				/// </summary>
				TextBuilderModes BuilderMode { get; set; }

                /// <summary>
                /// Replaces the current text content with the provided <see cref="RichText"/>.
                /// </summary>
                void SetText(RichText text);

                /// <summary>
                /// Clears the current text and sets it to a copy of the provided <see cref="StringBuilder"/>.
                /// </summary>
                /// <param name="format">Optional format to apply. If null, <see cref="Format"/> is used.</param>
                void SetText(StringBuilder text, GlyphFormat? format = null);

                /// <summary>
                /// Clears the current text and sets it to the provided <see cref="string"/>.
                /// </summary>
                /// <param name="format">Optional format to apply. If null, <see cref="Format"/> is used.</param>
                void SetText(string text, GlyphFormat? format = null);

                /// <summary>
                /// Appends the given <see cref="RichText"/> to the end of the current content.
                /// </summary>
                void Append(RichText text);

                /// <summary>
                /// Appends a copy of the <see cref="StringBuilder"/> to the end of the current content.
                /// </summary>
                /// <param name="format">Optional format to apply. If null, <see cref="Format"/> is used.</param>
                void Append(StringBuilder text, GlyphFormat? format = null);

                /// <summary>
                /// Appends the <see cref="string"/> to the end of the current content.
                /// </summary>
                /// <param name="format">Optional format to apply. If null, <see cref="Format"/> is used.</param>
                void Append(string text, GlyphFormat? format = null);

                /// <summary>
                /// Appends the <see cref="char"/> to the end of the current content.
                /// </summary>
                /// <param name="format">Optional format to apply. If null, <see cref="Format"/> is used.</param>
                void Append(char ch, GlyphFormat? format = null);

                /// <summary>
                /// Inserts the given <see cref="RichText"/> starting at the specified index.
                /// </summary>
                /// <param name="start">Insertion index (X: Line, Y: Column).</param>
                void Insert(RichText text, Vector2I start);

                /// <summary>
                /// Inserts a copy of the <see cref="StringBuilder"/> starting at the specified index.
                /// </summary>
                /// <param name="start">Insertion index (X: Line, Y: Column).</param>
                /// <param name="format">Optional format to apply.</param>
                void Insert(StringBuilder text, Vector2I start, GlyphFormat? format = null);

                /// <summary>
                /// Inserts the <see cref="string"/> starting at the specified index.
                /// </summary>
                /// <param name="start">Insertion index (X: Line, Y: Column).</param>
                /// <param name="format">Optional format to apply.</param>
                void Insert(string text, Vector2I start, GlyphFormat? format = null);

                /// <summary>
                /// Inserts the <see cref="char"/> starting at the specified index.
                /// </summary>
                /// <param name="start">Insertion index (X: Line, Y: Column).</param>
                /// <param name="format">Optional format to apply.</param>
                void Insert(char text, Vector2I start, GlyphFormat? format = null);

                /// <summary>
                /// Applies the specified formatting to the entire text content.
                /// </summary>
                void SetFormatting(GlyphFormat format);

                /// <summary>
                /// Applies the specified formatting to the characters within the given range.
                /// </summary>
                /// <param name="start">Start index.</param>
                /// <param name="end">End index.</param>
                void SetFormatting(Vector2I start, Vector2I end, GlyphFormat format);

                /// <summary>
                /// Returns the entire contents of the text builder as <see cref="RichText"/>.
                /// </summary>
                RichText GetText();

                /// <summary>
                /// Returns the text within the specified range as <see cref="RichText"/>.
                /// </summary>
                /// <param name="start">Start index.</param>
                /// <param name="end">End index.</param>
                RichText GetTextRange(Vector2I start, Vector2I end);

                /// <summary>
                /// Removes the character at the specified index.
                /// </summary>
                void RemoveAt(Vector2I index);

                /// <summary>
                /// Removes all characters within the specified range.
                /// </summary>
                /// <param name="start">Start index.</param>
                /// <param name="end">End index.</param>
                void RemoveRange(Vector2I start, Vector2I end);

                /// <summary>
                /// Clears all existing text.
                /// </summary>
                void Clear();
            }
        }
    }
}