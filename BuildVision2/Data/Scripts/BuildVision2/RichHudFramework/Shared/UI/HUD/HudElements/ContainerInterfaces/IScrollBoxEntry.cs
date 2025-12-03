namespace RichHudFramework.UI
{
	/// <summary>
	/// Interface for container entries within a scrollable list or box.
	/// </summary>
	/// <typeparam name="TElement">UI element type used for the entry</typeparam>
	public interface IScrollBoxEntry<TElement> : IChainElementContainer<TElement>
		where TElement : HudElementBase
	{
		/// <summary>
		/// Determines if the entry is enabled. Disabled entries are hidden from lists.
		/// </summary>
		bool Enabled { get; set; }
	}

	/// <summary>
	/// Interface for scrollbox entries that associate an arbitrary data object with the UI element.
	/// </summary>
	/// <typeparam name="TElement">UI element type used for the entry</typeparam>
	/// <typeparam name="TData">Data type associated with the entry</typeparam>
	public interface IScrollBoxEntryTuple<TElement, TData> : IScrollBoxEntry<TElement>
		where TElement : HudElementBase
	{
		/// <summary>
		/// The data object paired with the UI element in this entry.
		/// </summary>
		TData AssocMember { get; set; }
	}
}