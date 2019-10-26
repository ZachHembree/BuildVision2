using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, VRageMath.Color, float>;

namespace DarkHelmet
{
    using Vec2Prop = MyTuple<Func<Vector2>, Action<Vector2>>;
    using FloatProp = MyTuple<Func<float>, Action<float>>;
    using RichCharMembers = MyTuple<char, GlyphFormatMembers>;
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    namespace UI
    {
        using DarkHelmet.UI;
        using DarkHelmet.UI.Rendering;
        using LineMembers = MyTuple<
            Func<int, RichCharMembers>, // GetChar
            Func<int> // Count
        >;

        namespace Rendering
        {
            using TextBoardMembers = MyTuple<
                MyTuple<Func<int, LineMembers>, Func<int>>, // GetLine, GetCount
                FloatProp, // Scale
                Func<Vector2>, // TextSize
                Vec2Prop, // MaxSize
                FloatProp, // MaxLineWidth
                MyTuple<
                    Func<bool>, // WordWrapping
                    Action<Vector2>, // Draw
                    Func<Vector2I, Vector2I, List<RichStringMembers>>, // GetRange
                    Action<Vector2I, Vector2I, GlyphFormatMembers>, // SetFormatting
                    Action<IList<RichStringMembers>, Vector2I>, // Insert
                    MyTuple<
                        Action<RichStringMembers, Vector2I>, // Insert
                        Action<Vector2I, Vector2I>, // RemoveRange
                        Action // Clear
                    >
                >
            >;

            public class RichCharData : IRichChar
            {
                public char Ch { get; }
                public GlyphFormat Format { get; }

                public RichCharData(RichCharMembers members)
                {
                    Ch = members.Item1;
                    Format = new GlyphFormat(members.Item2);
                }

                public RichCharMembers GetApiData() =>
                    new RichCharMembers(Ch, Format.data);
            }

            public class LineData : ILine
            {
                public IRichChar this[int index] => new RichCharData(GetCharFunc(index));
                public int Count => GetCountFunc();

                private readonly Func<int, RichCharMembers> GetCharFunc;
                private readonly Func<int> GetCountFunc;

                public LineData(LineMembers members)
                {
                    GetCharFunc = members.Item1;
                    GetCountFunc = members.Item2;
                }
            }

            public class TextBoard : ITextBoard
            {
                public ILine this[int index] => new LineData(GetLine(index));
                public int Count => GetLineCount();

                public GlyphFormat Format { get; set; }
                public float Scale { get { return GetScale(); } set { SetScale(value); } }
                public Vector2 TextSize => GetTextSizeFunc();
                public Vector2 MaxSize { get { return GetMaxSizeFunc(); } set { SetMaxSizeAction(value); } }
                public bool WordWrapping => GetWordWrapping();
                public float MaxLineWidth => GetMaxWidth();

                private readonly Func<int, LineMembers> GetLine;
                private readonly Func<int> GetLineCount;
                private readonly Func<float> GetScale;
                private readonly Action<float> SetScale;
                private readonly Func<Vector2> GetTextSizeFunc;
                private readonly Func<Vector2> GetMaxSizeFunc;
                private readonly Action<Vector2> SetMaxSizeAction;

                private readonly Func<float> GetMaxWidth;
                private readonly Action<float> SetMaxWidth;
                private readonly Func<bool> GetWordWrapping;
                private readonly Action<Vector2> DrawAction;

                private readonly Func<Vector2I, Vector2I, List<RichStringMembers>> GetRangeFunc;
                private readonly Action<Vector2I, Vector2I, GlyphFormatMembers> SetFormatAction;
                private readonly Action<IList<RichStringMembers>, Vector2I> InsertText;
                private readonly Action<RichStringMembers, Vector2I> InsertString;
                private readonly Action<Vector2I, Vector2I> RemoveRangeAction;
                private readonly Action ClearAction;

                public TextBoard(bool wordWrapping)
                {
                    TextBoardMembers members = HudMain.GetTextBoardData(wordWrapping);
                    Format = GlyphFormat.Default;

                    GetLine = members.Item1.Item1;
                    GetLineCount = members.Item1.Item2;
                    GetScale = members.Item2.Item1;
                    SetScale = members.Item2.Item2;
                    GetTextSizeFunc = members.Item3;
                    GetMaxSizeFunc = members.Item4.Item1;
                    SetMaxSizeAction = members.Item4.Item2;

                    GetMaxWidth = members.Item5.Item1;
                    SetMaxWidth = members.Item5.Item2;

                    var data2 = members.Item6;
                    GetWordWrapping = data2.Item1;
                    DrawAction = data2.Item2;
                    GetRangeFunc = data2.Item3;
                    SetFormatAction = data2.Item4;
                    InsertText = data2.Item5;

                    var data3 = data2.Item6;
                    InsertString = data3.Item1;
                    RemoveRangeAction = data3.Item2;
                    ClearAction = data3.Item3;
                }

                public void Draw(Vector2 origin) =>
                    DrawAction(origin);

                public void SetLineWrapWidth(float width) =>
                    SetMaxWidth(width);

                public void SetText(string text)
                {
                    ClearAction();
                    InsertString(new RichStringMembers(new StringBuilder(text), Format.data), GetLastIndex());
                }

                public void SetText(RichText text)
                {
                    ClearAction();
                    Insert(text, GetLastIndex());
                }

                public void SetText(RichString text)
                {
                    ClearAction();
                    Insert(text, GetLastIndex());
                }

                public void Append(string text) =>
                    InsertString(new RichStringMembers(new StringBuilder(text), Format.data), GetLastIndex());

                public void Append(RichText text) =>
                    Insert(text, GetLastIndex());

                public void Append(RichString text) =>
                    Insert(text, GetLastIndex());

                private Vector2I GetLastIndex()
                {
                    Vector2I start = new Vector2I(Math.Max(0, Count - 1), 0);

                    if (Count > 0)
                        start.Y = Math.Max(0, this[start.X].Count);

                    return start;
                }

                public RichText GetText()
                {
                    if (Count > 0)
                        return GetTextRange(Vector2I.Zero, GetLastIndex());
                    else
                        return new RichText();
                }

                public RichText GetTextRange(Vector2I start, Vector2I end) =>
                    new RichText(GetRangeFunc(start, end));

                public void Insert(RichText text, Vector2I start) =>
                    InsertText(text.GetApiData(), start);

                public void Insert(RichString text, Vector2I start) =>
                    InsertString(text.GetApiData(), start);

                public void RemoveRange(Vector2I start, Vector2I end) =>
                    RemoveRangeAction(start, end);

                public void SetFormatting(Vector2I start, Vector2I end, GlyphFormat format) =>
                    SetFormatAction(start, end, format.data);

                public void Clear() =>
                    ClearAction();

                public TextBoardMembers GetApiData()
                {
                    return new TextBoardMembers()
                    {

                    };
                }
            }
        }
    }
}