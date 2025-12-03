using System;
using System.Collections.Generic;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

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

		/// <summary>
		/// Internal API data accessor enums for an RHF terminal control category
		/// </summary>
		/// <exclude/>
		public enum ControlCatAccessors : int
        {
            /// <summary>
            /// IList{RichStringMembers}
            /// </summary>
            HeaderText = 1,

            /// <summary>
            /// IList{RichStringMembers}
            /// </summary>
            SubheaderText = 2,

            /// <summary>
            /// bool
            /// </summary>
            Enabled = 3,

            /// <summary>
            /// out: MemberAccessor
            /// </summary>
            AddMember = 4
        }

		/// <summary>
		/// Internal interface for a horizontally scrolling list of control tiles, implemented in both 
        /// master and client modules.
		/// </summary>
		/// <exclude/>
		public interface IControlCategory : IControlCategory<ControlTile>
        {
            /// <summary>
            /// Read only collection of <see cref="ControlTile"/>s assigned to this category
            /// </summary>
            IReadOnlyList<ControlTile> Tiles { get; }

            /// <summary>
            /// Used to allow the addition of control tiles to categories using collection-initializer syntax in
            /// conjunction with normal initializers.
            /// </summary>
            IControlCategory TileContainer { get; }
        }

		/// <summary>
		/// Internal base interface for a control container in the RHF terminal
		/// </summary>
		/// <typeparam name="TElementContainer"></typeparam>
		/// <exclude/>
		public interface IControlCategory<TElementContainer> : IEnumerable<TElementContainer>
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
            /// Determines whether or not the element will be drawn.
            /// </summary>
            bool Enabled { get; set; }

            /// <summary>
            /// Unique identifier.
            /// </summary>
            object ID { get; }

            /// <summary>
            /// Adds members to the category
            /// </summary>
            void Add(TElementContainer tile);

            /// <summary>
            /// Retrieves information used by the Framework API
            /// </summary>
            ControlContainerMembers GetApiData();
        }
    }
}