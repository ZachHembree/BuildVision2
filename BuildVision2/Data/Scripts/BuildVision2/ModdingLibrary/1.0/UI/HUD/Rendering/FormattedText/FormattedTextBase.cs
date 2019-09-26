using DarkHelmet;
using System;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.UI.Rendering
{
    public partial class HudDocument
    {
        private abstract class FormattedTextBase : IIndexedCollection<Line>
        {
            public Line this[int index] => lines[index];
            public virtual int Count => lines.Count;
            public virtual int Capacity => lines.Capacity;
            public float Scale { get { return scale; } set { RescaleText(value / scale); scale = value; } }

            protected RichChar this[Vector2I index] => lines[index.X][index.Y];
            protected readonly List<Line> lines;
            private float scale;

            protected FormattedTextBase(List<Line> lines)
            {
                this.lines = lines;
                Scale = 1f;
            }

            /// <summary>
            /// Appends text to the end of the document.
            /// </summary>
            public virtual void Append(RichText text)
            {
                Vector2I start = new Vector2I(Math.Max(0, lines.Count - 1), 0);

                if (lines.Count > 0)
                    start.Y = Math.Max(0, lines[start.X].Count);

                Insert(text, start);
            }

            /// <summary>
            /// Appends text to the end of the document.
            /// </summary>
            public virtual void Append(RichString text)
            {
                Vector2I start = new Vector2I(Math.Max(0, lines.Count - 1), 0);

                if (lines.Count > 0)
                    start.Y = Math.Max(0, lines[start.X].Count);

                Insert(text, start);
            }

            /// <summary>
            /// Clears all existing text from the document.
            /// </summary>
            public virtual void Clear() =>
                lines.Clear();

            /// <summary>
            /// Inserts a string starting on a given line at a given position.
            /// </summary>
            public abstract void Insert(RichString text, Vector2I start);

            /// <summary>
            /// Inserts text at a given position in the document.
            /// </summary>
            /// <param name="pos">X = line; Y = ch</param>
            public abstract void Insert(RichText text, Vector2I start);

            /// <summary>
            /// Applies glyph formatting to a range of characters.
            /// </summary>
            /// <param name="start">Position of the first character being formatted.</param>
            /// <param name="end">Position of the last character being formatted.</param>
            public virtual void SetFormatting(Vector2I start, Vector2I end, GlyphFormat formatting)
            {
                for (int y = start.Y; y < lines[start.X].Count; y++)
                    lines[start.X][y].SetFormatting(formatting, Scale);

                for (int x = start.X + 1; x < end.X; x++)
                {
                    for (int y = 0; y < lines[x].Count; y++)
                        lines[x][y].SetFormatting(formatting, Scale);
                }

                for (int y = 0; y <= end.Y; y++)
                    lines[end.X][y].SetFormatting(formatting, Scale);

                for (int n = start.X; n <= end.X; n++)
                    lines[n].UpdateSize();
            }

            /// <summary>
            /// Removes characters within a specified range.
            /// </summary>
            public virtual void RemoveRange(Vector2I start, Vector2I end)
            {
                lines[start.X].RemoveRange(start.X, lines[start.X].Count - start.X);
                lines[end.X].RemoveRange(0, lines[start.X].Count - end.X);

                if (start.X + 1 < lines.Count)
                    lines.RemoveRange(start.X + 1, end.X - start.X - 1);

                if (lines.Capacity > 9 * lines.Count)
                    lines.TrimExcess();
            }

            protected virtual void RescaleText(float scale)
            {
                for (int line = 0; line < lines.Count; line++)
                {
                    lines[line].Rescale(scale);

                    for (int ch = 0; ch < lines[line].Count; ch++)
                    {
                        lines[line][ch].GlyphBoard.Size *= scale;
                        lines[line][ch].GlyphBoard.offset *= scale;
                    }
                }
            }

            protected Vector2I ClampIndex(Vector2I index)
            {
                index.X = Utils.Math.Clamp(index.X, 0, lines.Count);
                index.Y = Utils.Math.Clamp(index.Y, 0, (lines.Count > 0) ? lines[index.X].Count : 0);

                return index;
            }

            protected virtual bool TryGetLastIndex(Vector2I index, out Vector2I lastIndex)
            {
                if (index.Y > 0)
                    lastIndex = new Vector2I(index.X, index.Y - 1);
                else if (index.X > 0)
                    lastIndex = new Vector2I(index.X - 1, lines[index.X - 1].Count - 1);
                else
                {
                    lastIndex = index;
                    return false;
                }

                return true;
            }

            protected virtual bool TryGetNextIndex(Vector2I index, out Vector2I nextIndex)
            {
                if (index.X < lines.Count && index.Y + 1 < lines[index.X].Count)
                    nextIndex = new Vector2I(index.X, index.Y + 1);
                else if (index.X + 1 < lines.Count)
                    nextIndex = new Vector2I(index.X + 1, 0);
                else
                {
                    nextIndex = index;
                    return false;
                }

                return true;
            }
        }
    }
}