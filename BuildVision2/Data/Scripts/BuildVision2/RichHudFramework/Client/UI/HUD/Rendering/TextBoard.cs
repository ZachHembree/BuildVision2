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
        using UI.Client;
        using UI.Server;
        using TextBuilderMembers = MyTuple<
            MyTuple<Func<int, int, object>, Func<int>>, // GetLineMember, GetLineCount
            Func<Vector2I, int, object>, // GetCharMember
            Func<object, int, object>, // GetOrSetMember
            Action<IList<RichStringMembers>, Vector2I>, // Insert
            Action<RichStringMembers, Vector2I>, // Insert
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
                Action<Vector2> // Draw 
            >;

            public class TextBoard : TextBuilder, ITextBoard
            {
                public float Scale { get { return ScaleProp.Getter(); } set { ScaleProp.Setter(value); } }
                public Vector2 Size => GetSizeFunc();
                public Vector2 TextSize => GetTextSizeFunc();
                public Vector2 FixedSize { get { return FixedSizeProp.Getter(); } set { FixedSizeProp.Setter(value); } }
                public bool AutoResize
                {
                    get { return (bool)GetOrSetMemberFunc(null, (int)TextBoardAccessors.AutoResize); }
                    set { GetOrSetMemberFunc(value, (int)TextBoardAccessors.AutoResize); }
                } 
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

                public void MoveToChar(Vector2I index) =>
                    GetOrSetMemberFunc(index, (int)TextBoardAccessors.MoveToChar);

                public Vector2I GetCharAtOffset(Vector2 offset) =>
                    (Vector2I)GetOrSetMemberFunc(offset, (int)TextBoardAccessors.GetCharAtOffset);
            }
        }

        namespace Rendering.Server
        { }
    }
}