using System;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using RichHudFramework.UI.Rendering;

namespace RichHudFramework
{
    namespace UI
    {
        using Client;
        using Server;

        /// <summary>
        /// Scrollable text page used in the terminal.
        /// </summary>
        public interface ITextPage : ITerminalPage
        {
            /// <summary>
            /// Contents of the text box.
            /// </summary>
            RichText Text { get; set; }

            /// <summary>
            /// Text builder used to control the contents of the page
            /// </summary>
            ITextBuilder TextBuilder { get; }
        }
    }
}