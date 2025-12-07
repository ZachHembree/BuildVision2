using System;
using System.Collections;
using System.Collections.Generic;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
	using ControlContainerMembers = MyTuple<
		ApiMemberAccessor, // GetOrSetMember,
		MyTuple<object, Func<int>>, // Member List
		object // ID
	>;

	namespace UI.Client
	{
		/// <summary>
		/// Represents a horizontally scrolling row of <see cref="ControlTile"/>s within a <see cref="ControlPage"/> 
		/// in the <see cref="RichHudTerminal"/>.
		/// </summary>
		public class ControlCategory : IControlCategory
		{
			/// <summary>
			/// The primary header text displayed above the category.
			/// </summary>
			public string HeaderText
			{
				get { return GetOrSetMemberFunc(null, (int)ControlCatAccessors.HeaderText) as string; }
				set { GetOrSetMemberFunc(value, (int)ControlCatAccessors.HeaderText); }
			}

			/// <summary>
			/// The secondary description/subheader text displayed below the header.
			/// </summary>
			public string SubheaderText
			{
				get { return GetOrSetMemberFunc(null, (int)ControlCatAccessors.SubheaderText) as string; }
				set { GetOrSetMemberFunc(value, (int)ControlCatAccessors.SubheaderText); }
			}

			/// <summary>
			/// Read-only collection of <see cref="ControlTile"/>s assigned to this category.
			/// </summary>
			public IReadOnlyList<ControlTile> Tiles { get; }

			/// <summary>
			/// Nested collection initializer utility property
			/// </summary>
			public IControlCategory TileContainer => this;

			/// <summary>
			/// Unique identifier used by the Framework API.
			/// </summary>
			public object ID => data.Item3;

			/// <summary>
			/// Determines whether or not the category will be drawn in the menu.
			/// </summary>
			public bool Enabled
			{
				get { return (bool)GetOrSetMemberFunc(null, (int)ControlCatAccessors.Enabled); }
				set { GetOrSetMemberFunc(value, (int)ControlCatAccessors.Enabled); }
			}

			private ApiMemberAccessor GetOrSetMemberFunc => data.Item1;
			private readonly ControlContainerMembers data;

			public ControlCategory() : this(RichHudTerminal.Instance.GetNewMenuCategory())
			{ }

			/// <summary>
			/// Initializes a new control category from an API data tuple.
			/// </summary>
			/// <exclude/>
			public ControlCategory(ControlContainerMembers data)
			{
				this.data = RichHudTerminal.Instance.GetNewMenuCategory();

				var GetTileDataFunc = data.Item2.Item1 as Func<int, ControlContainerMembers>;
				Func<int, ControlTile> GetTileFunc = x => new ControlTile(GetTileDataFunc(x));

				Tiles = new ReadOnlyApiCollection<ControlTile>(GetTileFunc, data.Item2.Item2);
			}

			IEnumerator<ControlTile> IEnumerable<ControlTile>.GetEnumerator() =>
				Tiles.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() =>
				Tiles.GetEnumerator();

			/// <summary>
			/// Adds a <see cref="ControlTile"/> to the category.
			/// </summary>
			public void Add(ControlTile tile) =>
				GetOrSetMemberFunc(tile.ID, (int)ControlCatAccessors.AddMember);

			/// <summary>
			/// Returns the internal API data tuple.
			/// </summary>
			/// <exclude/>
			public ControlContainerMembers GetApiData() =>
				data;
		}
	}
}