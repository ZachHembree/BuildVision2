using System;
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
                /// If true, the text board will automatically resize to fit the text.
                /// </summary>
                public bool AutoResize { get; set; }

                /// <summary>
                /// If true, the text will be vertically aligned to the center of the text board.
                /// </summary>
                public bool VertCenterText { get; set; }

                private int startLine, endLine;
                private float columnOffset;
                private bool areOffsetsStale;
                private Vector2 fixedSize;

                public TextBoard(int capacity = 3) : base(capacity)
                {
                    Scale = 1f;
                    endLine = -1;
                    AutoResize = true;
                    VertCenterText = true;

                    Format = GlyphFormat.Black;
                    FixedSize = new Vector2(200f, 200f);
                }

                /// <summary>
                /// Calculates the minimum offset needed to ensure that the character at the specified index
                /// is within the visible range.
                /// </summary>
                public void MoveToChar(Vector2I index)
                {
                    if (!AutoResize)
                    {
                        if (BuilderMode != TextBuilderModes.Unlined)
                            startLine = GetFirstVisibleLine(index.X);
                        else
                            startLine = 0;

                        if (BuilderMode != TextBuilderModes.Wrapped)
                            columnOffset = GetCharRangeOffset(index);
                        else
                            columnOffset = 0f;

                        UpdateOffsets();
                    }
                }

                /// <summary>
                /// Finds the first line visible in the range that includes the given line index.
                /// </summary>
                private int GetFirstVisibleLine(int line)
                {
                    int newStart = startLine;

                    if (line > endLine)
                        newStart = GetStartFromEnd(line);
                    else if (line < startLine)
                        newStart = line;

                    return newStart;
                }

                /// <summary>
                /// Finds the first visible line given the index of the last visible line.
                /// </summary>
                private int GetStartFromEnd(int end)
                {
                    int start = end;
                    float height = 0f;

                    for (int line = end; line >= 0; line--)
                    {
                        if (height <= (FixedSize.Y - lines[line].Size.Y))
                        {
                            height += lines[line].Size.Y;
                            start = line;
                        }
                        else
                            break;
                    }

                    return start;
                }

                /// <summary>
                /// Calculates the horizontal offset needed to ensure that the character specified is within
                /// the visible range.
                /// </summary>
                private float GetCharRangeOffset(Vector2I index)
                {
                    Line line = lines[index.X];
                    float newOffset = columnOffset;

                    if (line.Count > 0)
                    {
                        if (index.Y != 0)
                        {
                            UpdateLineOffsets(index.X, 0f, true);

                            float dist = line[index.Y].Offset.X + (fixedSize.X - line[index.Y].Size.X) / 2f;

                            if (dist > (newOffset + fixedSize.X + line[index.Y].Size.X / 2f))
                            {
                                newOffset = dist - fixedSize.X;
                            }
                            else if (dist - line[index.Y].Size.X / 2f < newOffset)
                            {
                                newOffset = dist - line[index.Y].Size.X;
                            }
                        }
                        else
                            newOffset = 0f;
                    }

                    return newOffset;
                }

                /// <summary>
                /// Returns the index of the character closest to the given offset.
                /// </summary>
                public Vector2I GetCharAtOffset(Vector2 charOffset)
                {
                    int line = 0, ch = 0;

                    if (lines.Count > 0)
                    {
                        line = GetLineAt(charOffset.Y);
                        ch = GetCharAt(line, charOffset.X);
                    }

                    return new Vector2I(line, ch);
                }

                /// <summary>
                /// Returns the index of the line closest to the given offset.
                /// </summary>
                private int GetLineAt(float offset)
                {
                    float height;
                    int line = startLine;

                    if (VertCenterText)
                        height = TextSize.Y / 2f;
                    else
                        height = Size.Y / 2f;

                    height -= lines[0].Size.Y;

                    for (int n = startLine; n <= endLine; n++)
                    {
                        line = n;

                        if (offset <= height && offset > (height - lines[n].Size.Y))
                            break;

                        height -= lines[n].Size.Y;
                    }

                    return line;
                }

                /// <summary>
                /// Returns the index of the character on the given line closest to the given offset.
                /// </summary>
                private int GetCharAt(int line, float offset)
                {
                    float last, next;
                    int ch = 0;

                    for (int n = 0; n < lines[line].Count; n++)
                    {
                        MatBoard glyphBoard = lines[line][n].GlyphBoard;

                        if (glyphBoard.offset != -Vector2.PositiveInfinity)
                        {
                            if (glyphBoard.offset != Vector2.PositiveInfinity)
                            {
                                ch = n;

                                if (n - 1 >= 0) // Make sure the index is in range
                                    last = lines[line][n - 1].Offset.X;
                                else
                                    last = float.MinValue;

                                if (n + 1 < lines[line].Count)
                                    next = lines[line][n + 1].Offset.X;
                                else
                                    next = float.MaxValue;

                                if (offset > last && offset < next)
                                    break;
                            }
                            else
                                break;
                        }
                    }

                    return ch;
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

                /// <summary>
                /// Updates the offsets for characters within the visible range of text and updates the
                /// current size of the text box.
                /// </summary>
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
                                UpdateLineOffsets(line, height);
                                height -= lines[line].Size.Y;
                            }
                        }
                    }
                }

                /// <summary>
                /// Calculates the current size of the text box. If AutoResize == true, then the
                /// size of the text box will not be limited to the FixedSize and it will be allowed 
                /// to resize freely.
                /// </summary>
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

                /// <summary>
                /// Updates the position of each character in the given line.
                /// </summary>
                private void UpdateLineOffsets(int line, float height, bool ignoreBounds = false)
                {
                    RichChar leftChar = null, rightChar;
                    float width = -columnOffset, xAlign = GetLineAlignment(lines[line]);

                    height -= GetBaseline(line);

                    for (int ch = 0; ch < lines[line].Count; ch++)
                    {
                        rightChar = lines[line][ch];

                        if (width < 0 && !ignoreBounds) // Any characters before this point will not be drawn
                        {
                            rightChar.GlyphBoard.offset = -Vector2.PositiveInfinity;
                            width += rightChar.Size.X;
                            leftChar = rightChar;
                        }
                        else if (AutoResize || ignoreBounds || width + rightChar.Size.X <= fixedSize.X)
                        {
                            width = UpdateCharOffset(rightChar, leftChar, new Vector2(width, height), xAlign);
                            leftChar = rightChar;
                        }
                        else if (!ignoreBounds) // Any characters after this point will not be drawn
                        {
                            rightChar.GlyphBoard.offset = Vector2.PositiveInfinity;
                            break;
                        }
                    }
                }

                /// <summary>
                /// Returns the offset needed for the given line to ensure the given line matches its <see cref="TextAlignment"/>
                /// </summary>
                private float GetLineAlignment(Line line)
                {
                    float offset = 0f, lineWidth = Math.Min(line.Size.X, Size.X);
                    TextAlignment alignment = line[0].Format.Alignment; // the first character determines alignment

                    if (alignment == TextAlignment.Left)
                        offset = -Size.X / 2f;
                    else if (alignment == TextAlignment.Center)
                        offset = -lineWidth / 2f;
                    else if (alignment == TextAlignment.Right)
                        offset = (Size.X / 2f) - lineWidth;

                    return offset;
                }

                /// <summary>
                /// Calculates the baseline to be shared by each character in the line.
                /// </summary>
                private float GetBaseline(int line)
                {
                    float baseline = 0f;

                    for (int ch = 0; ch < lines[line].Count; ch++)
                    {
                        if (lines[line][ch].Size.Y == lines[line].Size.Y)
                        {
                            GlyphFormat format = lines[line][ch].Format;
                            IFontStyle fontStyle = FontManager.Fonts[format.StyleIndex.X][format.StyleIndex.Y];

                            baseline = (fontStyle.BaseLine - (fontStyle.Height - fontStyle.BaseLine) / 2f) * (format.TextSize * fontStyle.FontScale * Scale);
                        }
                    }

                    return baseline.Round();
                }

                /// <summary>
                /// Updates the position of the right character.
                /// </summary>
                private float UpdateCharOffset(RichChar rightChar, RichChar leftChar, Vector2 pos, float xAlign)
                {
                    GlyphFormat format = rightChar.Format;
                    IFontStyle fontStyle = FontManager.Fonts[format.StyleIndex.X][format.StyleIndex.Y];
                    float scale = format.TextSize * fontStyle.FontScale * Scale;

                    if (leftChar != null && CanUseKernings(leftChar, rightChar))
                        pos.X += fontStyle.GetKerningAdjustment(leftChar.Ch, rightChar.Ch) * scale;

                    rightChar.GlyphBoard.offset = new Vector2()
                    {
                        X = pos.X + rightChar.GlyphBoard.Size.X / 2f + (rightChar.Glyph.leftSideBearing * scale) + xAlign,
                        Y = pos.Y - (rightChar.GlyphBoard.Size.Y / 2f) + (fontStyle.BaseLine * scale)
                    };

                    pos.X += rightChar.Size.X;
                    return pos.X;
                }

                /// <summary>
                /// Determines whether the formatting of the characters given allows for the use of kerning pairs.
                /// </summary>
                private bool CanUseKernings(RichChar left, RichChar right) =>
                     left.Format.StyleIndex == right.Format.StyleIndex && left.Format.TextSize == right.Format.TextSize;

                /// <summary>
                /// General purpose method used to allow the API to access various members not included in this type's
                /// associated tuple.
                /// </summary>
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

                /// <summary>
                /// Returns a collection of members needed to access this object via the HUD API as a tuple.
                /// </summary>
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