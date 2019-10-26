using DarkHelmet.UI.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, VRageMath.Color, float>;

namespace DarkHelmet.UI
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    /// <summary>
    /// Contains information necessary for formatting the <see cref="Glyph"/>(s) of a Rich Text type.
    /// </summary>
    public class GlyphFormat
    {
        public static readonly GlyphFormat Default = new GlyphFormat();

        public Vector2I StyleIndex => data.Item1;
        public Color Color => data.Item2;
        public float Scale => data.Item3;
        public readonly GlyphFormatMembers data;

        public GlyphFormat(float scale, Color color, Vector2I styleIndex)
        {
            data = new GlyphFormatMembers(styleIndex, color, scale);
        }

        public GlyphFormat(Color color, float scale = 1f)
        {
            data = new GlyphFormatMembers(Vector2I.Zero, color, scale);
        }

        public GlyphFormat(float scale = 1f)
        {
            data = new GlyphFormatMembers(Vector2I.Zero, Color.White, scale);
        }

        public GlyphFormat(GlyphFormatMembers data)
        {
            this.data = data;
        }

        public GlyphFormat(GlyphFormat original)
        {
            data = original.data;
        }

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
    /// A collection of <see cref="RichString"/>s. Any string supplied without formatting will use the Text's
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
                this.defaultFormat = GlyphFormat.Default;
        }

        public RichText(IList<RichStringMembers> richStrings)
        {
            text = new List<RichString>(richStrings.Count);

            for (int n = 0; n < richStrings.Count; n++)
                text.Add(new RichString(richStrings[n]));
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

        public void Add(string text) =>
            this.text.Add(new RichString(text, defaultFormat));

        public void Add(RichText text) =>
            this.text.AddRange(text);

        public void Add(RichString text) =>
            this.text.Add(text);

        public void Add(GlyphFormat formatting, string text) =>
            this.text.Add(new RichString(text, formatting));

        public void Add(string text, GlyphFormat formatting) =>
            Add(formatting, text);

        public RichStringMembers[] GetApiData()
        {
            RichStringMembers[] data = new RichStringMembers[text.Count];

            for (int n = 0; n < data.Length; n++)
                data[n] = text[n].GetApiData();

            return data;
        }

        public static RichText operator +(RichText left, string right)
        {
            left.Add(right);
            return left;
        }

        public static RichText operator +(RichText left, RichString right)
        {
            left.Add(right);
            return left;
        }

        public static RichText operator +(RichText left, RichText right)
        {
            left.Add(right);
            return left;
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

        private readonly StringBuilder text;

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
                this.format = GlyphFormat.Default;
            else
                this.format = format;

            this.text = text;
        }

        public RichString(string text, GlyphFormat format = null)
        {
            this.text = new StringBuilder(text.Length);

            if (format == null)
                this.format = GlyphFormat.Default;
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

        public RichStringMembers GetApiData() =>
            new RichStringMembers(text, format.data);
    }
}