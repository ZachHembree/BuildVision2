using System;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.UI.Rendering
{
    public partial class HudDocument
    {
        private class WrappedText : FormattedTextBase
        {
            /// <summary>
            /// The maximum with of any given line before the text will wrap to the next line.
            /// </summary>
            public float MaxLineWidth { get; private set; }

            public WrappedText(List<Line> lines) : base(lines)
            { }

            public void SetMaxLineWidth(float width)
            {
                if (width < MaxLineWidth - 2f || width > MaxLineWidth + 4f)
                {
                    MaxLineWidth = width;
                    Rewrap();
                }
            }

            /// <summary>
            /// Inserts text at a given position in the document.
            /// </summary>
            /// <param name="pos">X = line; Y = ch</param>
            public override void Insert(RichText text, Vector2I start)
            {
                start = ClampIndex(start);

                List<RichChar> chars = new List<RichChar>(((text.Count + 3) * 130) / 10);
                int insertStart = GetInsertStart(chars, start);

                for (int n = 0; n < text.Count; n++)
                    text[n].GetRichChars(chars, Scale);

                InsertChars(chars, insertStart, start);
            }

            /// <summary>
            /// Inserts a string starting on a given line at a given position.
            /// </summary>
            public override void Insert(RichString text, Vector2I start)
            {
                start = ClampIndex(start);

                List<RichChar> chars = new List<RichChar>(((text.Length + 3) * 11) / 10);
                int insertStart = GetInsertStart(chars, start);

                text.GetRichChars(chars, Scale);
                InsertChars(chars, insertStart, start);
            }

            /// <summary>
            /// Applies glyph formatting to a range of characters.
            /// </summary>
            /// <param name="start">Position of the first character being formatted.</param>
            /// <param name="end">Position of the last character being formatted.</param>
            public override void SetFormatting(Vector2I start, Vector2I end, GlyphFormat formatting)
            {
                base.SetFormatting(start, end, formatting);

                RewrapRange(start.X, end.X);
            }

            /// <summary>
            /// Removes characters within a specified range.
            /// </summary>
            public override void RemoveRange(Vector2I start, Vector2I end)
            {
                base.RemoveRange(start, end);

                if (start.X + 1 < lines.Count)
                    TryPullToLine(start.X);
            }

            protected override void RescaleText(float scale)
            {
                base.RescaleText(scale);

                MaxLineWidth *= scale;
            }

            private void Rewrap()
            {
                int charCount = 0;

                for (int n = 0; n < Count; n++)
                    charCount += lines[n].Count;

                List<RichChar> chars = new List<RichChar>(charCount);

                for (int n = 0; n < Count; n++)
                    chars.AddRange(lines[n]);

                lines.Clear();
                lines.AddRange(GetLines(chars, GetCharListWidth(chars)));

                for (int n = 0; n < lines.Count; n++)
                    lines[n].UpdateSize();
            }

            private void RewrapRange(int start, int end)
            {
                int charCount = 0;

                for (int n = start; n <= end; n++)
                    charCount += lines[n].Count;

                List<RichChar> chars = new List<RichChar>(charCount);
                int insertStart = GetInsertStart(chars, new Vector2I(start, 0));

                for (int n = start; n <= end; n++)
                    chars.AddRange(lines[n]);

                lines.RemoveRange(insertStart, insertStart - end + 1);
                List<Line> newLines = GetLines(chars, GetCharListWidth(chars));
                InsertLines(newLines, insertStart);
            }

            private void InsertChars(List<RichChar> chars, int startLine, Vector2I splitStart)
            {
                if (lines.Count > 0)
                {
                    for (int y = splitStart.Y; y < lines[splitStart.X].Count; y++)
                        chars.Add(lines[splitStart.X][y]);

                    lines.RemoveRange(startLine, splitStart.X - startLine + 1);
                }

                List<Line> newLines = GetLines(chars, GetCharListWidth(chars));
                InsertLines(newLines, startLine);
            }

            private int GetInsertStart(List<RichChar> chars, Vector2I splitStart)
            {
                Vector2I splitEnd;

                if (TryGetLastIndex(splitStart, out splitEnd))
                {
                    splitStart = GetWordStart(splitEnd);
                    splitStart.Y = 0;

                    Vector2I i = splitStart;

                    do
                    {
                        chars.Add(this[i]);
                    }
                    while (TryGetNextIndex(i, out i) && (i.X < splitEnd.X || (i.X == splitEnd.X && i.Y <= splitEnd.Y)));
                }

                return splitStart.X;
            }

            private List<Line> GetLines(List<RichChar> chars, float listWidth)
            {
                Line currentLine = null;
                List<Line> newLines = new List<Line>(Math.Max(3, (int)(1.1f * (listWidth / MaxLineWidth))));
                int estLineLength = Math.Max(3, (int)(chars.Count / (listWidth / MaxLineWidth)) / 2), end;
                float wordWidth, spaceRemaining = -1f;

                for (int start = 0; TryGetWordEnd(chars, start, out end, out wordWidth); start = end + 1)
                {
                    bool wrapWord = (spaceRemaining < wordWidth && wordWidth <= MaxLineWidth) || chars[start].IsLineBreak;

                    for (int n = start; n <= end; n++)
                    {
                        if (spaceRemaining < chars[n].Size.X || wrapWord)
                        {
                            spaceRemaining = MaxLineWidth;
                            currentLine = new Line(estLineLength);

                            newLines.Add(currentLine);
                            wrapWord = false;
                        }

                        currentLine.Add(chars[n]);
                        spaceRemaining -= chars[n].Size.X;
                    }
                }

                return newLines;
            }

            private void InsertLines(List<Line> newLines, int start)
            {
                for (int n = 0; n < newLines.Count; n++)
                    newLines[n].UpdateSize();

                lines.InsertRange(start, newLines);

                while (start + newLines.Count < lines.Count && TryPullToLine(start + newLines.Count - 1))
                    start++;

                if (lines.Capacity > 9 * lines.Count)
                    lines.TrimExcess();
            }

            private bool TryPullToLine(int line)
            {
                float spaceRemaining = MaxLineWidth - lines[line].Size.X;
                Vector2I i = new Vector2I(line + 1, 0), wordEnd, end = new Vector2I();

                while (TryGetWordEnd(i, out wordEnd, ref spaceRemaining) && !this[i].IsLineBreak)
                {
                    end = wordEnd;

                    do
                    {
                        lines[line].Add(this[i]);
                    }
                    while (TryGetNextIndex(i, out i) && (i.X < wordEnd.X || (i.X == wordEnd.X && i.Y <= wordEnd.Y)));
                }

                if (end.X > line)
                {
                    if (end.Y < lines[end.X].Count - 1)
                    {
                        lines[end.X].RemoveRange(0, end.Y + 1);
                        lines[end.X].UpdateSize();
                        lines.RemoveRange(line + 1, end.X - line - 1);
                    }
                    else
                        lines.RemoveRange(line + 1, end.X - line);

                    lines[line].UpdateSize();
                    return true;
                }
                else
                    return false;
            }

            private static float GetCharListWidth(List<RichChar> chars)
            {
                float width = 0f;

                for (int n = 0; n < chars.Count; n++)
                    width += chars[n].Size.X;

                return width;
            }

            /// <summary>
            /// Determines the position of the beginning of a word.
            /// </summary>
            /// <param name="end">Where the search begins, not necessarily the end of the word.</param>
            /// <param name="start">Somewhere left of or equal to the end.</param>
            /// <returns></returns>
            private Vector2I GetWordStart(Vector2I end)
            {
                Vector2I start;

                while (TryGetLastIndex(end, out start) && !(this[end].IsLineBreak || this[start].IsWordBreak(this[end])))
                    end = start;

                return start;
            }

            /// <summary>
            /// Determines the position of the end of a word.
            /// </summary>
            /// <param name="start">Where the search begins, not necessarily the beginning of the word.</param>
            /// <param name="end">Somewhere to the right of or equal to the start.</param>
            /// <returns></returns>
            private bool TryGetWordEnd(Vector2I start, out Vector2I end, ref float spaceRemaining)
            {
                spaceRemaining -= this[start].Size.X;

                while (TryGetNextIndex(start, out end) && spaceRemaining > 0f && !(this[end].IsLineBreak || this[start].IsWordBreak(this[end])))
                {
                    spaceRemaining -= this[end].Size.X;
                    start = end;
                }

                end = start;
                return spaceRemaining > 0f;
            }

            private bool TryGetWordEnd(List<RichChar> chars, int start, out int wordEnd, out float width)
            {
                wordEnd = -1;
                width = 0f;

                for (int n = start; n < chars.Count; n++)
                {
                    width += chars[n].Size.X;

                    if (n == (chars.Count - 1) || chars[n + 1].IsLineBreak || chars[n].IsWordBreak(chars[n + 1]))
                    {
                        wordEnd = n;
                        return true;
                    }
                }

                return false;
            }
        }
    }
}