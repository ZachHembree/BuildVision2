using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework.UI.Client
{
	/// <summary>
	/// A collapsing dropdown list with a label. For <see cref="ControlTile"/>s.
	/// <para>Designed to mimic the appearance of the dropdown in the SE terminal.</para>
	/// </summary>
	/// <typeparam name="T">The type of object associated with each list entry.</typeparam>
	public class TerminalDropdown<T> : TerminalValue<EntryData<T>>
	{
		/// <summary>
		/// The currently selected list entry.
		/// </summary>
		public override EntryData<T> Value
		{
			get { return List.Selection; }
			set { List.SetSelection(value); }
		}

		/// <summary>
		/// Accessor for the underlying list data model.
		/// </summary>
		public ListBoxData<T> List { get; }

		public TerminalDropdown() : base(MenuControls.DropdownControl)
		{
			var listData = GetOrSetMember(null, (int)ListControlAccessors.ListAccessors) as ApiMemberAccessor;

			List = new ListBoxData<T>(listData);
		}
	}
}