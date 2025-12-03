using System.Collections.Generic;

namespace RichHudFramework.UI
{
	/// <summary>
	/// Minimal interface for UI elements that represent a collection of selectable 
	/// <see cref="IScrollBoxEntry{TElement}"/>.
	/// </summary>
	public interface IEntryBox<TContainer, TElement> : IEnumerable<TContainer>, IReadOnlyHudElement
		where TContainer : IScrollBoxEntry<TElement>, new()
		where TElement : HudElementBase, IMinLabelElement
	{
		/// <summary>
		/// Invoked when a member of the list is selected.
		/// </summary>
		event EventHandler SelectionChanged;

		/// <summary>
		/// Read-only collection of list entries.
		/// </summary>
		IReadOnlyList<TContainer> EntryList { get; }

		/// <summary>
		/// Current selection. Null if empty.
		/// </summary>
		TContainer Selection { get; }
	}

	/// <summary>
	/// Minimal interface for clickable UI elements that represent a collection of selectable 
	/// <see cref="ListBoxEntry{TValue}"/>.
	/// </summary>
	public interface IEntryBox<TValue> : IEntryBox<ListBoxEntry<TValue>, Label>
	{ }
}