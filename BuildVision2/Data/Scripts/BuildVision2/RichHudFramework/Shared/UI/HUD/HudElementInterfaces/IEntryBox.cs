using System.Collections.Generic;

namespace RichHudFramework.UI
{
	/// <summary>
	/// Minimal interface for UI elements that represent a collection of selectable 
	/// <see cref="IScrollBoxEntry{TElement}"/>.
	/// </summary>
	public interface IEntryBox<TContainer, TElement> : IEnumerable<TContainer>, IValueControl<TContainer>
		where TContainer : IScrollBoxEntry<TElement>, new()
		where TElement : HudElementBase, IMinLabelElement
	{
        /// <summary>
        /// Read-only collection of list entries.
        /// </summary>
        IReadOnlyList<TContainer> EntryList { get; }

		/// <summary>
		/// Current selection. Null if empty.
		/// </summary>
		new TContainer Value { get; }
	}
}