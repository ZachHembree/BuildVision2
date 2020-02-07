using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    using BoolProp = MyTuple<Func<bool>, Action<bool>>;
    using FloatProp = MyTuple<Func<float>, Action<float>>;
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;
    using Vec2Prop = MyTuple<Func<Vector2>, Action<Vector2>>;

    namespace UI
    {
        using UI.Client;
        using UI.Server;
        using TextBuilderMembers = MyTuple<
            MyTuple<Func<int, int, object>, Func<int>>, // GetLineMember, GetLineCount
            Func<Vector2I, int, object>, // GetCharMember
            Func<object, int, object>, // GetOrSetMember
            Action<IList<RichStringMembers>, Vector2I>, // Insert
            Action<IList<RichStringMembers>>, // SetText
            Action // Clear
        >;

        namespace Rendering.Client
        {
            using TextBoardMembers = MyTuple<
                TextBuilderMembers,
                FloatProp, // Scale
                Func<Vector2>, // Size
                Func<Vector2>, // TextSize
                Vec2Prop, // FixedSize
                Action<Vector2> // UpdateText, Draw 
            >;

            public class TextBoard : TextBuilder, ITextBoard
            {
                /// <summary>
                /// Invoked whenever a change is made to the text.
                /// </summary>
                public event Action OnTextChanged
                {
                    add
                    {
                        var args = new MyTuple<bool, Action>(true, value);
                        GetOrSetMemberFunc(args, (int)TextBoardAccessors.OnTextChanged);
                    }
                    remove
                    {
                        var args = new MyTuple<bool, Action>(false, value);
                        GetOrSetMemberFunc(args, (int)TextBoardAccessors.OnTextChanged);
                    }
                }

                /// <summary>
                /// Scale of the text board. Applied after scaling specified in GlyphFormat.
                /// </summary>
                public float Scale { get { return ScaleProp.Getter(); } set { ScaleProp.Setter(value); } }

                /// <summary>
                /// Size of the text box as rendered
                /// </summary>
                public Vector2 Size => GetSizeFunc();

                /// <summary>
                /// Full text size including any text outside the visible range.
                /// </summary>
                public Vector2 TextSize => GetTextSizeFunc();

                /// <summary>
                /// Used to change the position of the text within the text element. AutoResize must be disabled for this to work.
                /// </summary>
                public Vector2 TextOffset
                {
                    get { return (Vector2)GetOrSetMemberFunc(null, (int)TextBoardAccessors.TextOffset); }
                    set { GetOrSetMemberFunc(value, (int)TextBoardAccessors.TextOffset); }
                }

                /// <summary>
                /// Size of the text box when AutoResize is set to false. Does nothing otherwise.
                /// </summary>
                public Vector2 FixedSize { get { return FixedSizeProp.Getter(); } set { FixedSizeProp.Setter(value); } }

                /// <summary>
                /// If true, the text board will automatically resize to fit the text.
                /// </summary>
                public bool AutoResize
                {
                    get { return (bool)GetOrSetMemberFunc(null, (int)TextBoardAccessors.AutoResize); }
                    set { GetOrSetMemberFunc(value, (int)TextBoardAccessors.AutoResize); }
                }

                /// <summary>
                /// If true, the text will be vertically aligned to the center of the text board.
                /// </summary>
                public bool VertCenterText
                {
                    get { return (bool)GetOrSetMemberFunc(null, (int)TextBoardAccessors.VertAlign); }
                    set { GetOrSetMemberFunc(value, (int)TextBoardAccessors.VertAlign); }
                }

                private readonly PropWrapper<float> ScaleProp;
                private readonly Func<Vector2> GetSizeFunc;
                private readonly Func<Vector2> GetTextSizeFunc;
                private readonly PropWrapper<Vector2> FixedSizeProp;
                private readonly Action<Vector2> DrawAction;

                public TextBoard() : this(HudMain.GetTextBoardData())
                { }

                private TextBoard(TextBoardMembers members) : base(members.Item1)
                {
                    Format = GlyphFormat.Black;
                    ScaleProp = new PropWrapper<float>(members.Item2);
                    GetSizeFunc = members.Item3;
                    GetTextSizeFunc = members.Item4;
                    FixedSizeProp = new PropWrapper<Vector2>(members.Item5);
                    DrawAction = members.Item6;
                }

                public void Draw(Vector2 origin) =>
                    DrawAction(origin);

                /// <summary>
                /// Calculates and applies the minimum offset needed to ensure that the character at the specified index
                /// is within the visible range.
                /// </summary>
                public void MoveToChar(Vector2I index) =>
                    GetOrSetMemberFunc(index, (int)TextBoardAccessors.MoveToChar);

                /// <summary>
                /// Returns the index of the character at the given offset.
                /// </summary>
                public Vector2I GetCharAtOffset(Vector2 offset) =>
                    (Vector2I)GetOrSetMemberFunc(offset, (int)TextBoardAccessors.GetCharAtOffset);
            }
        }

        namespace Rendering.Server
        { }
    }
}