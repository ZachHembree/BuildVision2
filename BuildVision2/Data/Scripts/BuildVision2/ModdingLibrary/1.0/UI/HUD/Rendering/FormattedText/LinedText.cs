using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.UI.Rendering
{
    public partial class HudDocument
    {
        private class LinedText : FormattedTextBase
        {
            public LinedText(List<Line> lines) : base(lines)
            { }

            public override void Insert(RichText text, Vector2I start)
            {
                start = ClampIndex(start);

                List<RichChar> chars = new List<RichChar>(((text.Count + 3) * 130) / 10);

                for (int n = 0; n < text.Count; n++)
                    text[n].GetRichChars(chars, Scale);

                InsertChars(chars, start);
            }

            public override void Insert(RichString text, Vector2I start)
            {
                start = ClampIndex(start);

                List<RichChar> chars = new List<RichChar>(((text.Length + 3) * 11) / 10);

                if (lines.Count > 0)
                {
                    for (int n = 0; n < start.Y; n++)
                        chars.Add(lines[start.X][n]);
                }

                text.GetRichChars(chars, Scale);
                InsertChars(chars, start);
            }

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