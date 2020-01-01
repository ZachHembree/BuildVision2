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
        private class UnlinedText : FormattedTextBase
        {
            public UnlinedText(List<Line> lines) : base(lines)
            {
                FlattenText();
            }

            private void FlattenText()
            {
                int charCount = 0;

                for (int n = 0; n < Count; n++)
                    charCount += lines[n].Count;

                List<RichChar> chars = new List<RichChar>(charCount);

                for (int line = 0; line < Count; line++)
                {
                    for (int ch = 0; ch < lines[line].Count; ch++)
                    {
                        if (lines[line][ch].Ch >= ' ')
                            chars.Add(lines[line][ch]);
                    }
                }

                lines.Clear();
                lines.Add(new Line(chars.Count));

                lines[0].AddRange(chars);
                lines[0].UpdateSize();
            }

            /// <summary>
            /// Inserts text at a given position in the document.
            /// </summary>
            /// <param name="start">X = line; Y = ch</param>
            public override void Insert(IList<RichStringMembers> text, Vector2I start)
            {
                if (lines.Count == 0)
                    lines.Add(new Line());

                start.X = 0;
                start = ClampIndex(start);
                List<RichChar> chars = new List<RichChar>(((text.Count + 3) * 130) / 10);
                GlyphFormat previous = GetPreviousFormat(start);

                for (int n = 0; n < text.Count; n++)
                    GetRichChars(text[n], chars, previous, Scale, x => x >= ' ');

                lines[0].InsertRange(start.Y, chars);
                lines[0].UpdateSize();
            }

            /// <summary>
            /// Inserts a string starting on a given line at a given position.
            /// </summary>
            /// <param name="start">X = line; Y = ch</param>
            public override void Insert(RichStringMembers text, Vector2I start)
            {
                if (lines.Count == 0)
                    lines.Add(new Line());

                start.X = 0;
                start = ClampIndex(start);
                List<RichChar> chars = new List<RichChar>(((text.Item1.Length + 3) * 11) / 10);
                GlyphFormat previous = GetPreviousFormat(start);

                GetRichChars(text, chars, previous, Scale, x => x >= ' ');

                lines[0].InsertRange(start.Y, chars);
                lines[0].UpdateSize();
            }
        }
    }
}