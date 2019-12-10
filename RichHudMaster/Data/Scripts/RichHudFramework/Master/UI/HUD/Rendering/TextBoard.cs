using System;
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
        using TextBuilderMembers = MyTuple<
            MyTuple<Func<int, int, object>, Func<int>>, // GetLineMember, GetLineCount
            Func<Vector2I, int, object>, // GetCharMember
            Func<object, int, object>, // GetOrSetMember
            Action<IList<RichStringMembers>, Vector2I>, // Insert
            Action<RichStringMembers, Vector2I>, // Insert
            Action // Clear
        >;

        namespace Rendering.Server
        {
            using TextBoardMembers = MyTuple<
                TextBuilderMembers,
                FloatProp, // Scale
                Func<Vector2>, // Size
                Func<Vector2>, // TextSize
                Vec2Prop, // FixedSize
                Action<Vector2> // Draw 
            >;

            public class TextBoard : TextBuilder, ITextBoard
            {
                public event Action OnTextChanged;

                /// <summary>
                /// Text size
                /// </summary>
                public override float Scale
                {
                    get { return base.Scale; }
                    set
                    {
                        Size *= (value / base.Scale);
                        TextSize *= (value / base.Scale);
                        FixedSize *= (value / base.Scale);
                        columnOffset *= (value / base.Scale);
                        base.Scale = value;
                    }
                }

                /// <summary>
                /// Size of the text box as rendered
                /// </summary>
                public Vector2 Size { get; protected set; }

                /// <summary>
                /// Full text size beginning with the StartLine
                /// </summary>
                public Vector2 TextSize { get; protected set; }

                /// <summary>
                /// Size of the text box when AutoResize is set to false. Does nothing otherwise.
                /// </summary>
                public Vector2 FixedSize
                {
                    get { return fixedSize; }
                    set
                    {
                        if (fixedSize != value)
                        {
                            fixedSize = value;
                            LineWrapWidth = fixedSize.X;
                            AfterTextUpdate();
                        }
                    }
                }

                /// <summary>
                /// If true, the text box will automatically resize to fit the text.
                /// </summary>
                public bool AutoResize { get; set; }
                public bool VertCenterText { get; set; }

                private int startLine, endLine;
                private float columnOffset;
                private bool areOffsetsStale;
                private Vector2 fixedSize;

                public TextBoard(bool wordWrapping, int capacity = 3) : base(wordWrapping, capacity)
                {
                    Scale = 1f;
                    endLine = -1;
                    AutoResize = true;
                    VertCenterText = true;

                    Format = GlyphFormat.Black;
                    FixedSize = new Vector2(500f, 500f);
                }

                public void MoveToChar(Vector2I index)
                {
                    if (!AutoResize)
                    {
                        int x = index.X, y = index.Y, newStart = startLine;

                        if (x > endLine)
                            newStart += (x - endLine);
                        else if (x < startLine)
                            newStart = x;

                        Line line = lines[x];
                        float newOffset = 0f, dist = 0f;

                        if (y > 0)
                        {
                            for (int ch = 0; ch <= y; ch++)
                                dist += line[ch].Size.X;

                            newOffset = dist - fixedSize.X;
                            dist -= line[y].Size.X;

                            if (newOffset < columnOffset)
                            {
                                newOffset = columnOffset;

                                for (int ch = y; (ch >= 0 && dist < newOffset); ch++)
                                    newOffset -= line[ch].Size.X;
                            }
                        }

                        if (newStart != startLine || newOffset != columnOffset)
                        {
                            startLine = newStart;
                            columnOffset = newOffset;
                            UpdateOffsets();
                        }
                    }
                }

                /// <summary>
                /// Returns the index of the character at the given offset.
                /// </summary>
                public Vector2I GetCharAtOffset(Vector2 charOffset)
                {
                    int line = 0, ch = 0;

                    if (lines.Count > 0)
                    {
                        Vector2 last = new Vector2(), next = new Vector2();

                        for (int n = startLine; n <= endLine; n++)
                        {
                            if (lines[line].Count > 0)
                            {
                                if (n - 1 >= startLine && lines[n - 1].Count > 0)
                                    last.Y = lines[n - 1][0].GlyphBoard.offset.Y + lines[n].Size.Y / 3f;
                                else
                                    last.Y = float.MaxValue;

                                if (n + 1 <= endLine && lines[n + 1].Count > 0)
                                    next.Y = lines[n + 1][0].GlyphBoard.offset.Y + lines[n].Size.Y / 3f;
                                else
                                    next.Y = float.MinValue;

                                if (next.Y < charOffset.Y && charOffset.Y < last.Y)
                                {
                                    line = n;
                                    break;
                                }
                            }
                        }

                        for (int n = 0; n < lines[line].Count; n++)
                        {
                            MatBoard glyphBoard = lines[line][n].GlyphBoard;

                            if (glyphBoard.offset != -Vector2.PositiveInfinity)
                            {
                                if (glyphBoard.offset != Vector2.PositiveInfinity)
                                {
                                    if (n - 1 >= 0)
                                        last.X = lines[line][n - 1].GlyphBoard.offset.X;
                                    else
                                        last.X = float.MinValue;

                                    if (n + 1 < lines[line].Count)
                                        next.X = lines[line][n + 1].GlyphBoard.offset.X;
                                    else
                                        next.X = float.MaxValue;

                                    if (last.X < charOffset.X && charOffset.X < next.X)
                                    {
                                        ch = n;
                                        break;
                                    }
                                }
                                else
                                    break;
                            }
                        }
                    }

                    return new Vector2I(line, ch);
                }

                /// <summary>
                /// Returns the offset for the character at the given index.
                /// </summary>
                public Vector2 GetCharOffset(Vector2I index)
                {
                    if (lines.Count > 0 && lines[index.X].Count > 0)
                    {
                        MatBoard glyphBoard = lines[index.X][index.Y].GlyphBoard;
                        return glyphBoard.offset;
                    }
                    else
                    {
                        IFontStyle fontStyle = FontManager.Fonts[Format.StyleIndex.X][Format.StyleIndex.Y];
                        float width, height, fontHeight = fontStyle.Height * (Format.TextSize * Scale) * .7f;
                        TextAlignment alignment = Format.Alignment;

                        if (VertCenterText)
                            height = TextSize.Y / 2f - fontHeight / 2f;
                        else
                            height = Size.Y / 2f - fontHeight / 2f;

                        if (alignment == TextAlignment.Left)
                            width = -Size.X / 2f;
                        else if (alignment == TextAlignment.Right)
                            width = Size.X / 2f;
                        else
                            width = 0f;

                        return new Vector2(width, height);
                    }
                }

                public void Draw(Vector2 origin)
                {
                    if (areOffsetsStale)
                    {
                        UpdateOffsets();
                        areOffsetsStale = false;
                    }

                    for (int line = startLine; line <= endLine && line < lines.Count; line++)
                    {
                        for (int ch = 0; ch < lines[line].Count; ch++)
                        {
                            MatBoard glyphBoard = lines[line][ch].GlyphBoard;

                            if (glyphBoard.offset != -Vector2.PositiveInfinity)
                            {
                                if (glyphBoard.offset != Vector2.PositiveInfinity)
                                    glyphBoard.Draw(origin);
                                else
                                    break;
                            }
                        }
                    }
                }

                protected override void AfterTextUpdate()
                {
                    areOffsetsStale = true;
                    OnTextChanged?.Invoke();
                }

                private void UpdateOffsets()
                {
                    TextSize = GetSize();

                    if (AutoResize)
                        Size = TextSize;
                    else
                        Size = fixedSize;

                    if (lines.Count > 0)
                    {
                        float height;

                        if (VertCenterText)
                            height = TextSize.Y / 2f;
                        else
                            height = Size.Y / 2f;

                        for (int line = startLine; line <= endLine; line++)
                        {
                            if (lines[line].Count > 0)
                            {
                                height -= lines[line].Size.Y / 2f;
                                UpdateLineOffsets(line, height);
                                height -= lines[line].Size.Y / 2f;
                            }
                        }
                    }
                }

                private Vector2 GetSize()
                {
                    float width = 0f, height = 0f;

                    for (int line = startLine; line < lines.Count; line++)
                    {
                        if (AutoResize || height <= (FixedSize.Y - lines[line].Size.Y))
                        {
                            if (lines[line].Size.X > width)
                                width = lines[line].Size.X;

                            height += lines[line].Size.Y;
                            endLine = line;
                        }
                        else
                            break;
                    }

                    return new Vector2(width, height);
                }

                private void UpdateLineOffsets(int line, float height)
                {
                    float width = -columnOffset, xAlign = GetLineAlignment(lines[line]);
                    RichChar leftChar = null, rightChar;

                    for (int ch = 0; ch < lines[line].Count; ch++)
                    {
                        rightChar = lines[line][ch];

                        if (width < 0) // Any characters before this point will not be drawn
                        {
                            rightChar.GlyphBoard.offset = -Vector2.PositiveInfinity;
                            width += rightChar.Size.X;
                            leftChar = rightChar;
                        }
                        else if (AutoResize || width + rightChar.Size.X <= fixedSize.X)
                        {
                            GlyphFormat format = rightChar.Format;
                            IFontStyle fontStyle = FontManager.Fonts[format.StyleIndex.X][format.StyleIndex.Y];
                            float scale = format.TextSize * fontStyle.FontScale * Scale;

                            if (leftChar != null && CanUseKernings(leftChar, rightChar))
                                width += fontStyle.GetKerningAdjustment(leftChar.Ch, rightChar.Ch) * scale;

                            rightChar.GlyphBoard.offset = new Vector2()
                            {
                                X = width + rightChar.GlyphBoard.Size.X / 2f + (rightChar.Glyph.leftSideBearing * scale) + xAlign,
                                Y = height + (lines[line].Size.Y - rightChar.Size.Y)
                            };

                            width += rightChar.Size.X;
                            leftChar = rightChar;
                        }
                        else // Any characters after this point will not be drawn
                        {
                            rightChar.GlyphBoard.offset = Vector2.PositiveInfinity;
                            break;
                        }
                    }
                }

                private float GetLineAlignment(Line line)
                {
                    float offset = 0f, lineWidth = Math.Min(line.Size.X, Size.X);
                    TextAlignment alignment = line[0].Format.Alignment;

                    if (alignment == TextAlignment.Left)
                        offset = -Size.X / 2f;
                    else if (alignment == TextAlignment.Center)
                        offset = -lineWidth / 2f;
                    else if (alignment == TextAlignment.Right)
                        offset = (Size.X / 2f) - lineWidth;

                    return offset;
                }

                private bool CanUseKernings(RichChar left, RichChar right) =>
                     left.Format.StyleIndex == right.Format.StyleIndex && left.Format.TextSize == right.Format.TextSize;

                protected override object GetOrSetMember(object data, int memberEnum)
                {
                    if (memberEnum <= 128)
                        return base.GetOrSetMember(data, memberEnum);
                    else
                    {
                        var member = (TextBoardAccessors)memberEnum;

                        if (member == TextBoardAccessors.AutoResize)
                        {
                            if (data == null)
                                return AutoResize;
                            else
                                AutoResize = (bool)data;
                        }
                        else if (member == TextBoardAccessors.VertAlign)
                        {
                            if (data == null)
                                return VertCenterText;
                            else
                                VertCenterText = (bool)data;
                        }
                        else if (member == TextBoardAccessors.MoveToChar)
                            MoveToChar((Vector2I)data);
                        else if (member == TextBoardAccessors.GetCharAtOffset)
                            return GetCharAtOffset((Vector2)data);

                        return null;
                    }
                }

                public new TextBoardMembers GetApiData()
                {
                    return new TextBoardMembers()
                    {
                        Item1 = base.GetApiData(),
                        Item2 = new FloatProp(() => Scale, x => Scale = x),
                        Item3 = () => Size,
                        Item4 = () => TextSize,
                        Item5 = new Vec2Prop(() => FixedSize, x => FixedSize = x),
                        Item6 = Draw
                    };
                }
            }
        }

        namespace Rendering.Client
        { }
    }
}