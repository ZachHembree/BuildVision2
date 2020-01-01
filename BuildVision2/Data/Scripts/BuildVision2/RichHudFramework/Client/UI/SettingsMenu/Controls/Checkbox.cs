using System;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;

namespace RichHudFramework.UI.Client
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    /// <summary>
    /// Creates a named checkbox designed to mimic the appearance of checkboxes in the SE terminal.
    /// </summary>
    public class Checkbox : TerminalValue<bool>
    {
        public Checkbox() : base(MenuControls.Checkbox)
        { }
    }
}