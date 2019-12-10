using DarkHelmet.UI.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace DarkHelmet
{
    namespace UI
    {
        using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

        /// <summary>
        /// Defines the formatting of the characters in <see cref="RichString"/>s or <see cref="RichText"/>.
        /// </summary>
        public class GlyphFormat
        {
            public static readonly GlyphFormat Black = new GlyphFormat(),
                White = new GlyphFormat(color: Color.White);

            public Vector2I StyleIndex => data.Item1;

            /// <summary>
            /// Determines the alignment (left, center, right) of a given piece of RichText.
            /// </summary>
            public TextAlignment Alignment => (TextAlignment)data.Item2;

            /// <summary>
            /// Text color
            /// </summary>
            public Color Color => data.Item3;

            /// <summary>
            /// Text size
            /// </summary>
            public float TextSize => data.Item4;

            /// <summary>
            /// Backing object used by API
            /// </summary>
            public readonly GlyphFormatMembers data;

            public GlyphFormat(Color color = default(Color), TextAlignment alignment = TextAlignment.Left, float textSize = 1f, Vector2I fontStyle = default(Vector2I))
            {
                if (color == default(Color))
                    color = Color.Black;

                data = new GlyphFormatMembers(fontStyle, (int)alignment, color, textSize);
            }

            public GlyphFormat(GlyphFormatMembers data)
            {
                this.data = data;
            }

            public GlyphFormat(GlyphFormat original)
            {
                data = original.data;
            }

            public GlyphFormat WithColor(Color color) =>
                new GlyphFormat(color, Alignment, TextSize, StyleIndex);

            public GlyphFormat WithAlignment(TextAlignment textAlignment) =>
                new GlyphFormat(Color, textAlignment, TextSize, StyleIndex);

            public GlyphFormat WithFont(Vector2I fontStyle) =>
                new GlyphFormat(Color, Alignment, TextSize, fontStyle);

            public GlyphFormat WithSize(float size) =>
                new GlyphFormat(Color, Alignment, size, StyleIndex);

            /// <summary>
            /// Determines whether or not two given <see cref="GlyphFormat"/>s share the same configuration.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj is GlyphFormat)
                    return data.Equals(((GlyphFormat)obj).data);
                else
                    return false;
            }

            public override int GetHashCode() =>
                data.GetHashCode();
        }

        /// <summary>
        /// A collection of <see cref="RichString"/>s. Any string supplied without formatting will use the text's
        /// default formatting. Collection initialization syntax can be used with this type.
        /// </summary>
        public class RichText : IReadOnlyCollection<RichString>
        {
            public RichString this[int index] => text[index];
            public int Count => text.Count;

            public GlyphFormat defaultFormat;
            private readonly List<RichString> text;

            public RichText(GlyphFormat defaultFormat = null)
            {
                text = new List<RichString>();

                if (defaultFormat == null)
                    this.defaultFormat = defaultFormat;
                else
                    this.defaultFormat = GlyphFormat.Black;
            }

            public RichText(IList<RichStringMembers> richStrings)
            {
                text = new List<RichString>(richStrings.Count);

                for (int n = 0; n < richStrings.Count; n++)
                    text.Add(new RichString(richStrings[n]));
            }

            public RichText(RichString text)
            {
                this.text = new List<RichString>();
                Add(text);
            }

            public RichText(string text, GlyphFormat defaultFormat)
            {
                this.text = new List<RichString>();
                Add(defaultFormat, text);
            }

            public IEnumerator<RichString> GetEnumerator() =>
                text.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                GetEnumerator();

            /// <summary>
            /// Adds a <see cref="string"/> to the text using the default format.
            /// </summary>
            public void Add(string text) =>
                this.text.Add(new RichString(text, defaultFormat));

            /// <summary>
            /// Adds a <see cref="RichText"/> to the collection using the formatting specified in the <see cref="RichText"/>.
            /// </summary>
            public void Add(RichText text) =>
                this.text.AddRange(text);

            /// <summary>
            /// Adds a <see cref="RichString"/> to the collection using the formatting specified in the <see cref="RichString"/>.
            /// </summary>
            /// <param name="text"></param>
            public void Add(RichString text) =>
                this.text.Add(text);

            /// <summary>
            /// Adds a <see cref="string"/> using the given <see cref="GlyphFormat"/>.
            /// </summary>
            public void Add(GlyphFormat formatting, string text) =>
                this.text.Add(new RichString(text, formatting));

            /// <summary>
            /// Adds a <see cref="string"/> using the given <see cref="GlyphFormat"/>.
            /// </summary>
            public void Add(string text, GlyphFormat formatting) =>
                Add(formatting, text);

            /// <summary>
            /// Adds a <see cref="string"/> to the text using the default format.
            /// </summary>
            public static RichText operator +(RichText left, string right)
            {
                left.Add(right);
                return left;
            }

            /// <summary>
            /// Adds a <see cref="RichString"/> to the collection using the formatting specified in the <see cref="RichString"/>.
            /// </summary>
            /// <param name="text"></param>
            public static RichText operator +(RichText left, RichString right)
            {
                left.Add(right);
                return left;
            }

            /// <summary>
            /// Adds a <see cref="RichText"/> to the collection using the formatting specified in the <see cref="RichText"/>.
            /// </summary>
            public static RichText operator +(RichText left, RichText right)
            {
                left.Add(right);
                return left;
            }

            public override string ToString()
            {
                StringBuilder rawText = new StringBuilder();

                for (int a = 0; a < text.Count; a++)
                {
                    for (int b = 0; b < text[a].Length; b++)
                        rawText.Append(text[a][b]);
                }

                return rawText.ToString();
            }

            public RichStringMembers[] GetApiData()
            {
                RichStringMembers[] data = new RichStringMembers[text.Count];

                for (int n = 0; n < data.Length; n++)
                    data[n] = text[n].GetApiData();

                return data;
            }
        }

        /// <summary>
        /// A string associated with a given <see cref="GlyphFormat"/>. Each glyph generated from a 
        /// <see cref="RichString"/> will share the same formatting.
        /// </summary>
        public class RichString
        {
            public char this[int index] => text[index];
            public int Length => text.Length;
            public GlyphFormat format;

            public readonly StringBuilder text;

            public RichString(int capacity = 6)
            {
                text = new StringBuilder(capacity);
            }

            public RichString(RichStringMembers data)
            {
                text = data.Item1;
                format = new GlyphFormat(data.Item2);
            }

            public RichString(StringBuilder text, GlyphFormat format = null)
            {
                if (format == null)
                    this.format = GlyphFormat.Black;
                else
                    this.format = format;

                this.text = text;
            }

            public RichString(string text, GlyphFormat format = null)
            {
                this.text = new StringBuilder(text.Length);

                if (format == null)
                    this.format = GlyphFormat.Black;
                else
                    this.format = format;

                this.text.Append(text);
            }

            public RichString(RichString text) : this(text.text, text.format)
            { }

            public void Append(StringBuilder text) =>
                this.text.Append(text);

            public void Append(string text) =>
                this.text.Append(text);

            public void Append(char ch) =>
                text.Append(ch);

            public override string ToString() =>
                text.ToString();

            public RichStringMembers GetApiData() =>
                new RichStringMembers(text, format.data);
        }
    }
}