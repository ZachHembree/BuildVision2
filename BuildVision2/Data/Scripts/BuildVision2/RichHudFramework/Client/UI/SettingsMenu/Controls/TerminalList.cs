using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework.UI.Client
{
	/// <summary>
	/// Internal API member accessor indices
	/// </summary>
	/// <exclude/>
	public enum ListControlAccessors : int
	{
		ListAccessors = 16,
	}

	/// <summary>
	/// A non-collapsing, fixed-height list box with a label. For <see cref="ControlTile"/>s.
	/// <para>Designed to mimic the appearance of the list box in the SE terminal.</para>
	/// </summary>
	public class TerminalList<T> : TerminalValue<EntryData<T>>
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

		public TerminalList() : base(MenuControls.ListControl)
		{
			var listData = GetOrSetMember(null, (int)ListControlAccessors.ListAccessors) as ApiMemberAccessor;

			List = new ListBoxData<T>(listData);
		}
	}
}