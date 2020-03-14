using System;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    using ControlMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember
        object // ID
    >;

    namespace UI
    {
        using Client;
        using Server;

        public enum TerminalPageAccessors : int
        {
            /// <summary>
            /// IList<RichStringMembers>
            /// </summary>
            Name = 1,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 2,
        }

        public interface ITerminalPage
        {
            /// <summary>
            /// Name of the <see cref="ITerminalPage"/> as it appears in the dropdown of the <see cref="IModControlRoot"/>.
            /// </summary>
            string Name { get; set; }

            /// <summary>
            /// Determines whether or not the <see cref="ITerminalPage"/> will be drawn.
            /// </summary>
            bool Enabled { get; set; }

            /// <summary>
            /// Unique identifier
            /// </summary>
            object ID { get; }

            /// <summary>
            /// Retrieves information used by the Framework API
            /// </summary>
            ControlMembers GetApiData();
        }
    }
}