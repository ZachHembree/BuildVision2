using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    namespace UI
    {
        using Server;
        using Client;

        using ControlContainerMembers = MyTuple<
            ApiMemberAccessor, // GetOrSetMember,
            MyTuple<object, Func<int>>, // Member List
            object // ID
        >;

        internal enum ControlCatAccessors : int
        {
            /// <summary>
            /// IList<RichStringMembers>
            /// </summary>
            HeaderText = 1,

            /// <summary>
            /// IList<RichStringMembers>
            /// </summary>
            SubheaderText = 2,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 3,

            /// <summary>
            /// out: MemberAccessor
            /// </summary>
            AddTile = 4,
        }

        /// <summary>
        /// Horizontally scrolling list of control tiles.
        /// </summary>
        public interface IControlCategory : IEnumerable<IControlTile>
        {
            /// <summary>
            /// Category name
            /// </summary>
            string HeaderText { get; set; }

            /// <summary>
            /// Category information
            /// </summary>
            string SubheaderText { get; set; }

            /// <summary>
            /// Read only collection of <see cref="IControlTile"/>s assigned to this category
            /// </summary>
            IReadOnlyCollection<IControlTile> Tiles { get; }

            IControlCategory TileContainer { get; }

            /// <summary>
            /// Determines whether or not the element will be drawn.
            /// </summary>
            bool Enabled { get; set; }

            /// <summary>
            /// Unique identifier.
            /// </summary>
            object ID { get; }

            /// <summary>
            /// Adds a <see cref="IControlTile"/> to the category
            /// </summary>
            void Add(ControlTile tile);

            /// <summary>
            /// Retrieves information used by the Framework API
            /// </summary>
            ControlContainerMembers GetApiData();
        }
    }
}