using System;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI.Client
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    internal enum TextFieldAccessors : int
    {
        CharFilterFunc = 16,
    }

    public class TextField : TerminalValue<string, TextField>
    {
        public Func<char, bool> CharFilterFunc
        {
            get { return GetOrSetMember(null, (int)TextFieldAccessors.CharFilterFunc) as Func<char, bool>; }
            set { GetOrSetMember(value, (int)TextFieldAccessors.CharFilterFunc); }
        }

        public TextField() : base(MenuControls.TextField)
        { }
    }
}