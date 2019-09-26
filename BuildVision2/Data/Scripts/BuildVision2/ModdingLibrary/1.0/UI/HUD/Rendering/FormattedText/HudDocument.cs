using System.Collections;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.UI.Rendering
{
    /// <summary>
    /// Contains the information needed to render an individual <see cref="Glyph"/> with a given
    /// <see cref="GlyphFormat"/>.
    /// </summary>
    public class RichChar : IRichChar
    {
        public char Ch { get; }
        public bool IsSeparator => (Ch == ' ' || Ch == '-' || Ch == '_');
        public bool IsLineBreak => Ch == '\n';
        public Glyph Glyph { get; private set; }
        public HudBoard GlyphBoard { get; private set; }
        public Vector2 Size { get; private set; }
        public GlyphFormat Formatting { get { return formatting; } }

        public GlyphFormat Format { get; internal set; }

        private GlyphFormat formatting;

        public RichChar(char ch, GlyphFormat formatting, float scale)
        {
            Ch = ch;
            GlyphBoard = new HudBoard();
            SetFormatting(formatting, scale);
        }

        public bool IsWordBreak(RichChar right) =>
            (IsSeparator && !right.IsSeparator);

        public void SetFormatting(GlyphFormat formatting, float scale)
        {
            scale *= formatting.scale * formatting.fontStyle.FontScale;

            this.formatting = formatting;
            Glyph = formatting.fontStyle[Ch];
            Size = new Vector2(Glyph.advanceWidth, formatting.fontStyle.height) * scale;

            GlyphBoard.MatAlignment = MaterialAlignment.FitVertical;
            GlyphBoard.Material = Glyph.material;
            GlyphBoard.Color = formatting.color;
            GlyphBoard.Size = Glyph.material.size * scale;
        }
    }

    /// <summary>
    /// A collection of word-wrapped <see cref="RichText"/> in which every <see cref="RichChar"/> on every <see cref="ILine"/>
    /// can be arbitrarily formatted to use any color, size or font.
    /// </summary>
    public abstract partial class HudDocument
    {
        public bool WordWrapping { get; }
        public virtual float Scale { get { return formatter.Scale; } set { formatter.Scale = value; } }

        protected readonly List<Line> lines;
        private readonly FormattedTextBase formatter;
        private readonly WrappedText wrappedText;

        public HudDocument(bool wordWrapping, int capacity)
        {
            lines = new List<Line>(capacity);
            WordWrapping = wordWrapping;

            if (wordWrapping)
            {
                wrappedText = new WrappedText(lines);
                formatter = wrappedText;
            }
            else
                formatter = new LinedText(lines);

            Scale = 1f;
        }

        protected void SetLineWrapWidth(float width)
        {
            if (WordWrapping)
            {
                wrappedText.SetMaxLineWidth(width);
                AfterTextUpdate();
            }
        }

        public void Append(RichString text)
        {
            formatter.Append(text);
            AfterTextUpdate();
        }

        public void Append(RichText text)
        {
            formatter.Append(text);
            AfterTextUpdate();
        }

        public void Clear()
        {
            formatter.Clear();
        }

        public void Insert(RichString text, Vector2I start)
        {
            formatter.Insert(text, start);
            AfterTextUpdate();
        }

        public void Insert(RichText text, Vector2I start)
        {
            formatter.Insert(text, start);
            AfterTextUpdate();
        }

        public RichText GetText()
        {
            if (lines.Count > 0)
                return GetTextRange(Vector2I.Zero, new Vector2I(lines.Count - 1, lines[lines.Count - 1].Count));
            else
                return new RichText();
        }

        public RichText GetTextRange(Vector2I start, Vector2I end)
        {
            RichText text = new RichText();
            RichString richString;

            text.Add(GetLineRange(start.X, start.Y, lines[start.X].Count - 1));

            if (end.X > start.X)
            {
                for (int line = start.X + 1; line <= end.X - 1; line++)
                {
                    for (int ch = 0; ch < lines[line].Count; ch++)
                    {
                        GlyphFormat format = lines[line][ch].Format;
                        richString = new RichString() { format = format };
                        ch--;

                        do
                        {
                            ch++;
                            richString.Append(lines[line][ch].Ch);
                        }
                        while (ch + 1 < lines[line].Count && format.Equals(lines[line][ch + 1].Format));

                        text.Add(richString);
                    }
                }

                text.Add(GetLineRange(end.X, 0, end.Y));
            }

            return text;
        }

        public RichText GetLine(int line) =>
            GetLineRange(line, 0, lines[line].Count - 1);

        private RichText GetLineRange(int line, int start, int end)
        {
            RichText text = new RichText();
            RichString richString;

            for (int ch = start; ch <= end; ch++)
            {
                GlyphFormat format = lines[line][ch].Format;
                richString = new RichString() { format = format };
                ch--;

                do
                {
                    ch++;
                    richString.Append(lines[line][ch].Ch);
                }
                while (ch + 1 < lines[line].Count && format.Equals(lines[line][ch + 1].Format));

                text.Add(richString);
            }

            return text;
        }

        public void RemoveRange(Vector2I start, Vector2I end)
        {
            formatter.RemoveRange(start, end);
            AfterTextUpdate();
        }

        public void SetFormatting(Vector2I start, Vector2I end, GlyphFormat formatting)
        {
            formatter.SetFormatting(start, end, formatting);
            AfterTextUpdate();
        }

        protected virtual void AfterTextUpdate()
        { }

        protected class Line : IIndexedEnumerable<RichChar>
        {
            public RichChar this[int index] { get { return chars[index]; } set { chars[index] = value; } }
            public int Count => chars.Count;
            public int Capacity => chars.Capacity;
            public Vector2 Size => size;

            private readonly List<RichChar> chars;
            private Vector2 size;

            public Line(int capacity = 6)
            {
                chars = new List<RichChar>(capacity);
            }

            public IEnumerator<RichChar> GetEnumerator() =>
                chars.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                GetEnumerator();

            public void Add(RichChar ch) =>
                chars.Add(ch);

            public void RemoveRange(int index, int count) =>
                chars.RemoveRange(index, count);

            public void Rescale(float scale)
            {
                size *= scale;
            }

            /// <summary>
            /// Recalculates the width and height of the line.
            /// </summary>
            public void UpdateSize()
            {
                size = Vector2.Zero;

                for (int n = 0; n < Count; n++)
                {
                    if (this[n].Size.Y > size.Y)
                        size.Y = this[n].Size.Y;

                    size.X += this[n].Size.X;
                }
            }
        }
    }
}