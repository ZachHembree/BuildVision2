namespace RichHudFramework.UI
{
	/// <summary>
	/// Container class associating a ScrollBox element with an arbitrary object of 
	/// type <typeparamref name="TData"/>.
	/// </summary>
	/// <typeparam name="TElement">UI element type used for the entry</typeparam>
	/// <typeparam name="TData">Data type associated with the entry</typeparam>
	public class ScrollBoxEntryTuple<TElement, TData>
		: ScrollBoxEntry<TElement>, IScrollBoxEntryTuple<TElement, TData>
		where TElement : HudElementBase
	{
		/// <summary>
		/// The object associated with the entry.
		/// </summary>
		public virtual TData AssocMember { get; set; }

		public ScrollBoxEntryTuple()
		{ }
	}

	/// <summary>
	/// Container class associating a base ScrollBox element with an arbitrary object of 
	/// type <typeparamref name="TData"/>.
	/// </summary>
	/// <typeparam name="TData">Data type associated with the entry</typeparam>
	public class ScrollBoxEntryTuple<TData> : ScrollBoxEntryTuple<HudElementBase, TData>
	{ }
}