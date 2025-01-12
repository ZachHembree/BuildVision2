using System;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI.Client
{
    /// <summary>
    /// Label for use within control tiles and control categories
    /// </summary>
    public class TerminalLabel : TerminalControlBase
    {
        public TerminalLabel() : base(MenuControls.Label)
        { }
    }
}
