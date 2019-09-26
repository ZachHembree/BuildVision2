using System.Collections;
using System.Collections.Generic;
using VRageMath;
using DarkHelmet;
using DarkHelmet.UI.Rendering;
using System.Text;

namespace DarkHelmet.UI
{
    /// <summary>
    /// Contains information necessary for formatting the <see cref="Glyph"/>(s) of a Rich Text type.
    /// </summary>
    public class GlyphFormat
    {
        public static readonly GlyphFormat Default = new GlyphFormat();

        public readonly Font.Style fontStyle;
        public readonly Color color;
        public readonly float scale;

        public GlyphFormat(float scale, Color color, Font.Style fontStyle)
        {
            this.fontStyle = fontStyle;
            this.color = color;
            this.scale = scale;
        }

        public GlyphFormat(Color color, float scale = 1f)
        {
            fontStyle = FontManager.Default;
            this.color = color;
            this.scale = scale;
        }

        public GlyphFormat(float scale = 1f)
        {
            fontStyle = FontManager.Default;
            color = Color.White;
            this.scale = scale;
        }

        public GlyphFormat(GlyphFormat original)
        {
            fontStyle = original.fontStyle;
            color = original.color;
            scale = original.scale;
        }

        public override bool Equals(object obj)
        {
            if (obj is GlyphFormat)
            {
                GlyphFormat b = (GlyphFormat)obj;

                return fontStyle == b.fontStyle && color == b.color && scale == b.scale;
            }
            else
                return false;
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
        public StringBuilder text;
        public GlyphFormat format;

        public RichString(int capacity = 6)
        {
            text = new StringBuilder(capacity);
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

        public void GetRichChars(List<RichChar> chars, float scale)
        {
            for (int n = 0; n < text.Length; n++)
            {
                if (text[n] >= ' ' || text[n] == '\n')
                    chars.Add(new RichChar(text[n], format, scale));
            }
        }
    }

    /// <summary>
    /// A collection of <see cref="RichString"/>s. Any string supplied without formatting will use the Text's
    /// default formatting. Collection initialization syntax can be used with this type.
    /// </summary>
    public class RichText : IIndexedEnumerable<RichString>
    {
        public RichString this[int index] => strings[index];
        public int Count => strings.Count;

        public GlyphFormat defaultFormat;
        private readonly List<RichString> strings;

        public RichText(GlyphFormat defaultFormat = null)
        {
            strings = new List<RichString>();

            if (defaultFormat == null)
                this.defaultFormat = defaultFormat;
            else
                this.defaultFormat = GlyphFormat.Default;
        }

        public RichText(string text, GlyphFormat defaultFormat)
        {
            strings = new List<RichString>();
            Add(defaultFormat, text);
        }

        public IEnumerator<RichString> GetEnumerator() =>
            strings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        public void Add(string text) =>
            strings.Add(new RichString(text, defaultFormat));

        public void Add(RichText text) =>
            strings.AddRange(text);

        public void Add(RichString text) =>
            strings.Add(text);

        public void Add(GlyphFormat formatting, string text) =>
            strings.Add(new RichString(text, formatting));

        public void Add(string text, GlyphFormat formatting) =>
            Add(formatting, text);

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

    public interface IRichChar
    {
        char Ch { get; }
        Vector2 Size { get; }
        GlyphFormat Formatting { get; }
    }
}