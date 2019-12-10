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
    using RangeData = MyTuple<Vector2I, Vector2I>;
    using RangeFormatData = MyTuple<Vector2I, Vector2I, GlyphFormatMembers>;

    namespace UI
    {
        using System.Collections;
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
            public abstract class TextBuilder : ITextBuilder
            {
                public IRichChar this[Vector2I index] => new RichCharData(this, index);
                public ILine this[int index] => new LineData(this, index);
                public int Count => GetCountFunc();
                public GlyphFormat Format { get; set; }
                public float LineWrapWidth
                {
                    get { return (float)GetOrSetMemberFunc(null, (int)TextBuilderAccessors.LineWrapWidth); }
                    set { GetOrSetMemberFunc(value, (int)TextBuilderAccessors.LineWrapWidth); }
                } 
                public bool WordWrapping => (bool)GetOrSetMemberFunc(null, (int)TextBuilderAccessors.WordWrapping);

                protected readonly Func<object, int, object> GetOrSetMemberFunc;
                private readonly Func<int, int, object> GetLineMemberFunc;
                private readonly Func<int> GetCountFunc;
                private readonly Func<Vector2I, int, object> GetCharMemberFunc;
                private readonly Action<IList<RichStringMembers>, Vector2I> InsertTextAction;
                private readonly Action<RichStringMembers, Vector2I> InsertStringAction;
                private readonly Action ClearAction;

                public TextBuilder(TextBuilderMembers data)
                {
                    GetLineMemberFunc = data.Item1.Item1;
                    GetCountFunc = data.Item1.Item2;

                    GetCharMemberFunc = data.Item2;
                    GetOrSetMemberFunc = data.Item3;
                    InsertTextAction = data.Item4;
                    InsertStringAction = data.Item5;
                    ClearAction = data.Item6;
                }

                public IEnumerator<ILine> GetEnumerator() =>
                    new CollectionDataEnumerator<ILine>(x => new LineData(this, x), GetCountFunc);

                IEnumerator IEnumerable.GetEnumerator() =>
                    GetEnumerator();

                public void SetText(string text)
                {
                    ClearAction();
                    InsertStringAction(new RichStringMembers(new StringBuilder(text), Format.data), GetLastIndex());
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
                    InsertStringAction(new RichStringMembers(new StringBuilder(text), Format.data), GetLastIndex());

                public void Append(RichText text) =>
                    Insert(text, GetLastIndex());

                public void Append(RichString text) =>
                    Insert(text, GetLastIndex());

                public void Insert(RichText text, Vector2I start) =>
                    InsertTextAction(text.GetApiData(), start);

                public void Insert(RichString text, Vector2I start) =>
                    InsertStringAction(text.GetApiData(), start);

                public void Insert(string text, Vector2I start) =>
                    InsertStringAction(new RichStringMembers(new StringBuilder(text), Format.data), start);

                public RichText GetText() =>
                    GetTextRange(Vector2I.Zero, GetLastIndex() - new Vector2I(0, 1));

                public RichText GetTextRange(Vector2I start, Vector2I end) =>
                    new RichText(GetOrSetMemberFunc(new RangeData(start, end), (int)TextBuilderAccessors.GetRange) as IList<RichStringMembers>);

                public void SetFormatting(GlyphFormat format) =>
                    SetFormatting(Vector2I.Zero, GetLastIndex() - new Vector2I(0, 1), format);

                public void SetFormatting(Vector2I start, Vector2I end, GlyphFormat format) =>
                    GetOrSetMemberFunc(new RangeFormatData(start, end, format.data), (int)TextBuilderAccessors.SetFormatting);

                public void RemoveAt(Vector2I index) =>
                    RemoveRange(index, index);

                public void RemoveRange(Vector2I start, Vector2I end) =>
                    GetOrSetMemberFunc(new RangeData(start, end), (int)TextBuilderAccessors.RemoveRange);

                public void Clear() =>
                    ClearAction();

                protected Vector2I GetLastIndex()
                {
                    Vector2I start = new Vector2I(Math.Max(0, Count - 1), 0);

                    if (Count > 0)
                        start.Y = Math.Max(0, this[start.X].Count);

                    return start;
                }

                protected class LineData : ILine
                {
                    public IRichChar this[int ch] => new RichCharData(parent, new Vector2I(index, ch));
                    public int Count => (int)parent.GetLineMemberFunc(index, (int)LineAccessors.Count);
                    public Vector2 Size => (Vector2)parent.GetLineMemberFunc(index, (int)LineAccessors.Size);

                    private readonly TextBuilder parent;
                    private readonly int index;

                    public LineData(TextBuilder parent, int index)
                    {
                        this.parent = parent;
                        this.index = index;
                    }
                }

                protected class RichCharData : IRichChar
                {
                    public char Ch => (char)parent.GetCharMemberFunc(index, (int)RichCharAccessors.Ch);
                    public GlyphFormat Format => new GlyphFormat((GlyphFormatMembers)parent.GetCharMemberFunc(index, (int)RichCharAccessors.Format));
                    public Vector2 Size => (Vector2)parent.GetCharMemberFunc(index, (int)RichCharAccessors.Size);
                    public Vector2 Offset => (Vector2)parent.GetCharMemberFunc(index, (int)RichCharAccessors.Offset);

                    private readonly TextBuilder parent;
                    private readonly Vector2I index;

                    public RichCharData(TextBuilder parent, Vector2I index)
                    {
                        this.parent = parent;
                        this.index = index;
                    }
                }
            }
        }
    }
}