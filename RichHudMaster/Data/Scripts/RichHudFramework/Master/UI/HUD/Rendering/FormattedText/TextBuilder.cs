using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace DarkHelmet
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;
    using Vec2Prop = MyTuple<Func<Vector2>, Action<Vector2>>;
    using FloatProp = MyTuple<Func<float>, Action<float>>;
    using BoolProp = MyTuple<Func<bool>, Action<bool>>;

    namespace UI
    {
        using Client;
        using Server;
        using TextBuilderMembers = MyTuple<
            MyTuple<Func<int, int, object>, Func<int>>, // GetLineMember, GetLineCount
            Func<Vector2I, int, object>, // GetCharMember
            Func<object, int, object>, // GetOrSetMember
            Action<IList<RichStringMembers>, Vector2I>, // Insert
            Action<RichStringMembers, Vector2I>, // Insert
            Action // Clear
        >;

        namespace Rendering
        {
            using Client;
            using Server;
            public abstract partial class TextBuilder : ITextBuilder
            {
                public IRichChar this[Vector2I index] => lines[index.X][index.Y];

                /// <summary>
                /// Gets the line at the specified index.
                /// </summary>
                public ILine this[int index] => lines[index];

                /// <summary>
                /// Number of lines in the text.
                /// </summary>
                public int Count => lines.Count;

                /// <summary>
                /// Base text size. Compounds text scaling specified by <see cref="GlyphFormat"/>ting.
                /// </summary>
                public virtual float Scale { get { return formatter.Scale; } set { formatter.Scale = value; } }

                /// <summary>
                /// Default text format. Applied to strings added without any other formatting specified.
                /// </summary>
                public GlyphFormat Format { get; set; }

                /// <summary>
                /// Gets or sets the maximum line width before text will wrap to the next line. Word wrapping must be enabled for
                /// this to apply.
                /// </summary>
                public float LineWrapWidth { get { return formatter.MaxLineWidth; } set { SetWrapWidth(value); } }

                /// <summary>
                /// Determines whether or not word wrapping is enabled.
                /// </summary>
                public bool WordWrapping { get; }

                protected readonly List<Line> lines;
                private readonly FormattedTextBase formatter;
                private readonly WrappedText wrappedText;

                public TextBuilder(bool wordWrapping, int capacity)
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

                public IEnumerator<ILine> GetEnumerator() =>
                    lines.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() =>
                    GetEnumerator();

                protected virtual object GetOrSetMember(object data, int memberEnum)
                {
                    TextBuilderAccessors member = (TextBuilderAccessors)memberEnum;

                    if (member == TextBuilderAccessors.LineWrapWidth)
                    {
                        if (data == null)
                            return LineWrapWidth;
                        else
                            LineWrapWidth = (float)data;
                    }
                    else if (member == TextBuilderAccessors.WordWrapping)
                    {
                        return WordWrapping;
                    }
                    else if (member == TextBuilderAccessors.GetRange)
                    {
                        var range = (MyTuple<Vector2I, Vector2I>)data;
                        return GetTextRangeData(range.Item1, range.Item2);
                    }
                    else if (member == TextBuilderAccessors.SetFormatting)
                    {
                        var input = (MyTuple<Vector2I, Vector2I, GlyphFormatMembers>)data;
                        SetFormattingData(input.Item1, input.Item2, input.Item3);
                    }
                    else if (member == TextBuilderAccessors.RemoveRange)
                    {
                        var range = (MyTuple<Vector2I, Vector2I>)data;
                        RemoveRange(range.Item1, range.Item2);
                    }

                    return null;
                }

                protected object GetLineMember(int data, int memberEnum)
                {
                    LineAccessors member = (LineAccessors)memberEnum;

                    if (member == LineAccessors.Count)
                        return lines[data].Count;
                    else if (member == LineAccessors.Size)
                        return lines[data].Size;

                    return null;
                }

                protected object GetRichCharMember(Vector2I index, int memberEnum)
                {
                    RichCharAccessors member = (RichCharAccessors)memberEnum;

                    if (member == RichCharAccessors.Ch)
                        return this[index].Ch;
                    else if (member == RichCharAccessors.Format)
                        return this[index].Format.data;
                    else if (member == RichCharAccessors.Offset)
                        return this[index].Offset;
                    else if (member == RichCharAccessors.Size)
                        return this[index].Size;

                    return null;
                }

                protected virtual void AfterTextUpdate()
                { }

                protected void SetWrapWidth(float width)
                {
                    if (WordWrapping)
                    {
                        wrappedText.SetWrapWidth(width);
                        //AfterTextUpdate();
                    }
                }

                /// <summary>
                /// Clears current text and appends the text given.
                /// </summary>
                public void SetText(RichString text)
                {
                    Clear();
                    Append(text);
                }

                /// <summary>
                /// Clears current text and appends the text given.
                /// </summary>
                public void SetText(RichText text)
                {
                    Clear();
                    Append(text);
                }

                /// <summary>
                /// Clears current text and appends the text given.
                /// </summary>
                public void SetText(string text)
                {
                    Clear();
                    Append(text);
                }

                /// <summary>
                /// Appends the given <see cref="string"/> to the end of the text using the default <see cref="GlyphFormat"/>.
                /// </summary>
                public void Append(string text)
                {
                    formatter.Append(new RichStringMembers(new StringBuilder(text), Format.data));
                    AfterTextUpdate();
                }

                /// <summary>
                /// Appends the given text to the end of the text using the <see cref="GlyphFormat"/>ting specified in the <see cref="RichString"/>.
                /// </summary>
                public void Append(RichString text) =>
                    AppendData(text.GetApiData());

                protected void AppendData(RichStringMembers text)
                {
                    formatter.Append(text);
                    AfterTextUpdate();
                }

                /// <summary>
                /// Appends the given text to the end of the text using the <see cref="GlyphFormat"/>ting specified in the <see cref="RichText"/>.
                /// </summary>
                public void Append(RichText text) =>
                    AppendData(text.GetApiData());

                protected void AppendData(IList<RichStringMembers> text)
                {
                    formatter.Append(text);
                    AfterTextUpdate();
                }

                /// <summary>
                /// Inserts the given text to the end of the text at the specified starting index using the <see cref="GlyphFormat"/>ting specified in the <see cref="RichText"/>.
                /// </summary>
                public void Insert(RichText text, Vector2I start) =>
                    InsertData(text.GetApiData(), start);

                protected void InsertData(IList<RichStringMembers> text, Vector2I start)
                {
                    formatter.Insert(text, start);
                    AfterTextUpdate();
                }

                /// <summary>
                /// Inserts the given text to the end of the text at the specified starting index using the <see cref="GlyphFormat"/>ting specified in the <see cref="RichString"/>.
                /// </summary>
                public void Insert(RichString text, Vector2I start) =>
                    InsertData(text.GetApiData(), start);

                public void InsertData(RichStringMembers text, Vector2I start)
                {
                    formatter.Insert(text, start);
                    AfterTextUpdate();
                }

                /// <summary>
                /// Inserts the given <see cref="string"/> at the given starting index using the default <see cref="GlyphFormat"/>.
                /// </summary>
                public void Insert(string text, Vector2I start)
                {
                    formatter.Insert(new RichStringMembers(new StringBuilder(text), Format.data), start);
                    AfterTextUpdate();
                }

                /// <summary>
                /// Changes the formatting for the whole text to the given format.
                /// </summary>
                public void SetFormatting(GlyphFormat format)
                {
                    if (lines.Count > 0 && lines[lines.Count - 1].Count > 0)
                        SetFormattingData(Vector2I.Zero, new Vector2I(lines.Count - 1, lines[lines.Count - 1].Count - 1), format.data);
                }

                /// <summary>
                /// Changes the formatting for the text within the given range to the given format.
                /// </summary>
                public void SetFormatting(Vector2I start, Vector2I end, GlyphFormat format) =>
                    SetFormattingData(start, end, format.data);

                protected void SetFormattingData(Vector2I start, Vector2I end, GlyphFormatMembers format)
                {
                    formatter.SetFormatting(start, end, new GlyphFormat(format));
                    AfterTextUpdate();
                }

                /// <summary>
                /// Returns the contents of the text as <see cref="RichText"/>.
                /// </summary>
                public RichText GetText()
                {
                    if (lines.Count > 0 && lines[lines.Count - 1].Count > 0)
                        return GetTextRange(Vector2I.Zero, new Vector2I(lines.Count - 1, lines[lines.Count - 1].Count - 1));
                    else
                        return new RichText();
                }

                /// <summary>
                /// Returns the specified range of characters from the text as <see cref="RichText"/>.
                /// </summary>
                public RichText GetTextRange(Vector2I start, Vector2I end) =>
                    new RichText(GetTextRangeData(start, end));

                public List<RichStringMembers> GetTextRangeData(Vector2I start, Vector2I end)
                {
                    List<RichStringMembers> text = new List<RichStringMembers>();

                    if (end.X > start.X)
                    {
                        lines[start.X].GetRangeString(text, start.Y, lines[start.X].Count - 1);

                        for (int line = start.X + 1; line <= end.X - 1; line++)
                        {
                            for (int ch = 0; ch < lines[line].Count; ch++)
                            {
                                StringBuilder richString = new StringBuilder();
                                GlyphFormat format = lines[line][ch].Format;
                                ch--;

                                do
                                {
                                    ch++;
                                    richString.Append(lines[line][ch].Ch);
                                }
                                while (ch + 1 < lines[line].Count && format.Equals(lines[line][ch + 1].Format));

                                text.Add(new RichStringMembers(richString, format.data));
                            }
                        }

                        lines[end.X].GetRangeString(text, 0, end.Y);
                    }
                    else
                        lines[start.X].GetRangeString(text, start.Y, end.Y);

                    return text;
                }

                public void RemoveAt(Vector2I index) =>
                    RemoveRange(index, index);

                /// <summary>
                /// Removes all text within the specified range.
                /// </summary>
                public void RemoveRange(Vector2I start, Vector2I end)
                {
                    formatter.RemoveRange(start, end);
                    AfterTextUpdate();
                }

                /// <summary>
                /// Clears all existing text.
                /// </summary>
                public void Clear()
                {
                    formatter.Clear();
                }

                public TextBuilderMembers GetApiData()
                {
                    return new TextBuilderMembers()
                    {
                        Item1 = new MyTuple<Func<int, int, object>, Func<int>>(GetLineMember, () => lines.Count),
                        Item2 = GetRichCharMember,
                        Item3 = GetOrSetMember,
                        Item4 = InsertData,
                        Item5 = InsertData,
                        Item6 = Clear
                    };
                }

                protected class Line : IReadOnlyCollection<RichChar>, ILine
                {
                    IRichChar IIndexedCollection<IRichChar>.this[int index] => chars[index];
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

                    public List<RichStringMembers> GetString()
                    {
                        List<RichStringMembers> text = new List<RichStringMembers>();
                        GetRangeString(text, 0, Count - 1);

                        return text;
                    }

                    public void GetRangeString(List<RichStringMembers> text, int start, int end)
                    {
                        for (int ch = start; ch <= end; ch++)
                        {
                            StringBuilder richString = new StringBuilder();
                            GlyphFormat format = chars[ch].Format;
                            ch--;

                            do
                            {
                                ch++;
                                richString.Append(chars[ch].Ch);
                            }
                            while (ch + 1 <= end && format.Equals(chars[ch + 1].Format));

                            text.Add(new RichStringMembers(richString, format.data));
                        }
                    }

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

                        for (int n = 0; n < chars.Count; n++)
                        {
                            if (chars[n].Size.Y > size.Y)
                                size.Y = this[n].Size.Y;

                            size.X += chars[n].Size.X;
                        }
                    }

                    public override string ToString()
                    {
                        StringBuilder sb = new StringBuilder(Count);

                        for (int n = 0; n < chars.Count; n++)
                            sb.Append(chars[n].Ch);

                        return sb.ToString();
                    }
                }

                /// <summary>
                /// Contains the information needed to render an individual <see cref="Glyph"/> with a given
                /// <see cref="GlyphFormat"/>.
                /// </summary>
                protected class RichChar : IRichChar
                {
                    public char Ch { get; }
                    public bool IsSeparator => (Ch == ' ' || Ch == '-' || Ch == '_');
                    public bool IsLineBreak => Ch == '\n';
                    public Glyph Glyph { get; private set; }
                    public MatBoard GlyphBoard { get; private set; }
                    public Vector2 Size { get; private set; }
                    public Vector2 Offset => GlyphBoard.offset;
                    public GlyphFormat Format { get; private set; }

                    public RichChar(char ch, GlyphFormat formatting, float scale)
                    {
                        Ch = ch;
                        GlyphBoard = new MatBoard() { MatAlignment = MaterialAlignment.FitHorizontal };
                        SetFormatting(formatting, scale);
                    }

                    public bool IsWordBreak(RichChar right) =>
                        (IsSeparator && !right.IsSeparator);

                    public void SetFormatting(GlyphFormat format, float scale)
                    {
                        Vector2I index = format.StyleIndex;
                        IFontStyle fontStyle = FontManager.Fonts[index.X][index.Y];

                        scale *= format.TextSize * fontStyle.FontScale;
                        Format = format;
                        GlyphBoard.Color = format.Color;
                        GlyphBoard.MatScale = scale;
                        GlyphBoard.MatOffset *= scale;

                        if (IsLineBreak)
                        {
                            Glyph = fontStyle[' '];
                            Size = new Vector2(0f, fontStyle.Height) * scale;
                        }
                        else
                        {
                            Glyph = fontStyle[Ch];
                            Size = new Vector2(Glyph.advanceWidth, fontStyle.Height) * scale;
                        }

                        GlyphBoard.Material = Glyph.material;
                        GlyphBoard.Size = Glyph.material.size * scale;
                    }
                }
            }
        }
    }
}