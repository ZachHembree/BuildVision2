using System;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace RichHudFramework.UI.Client
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    public class TextField : TerminalValue<string>
    {
        public TextField() : base(MenuControls.TextField)
        { }
    }
}