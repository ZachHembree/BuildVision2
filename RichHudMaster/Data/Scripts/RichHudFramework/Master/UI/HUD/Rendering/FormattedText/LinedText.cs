using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace RichHudFramework.UI.Rendering
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    public partial class TextBuilder
    {
        private class LinedText : FormattedTextBase
        {
            public LinedText(List<Line> lines) : base(lines)
            { }

            /// <summary>
            /// Inserts text at a given position in the document.
            /// </summary>
            /// <param name="start">X = line; Y = ch</param>
            public override void Insert(IList<RichStringMembers> text, Vector2I start)
            {
                start = ClampIndex(start);
                List<RichChar> chars = new List<RichChar>(((text.Count + 3) * 130) / 10);
                GlyphFormat previous = GetPreviousFormat(start);

                for (int n = 0; n < text.Count; n++)
                    GetRichChars(text[n], chars, previous, Scale, x => (x >= ' ' || x == '\n'));

                InsertChars(chars, start);
            }

            /// <summary>
            /// Inserts a string starting on a given line at a given position.
            /// </summary>
            /// <param name="start">X = line; Y = ch</param>
            public override void Insert(RichStringMembers text, Vector2I start)
            {
                start = ClampIndex(start);
                List<RichChar> chars = new List<RichChar>(((text.Item1.Length + 3) * 11) / 10);
                GlyphFormat previous = GetPreviousFormat(start);

                if (lines.Count > 0)
                {
                    for (int n = 0; n < start.Y; n++)
                        chars.Add(lines[start.X][n]);
                }

                GetRichChars(text, chars, previous, Scale, x => (x >= ' ' || x == '\n'));
                InsertChars(chars, start);
            }

            /// <summary>
            /// Inserts a list of <see cref="RichChar"/> at a given index as a list of <see cref="Line"/>s.
            /// </summary>
            private void InsertChars(List<RichChar> chars, Vector2I splitStart)
            {
                if (lines.Count > 0)
                {
                    for (int y = splitStart.Y; y < lines[splitStart.X].Count; y++)
                        chars.Add(lines[splitStart.X][y]);

                    lines.RemoveAt(splitStart.X);
                }

                List<Line> newLines = GetLines(chars);
                InsertLines(newLines, splitStart.X);
            }

            /// <summary>
            /// Generates a list of lines separated only by line breaks.
            /// </summary>
            private static List<Line> GetLines(List<RichChar> chars)
            {
                Line currentLine = null;
                List<Line> newLines = new List<Line>();

                for (int a = 0; a < chars.Count; a++)
                {
                    if (currentLine == null || (chars[a].IsLineBreak && currentLine.Count > 0))
                    {
                        currentLine = new Line();
                        newLines.Add(currentLine);
                    }

                    currentLine.Add(chars[a]);
                }

                return newLines;
            }

            /// <summary>
            /// Inserts new lines at the given index and calculates the size of each line.
            /// </summary>
            private void InsertLines(List<Line> newLines, int start)
            {
                for (int n = 0; n < newLines.Count; n++)
                    newLines[n].UpdateSize();

                lines.InsertRange(start, newLines);

                if (lines.Capacity > 9 * lines.Count)
                    lines.TrimExcess();
            }
        }
    }
}