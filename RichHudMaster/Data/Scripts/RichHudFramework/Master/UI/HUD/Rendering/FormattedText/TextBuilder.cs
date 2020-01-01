using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace RichHudFramework
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
                /// Determines the formatting mode of the text.
                /// </summary>
                public TextBuilderModes BuilderMode
                {
                    get { return builderMode; }
                    set
                    {
                        if (value != builderMode)
                        {
                            if (value == TextBuilderModes.Unlined)
                            {
                                wrappedText = null;
                                formatter = new UnlinedText(lines);
                            }
                            else if (value == TextBuilderModes.Lined)
                            {
                                wrappedText = null;
                                formatter = new LinedText(lines);
                            }
                            else if (value == TextBuilderModes.Wrapped)
                            {
                                wrappedText = new WrappedText(lines);
                                formatter = wrappedText;
                            }

                            builderMode = value;
                        }
                    }
                }

                protected readonly List<Line> lines;
                protected TextBuilderModes builderMode;

                private FormattedTextBase formatter;
                private WrappedText wrappedText;

                public TextBuilder(int capacity)
                {
                    lines = new List<Line>(capacity);
                    BuilderMode = TextBuilderModes.Lined;
                    Format = GlyphFormat.White;
                    Scale = 1f;
                }

                public IEnumerator<ILine> GetEnumerator() =>
                    lines.GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() =>
                    GetEnumerator();

                protected virtual object GetOrSetMember(object data, int memberEnum)
                {
                    switch ((TextBuilderAccessors)memberEnum)
                    {
                        case TextBuilderAccessors.LineWrapWidth:
                            {
                                if (data == null)
                                    return LineWrapWidth;
                                else
                                    LineWrapWidth = (float)data;

                                break;
                            }
                        case TextBuilderAccessors.BuilderMode:
                            {
                                if (data == null)
                                    return BuilderMode;
                                else
                                    BuilderMode = (TextBuilderModes)data;

                                break;
                            }
                        case TextBuilderAccessors.GetRange:
                            {
                                var range = (MyTuple<Vector2I, Vector2I>)data;
                                return GetTextRangeData(range.Item1, range.Item2);
                            }
                        case TextBuilderAccessors.SetFormatting:
                            {
                                var input = (MyTuple<Vector2I, Vector2I, GlyphFormatMembers>)data;

                                SetFormattingData(input.Item1, input.Item2, input.Item3);
                                break;
                            }
                        case TextBuilderAccessors.RemoveRange:
                            {
                                var range = (MyTuple<Vector2I, Vector2I>)data;

                                RemoveRange(range.Item1, range.Item2);
                                break;
                            }
                        case TextBuilderAccessors.Format:
                            {
                                if (data == null)
                                    return Format.data;
                                else
                                    Format = new GlyphFormat((GlyphFormatMembers)data);

                                break;
                            }
                    }

                    return null;
                }

                protected object GetLineMember(int index, int memberEnum)
                {
                    switch ((LineAccessors)memberEnum)
                    {
                        case LineAccessors.Count:
                            return lines[index].Count;
                        case LineAccessors.Size:
                            return lines[index].Size;
                    }

                    return null;
                }

                protected object GetRichCharMember(Vector2I index, int memberEnum)
                {
                    switch ((RichCharAccessors)memberEnum)
                    {
                        case RichCharAccessors.Ch:
                            return this[index].Ch;
                        case RichCharAccessors.Format:
                            return this[index].Format.data;
                        case RichCharAccessors.Offset:
                            return this[index].Offset;
                        case RichCharAccessors.Size:
                            return this[index].Size;
                    }

                    return null;
                }

                protected virtual void AfterTextUpdate()
                { }

                protected void SetWrapWidth(float width)
                {
                    if (BuilderMode == TextBuilderModes.Wrapped)
                    {
                        wrappedText.SetWrapWidth(width);
                        AfterTextUpdate();
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
                public void Append(RichString text)
                {
                    if (text.format == GlyphFormat.Empty)
                        text.format = Format;

                    AppendData(text.GetApiData());
                }

                protected void AppendData(RichStringMembers text)
                {
                    if (text.Item2.Equals(GlyphFormat.Empty.data))
                        text.Item2 = Format.data;

                    formatter.Append(text);
                    AfterTextUpdate();
                }

                /// <summary>
                /// Appends the given text to the end of the text using the <see cref="GlyphFormat"/>ting specified in the <see cref="RichText"/>.
                /// </summary>
                public void Append(RichText text)
                {
                    for (int n = 0; n < text.Count; n++)
                    {
                        if (text[n].format == null)
                            text[n].format = Format;
                    }

                    AppendData(text.GetApiData());
                }

                protected void AppendData(IList<RichStringMembers> text)
                {
                    for (int n = 0; n < text.Count; n++)
                    {
                        if (text[n].Item2.Equals(GlyphFormat.Empty.data))
                            text[n] = new RichStringMembers(text[n].Item1, Format.data);
                    }

                    formatter.Append(text);
                    AfterTextUpdate();
                }

                /// <summary>
                /// Inserts the given text to the end of the text at the specified starting index using the <see cref="GlyphFormat"/>ting specified in the <see cref="RichText"/>.
                /// </summary>
                public void Insert(RichText text, Vector2I start)
                {
                    for (int n = 0; n < text.Count; n++)
                    {
                        if (text[n].format == GlyphFormat.Empty)
                            text[n].format = Format;
                    }

                    InsertData(text.GetApiData(), start);
                }

                public void InsertData(IList<RichStringMembers> text, Vector2I start)
                {
                    for (int n = 0; n < text.Count; n++)
                    {
                        if (text[n].Item2.Equals(GlyphFormat.Empty.data))
                            text[n] = new RichStringMembers(text[n].Item1, Format.data);
                    }

                    formatter.Insert(text, start);
                    AfterTextUpdate();
                }

                /// <summary>
                /// Inserts the given text to the end of the text at the specified starting index using the <see cref="GlyphFormat"/>ting specified in the <see cref="RichString"/>.
                /// </summary>
                public void Insert(RichString text, Vector2I start)
                {
                    if (text.format == GlyphFormat.Empty)
                        text.format = Format;

                    InsertData(text.GetApiData(), start);
                }

                public void InsertData(RichStringMembers text, Vector2I start)
                {
                    if (text.Item2.Equals(GlyphFormat.Empty.data))
                        text.Item2 = Format.data;

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
                /// Changes the formatting of the entire text to the given format.
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
                    Format = new GlyphFormat(format);
                    formatter.SetFormatting(start, end, Format);
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
                    if (lines.Count > 0)
                    {
                        formatter.Clear();
                        AfterTextUpdate();
                    }
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

                    public void AddRange(IList<RichChar> newChars) =>
                        chars.AddRange(newChars);

                    public void InsertRange(int index, IList<RichChar> newChars) =>
                        chars.InsertRange(index, newChars);

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
                            {
                                size.Y = chars[n].Size.Y;
                            }

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

                        GlyphBoard.Color = format.Color;
                        GlyphBoard.Material = Glyph.material;
                        GlyphBoard.Size = Glyph.material.size * scale;
                    }
                }
            }
        }
    }
}