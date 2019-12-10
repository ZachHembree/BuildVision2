using System;
using System.Collections.Generic;
using VRageMath;
using VRage;
using System.Text;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace DarkHelmet.UI.Rendering
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    public partial class TextBuilder
    {
        private class WrappedText : FormattedTextBase
        {
            public WrappedText(List<Line> lines) : base(lines)
            { }

            /// <summary>
            /// Sets the maximum width for a line in the text before its wrapped to the next line and updates
            /// text wrapping.
            /// </summary>
            public void SetWrapWidth(float width)
            {
                if (width < MaxLineWidth - 2f || width > MaxLineWidth + 4f)
                {
                    MaxLineWidth = width;
                    Rewrap(); // not ideal
                }
            }

            /// <summary>
            /// Inserts text at a given position in the document.
            /// </summary>
            /// <param name="start">X = line; Y = ch</param>
            public override void Insert(IList<RichStringMembers> text, Vector2I start)
            {
                start = ClampIndex(start);

                List<RichChar> chars = new List<RichChar>(((text.Count + 3) * 130) / 10);
                int insertStart = GetInsertStart(chars, start);

                for (int n = 0; n < text.Count; n++)
                    GetRichChars(text[n], chars, Scale);

                InsertChars(chars, insertStart, start);
            }

            /// <summary>
            /// Inserts a string starting on a given line at a given position.
            /// </summary>
            public override void Insert(RichStringMembers text, Vector2I start)
            {
                start = ClampIndex(start);

                List<RichChar> chars = new List<RichChar>(((text.Item1.Length + 3) * 11) / 10);
                int insertStart = GetInsertStart(chars, start);

                GetRichChars(text, chars, Scale);
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

                if (start.X < lines.Count - 1)
                    TryPullToLine(start.X);
            }

            /// <summary>
            /// Regenerates text wrapping for the entire document.
            /// </summary>
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

            /// <summary>
            /// Regenerates text wrapping for the specified range of lines.
            /// </summary>
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

            /// <summary>
            /// Inserts a list of <see cref="RichChar"/>s at the given starting index and updates wrapping of
            /// the surrounding text.
            /// </summary>
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

            /// <summary>
            /// Gets characters from the word immediately preceeding the insert in order to ensure proper wrapping.
            /// Returns the index of the line the insert will start on after the relevant preceeding text is appended.
            /// </summary>
            private int GetInsertStart(List<RichChar> chars, Vector2I splitStart)
            {
                Vector2I splitEnd;

                if (TryGetLastIndex(splitStart, out splitEnd))
                {
                    splitStart = GetWordStart(splitEnd); // Find word start immediately preceeding splitStart
                    splitStart.Y = 0; // So you pull the whole line

                    Vector2I i = splitStart;

                    do
                    {
                        chars.Add(this[i]);
                    }
                    while (TryGetNextIndex(i, out i) && (i.X < splitEnd.X || (i.X == splitEnd.X && i.Y <= splitEnd.Y)));
                }

                return splitStart.X;
            }

            /// <summary>
            /// Generates a new list of wrapped <see cref="Line"/>s from a list of <see cref="RichChar"/>. Uses precalculated list
            /// width to estimate the size of the collection.
            /// </summary>
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

            /// <summary>
            /// Inserts a list of lines at the specified starting index and updates the wrapping of the lines following
            /// as needed.
            /// </summary>
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

            /// <summary>
            /// Attempts to pull text from the lines following to the one specified while maintaining proper word wrapping.
            /// </summary>
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

            /// <summary>
            /// Calculates the total width of a given list of characters.
            /// </summary>
            private static float GetCharListWidth(List<RichChar> chars)
            {
                float width = 0f;

                for (int n = 0; n < chars.Count; n++)
                    width += chars[n].Size.X;

                return width;
            }

            /// <summary>
            /// Gets the position of the beginning of a word.
            /// </summary>
            private Vector2I GetWordStart(Vector2I end)
            {
                Vector2I start;

                while (TryGetLastIndex(end, out start) && !(this[end].IsLineBreak || this[start].IsWordBreak(this[end])))
                    end = start;

                return start;
            }

            /// <summary>
            /// Gets the position of the end of a word while staying without exceeding the given space remaining.
            /// </summary>
            /// <param name="start">Where the search begins, not necessarily the beginning of the word.</param>
            /// <param name="end">Somewhere to the right of or equal to the start.</param>
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

            /// <summary>
            /// Tries to find the end of a word in a list of characters starting at a given index.
            /// </summary>
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