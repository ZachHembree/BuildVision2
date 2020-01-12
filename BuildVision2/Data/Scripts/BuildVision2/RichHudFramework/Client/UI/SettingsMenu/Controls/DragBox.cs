using System;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;

namespace RichHudFramework.UI.Client
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    internal enum DragBoxAccessors : int
    {
        BoxSize = 16,
        AlignToEdge = 17,
    }

    public class DragBox : TerminalValue<Vector2, DragBox>
    {
        public Vector2 BoxSize
        {
            get { return (Vector2)GetOrSetMember(null, (int)DragBoxAccessors.BoxSize); }
            set { GetOrSetMember(value, (int)DragBoxAccessors.BoxSize); }
        }

        public bool AlignToEdge
        {
            get { return (bool)GetOrSetMember(null, (int)DragBoxAccessors.AlignToEdge); }
            set { GetOrSetMember(value, (int)DragBoxAccessors.AlignToEdge); }
        }

        public DragBox() : base(MenuControls.DragBox)
        { }
    }
}