namespace RichHudFramework.UI
{
	/// <summary>
	/// Interface for a selection box entry wrapping a UI element.
	/// <para>Adds selection highlighting capabilities to the standard scroll box entry.</para>
	/// </summary>
	/// <typeparam name="TElement">UI element type used for the entry</typeparam>
	public interface ISelectionBoxEntry<TElement> : IScrollBoxEntry<TElement>
		 where TElement : HudElementBase
	{
		/// <summary>
		/// If true, the UI element will accept highlighting effects (e.g., mouse-over or selection).
		/// </summary>
		bool AllowHighlighting { get; set; }

		/// <summary>
		/// Resets the entry to its default state
		/// </summary>
		void Reset();
	}

	/// <summary>
	/// Interface for a selection box entry that pairs a UI element with a data value of 
	/// type <typeparamref name="TValue"/>.
	/// </summary>
	/// <typeparam name="TElement">UI element type used for the entry</typeparam>
	/// <typeparam name="TValue">Data type associated with the entry</typeparam>
	public interface ISelectionBoxEntryTuple<TElement, TValue>
		: ISelectionBoxEntry<TElement>, IScrollBoxEntryTuple<TElement, TValue>
		where TElement : HudElementBase
	{ }
}
