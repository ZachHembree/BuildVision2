using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI.Client
{
	using ControlContainerMembers = MyTuple<
		ApiMemberAccessor, // GetOrSetMember,
		MyTuple<object, Func<int>>, // Member List
		object // ID
	>;
	using ControlMembers = MyTuple<
		ApiMemberAccessor, // GetOrSetMember
		object // ID
	>;

	/// <summary>
	/// Represents a vertical layout column within a <see cref="ControlCategory"/>. 
	/// <para>
	/// Tiles are organized horizontally within a category. Each tile should generally contain 
	/// no more than 3 controls to maintain proper UI scaling and layout bounds.
	/// </para>
	/// </summary>
	public class ControlTile : IControlTile
	{
		/// <summary>
		/// Read-only collection of the <see cref="TerminalControlBase"/> elements attached to this tile.
		/// </summary>
		public IReadOnlyList<TerminalControlBase> Controls { get; }

		/// <summary>
		/// Interface accessor for use in collection initializers.
		/// </summary>
		public IControlTile ControlContainer => this;

		/// <summary>
		/// Determines whether or not the tile and its contents will be rendered in the list.
		/// </summary>
		public bool Enabled
		{
			get { return (bool)GetOrSetMemberFunc(null, (int)ControlTileAccessors.Enabled); }
			set { GetOrSetMemberFunc(value, (int)ControlTileAccessors.Enabled); }
		}

		/// <summary>
		/// Unique identifier used by the Framework API.
		/// </summary>
		public object ID => tileMembers.Item3;

		private ApiMemberAccessor GetOrSetMemberFunc => tileMembers.Item1;
		private readonly ControlContainerMembers tileMembers;

		public ControlTile() : this(RichHudTerminal.Instance.GetNewMenuTile())
		{ }

		/// <summary>
		/// Internal API initializer
		/// </summary>
		/// <exclude/>
		public ControlTile(ControlContainerMembers data)
		{
			tileMembers = data;

			var GetControlDataFunc = data.Item2.Item1 as Func<int, ControlMembers>;
			Func<int, TerminalControlBase> GetControlFunc = (x => new TerminalControl(GetControlDataFunc(x)));

			Controls = new ReadOnlyApiCollection<TerminalControlBase>(GetControlFunc, data.Item2.Item2);
		}

		IEnumerator<ITerminalControl> IEnumerable<ITerminalControl>.GetEnumerator() =>
			Controls.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() =>
			Controls.GetEnumerator();

		/// <summary>
		/// Adds a <see cref="TerminalControlBase"/> to this tile.
		/// <para>Note: Controls are stacked vertically within the tile.</para>
		/// </summary>
		public void Add(TerminalControlBase control) =>
			GetOrSetMemberFunc(control.ID, (int)ControlTileAccessors.AddControl);

		/// <summary>
		/// Retrieves the internal data tuple required by the Framework API.
		/// </summary>
		public ControlContainerMembers GetApiData() =>
			tileMembers;

		private class TerminalControl : TerminalControlBase
		{
			public TerminalControl(ControlMembers data) : base(data)
			{ }
		}
	}
}