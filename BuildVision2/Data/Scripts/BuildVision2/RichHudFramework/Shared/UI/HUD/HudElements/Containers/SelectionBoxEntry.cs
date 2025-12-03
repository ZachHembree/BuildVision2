namespace RichHudFramework.UI
{
	/// <summary>
	/// Implementation of a Selection Box entry.
	/// <para>Manages enablement and highlighting permissions for the contained element.</para>
	/// </summary>
	/// <typeparam name="TElement">UI element type used for the entry</typeparam>
	public class SelectionBoxEntry<TElement> : HudElementContainer<TElement>, ISelectionBoxEntry<TElement>
		where TElement : HudElementBase
	{
		/// <summary>
		/// Determines if the entry is enabled. Disabled entries are hidden from lists.
		/// </summary>
		public virtual bool Enabled { get; set; }

		/// <summary>
		/// If true, the UI element will accept highlighting effects (e.g., mouse-over or selection).
		/// </summary>
		public virtual bool AllowHighlighting { get; set; }

		public SelectionBoxEntry()
		{
			Enabled = true;
			AllowHighlighting = true;
		}

		/// <summary>
		/// Resets the entry to its default state
		/// </summary>
		public virtual void Reset()
		{
			Enabled = true;
			AllowHighlighting = true;
		}
	}

	/// <summary>
	/// Implementation of a Selection Box entry that pairs the element with a value.
	/// </summary>
	/// <typeparam name="TElement">UI element type used for the entry</typeparam>
	/// <typeparam name="TValue">Data type associated with the entry</typeparam>
	public class SelectionBoxEntryTuple<TElement, TValue>
		: SelectionBoxEntry<TElement>, ISelectionBoxEntryTuple<TElement, TValue>
		where TElement : HudElementBase
	{
		/// <summary>
		/// The data object paired with the UI element in this entry.
		/// </summary>
		public TValue AssocMember { get; set; }

		/// <summary>
		/// Resets the entry to its default state
		/// </summary>
		public override void Reset()
		{
			Enabled = true;
			AllowHighlighting = true;
			AssocMember = default(TValue);
		}
	}
}
