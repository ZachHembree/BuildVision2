using System;
using System.Collections.Generic;

namespace RichHudFramework.UI
{
	/// <summary>
	/// Generic tree box supporting custom entry types of arbitrary height, provided they have a label.
	/// </summary>
	/// <typeparam name="TContainer">
	/// Container type that wraps each entry's UI element and provides selection/association data.
	/// </typeparam>
	/// <typeparam name="TElement">
	/// The actual UI element displayed for each entry (must support minimal labeling).
	/// </typeparam>
	public class TreeBox<TContainer, TElement> : TreeBoxBase<TContainer, TElement>
		where TElement : HudElementBase, IMinLabelElement
		where TContainer : class, ISelectionBoxEntry<TElement>, new()
	{
		/// <summary>
		/// Returns the entry at the given index
		/// </summary>
		public TContainer this[int index] => selectionBox.EntryChain[index];

		/// <summary>
		/// Enables collection-initializer syntax (e.g., new MyTreeBox { ListContainer = { entry1, entry2 } })
		/// </summary>
		public new TreeBox<TContainer, TElement> ListContainer => this;

		public TreeBox(HudParentBase parent) : base(parent)
		{ }

		public TreeBox() : base(null)
		{ }

		/// <summary>
		/// Adds an element of type TElement to the collection.
		/// </summary>
		public void Add(TElement element) =>
			selectionBox.EntryChain.Add(element);

		/// <summary>
		/// Adds an element of type TContainer to the collection.
		/// </summary>
		public void Add(TContainer element) =>
			selectionBox.EntryChain.Add(element);

		/// <summary>
		/// Add the given range to the end of the collection.
		/// </summary>
		public void AddRange(IReadOnlyList<TContainer> newContainers) =>
			selectionBox.EntryChain.AddRange(newContainers);

		/// <summary>
		/// Remove all elements in the collection. Does not affect normal child elements.
		/// </summary>
		public void Clear() =>
			selectionBox.EntryChain.Clear();

		/// <summary>
		/// Finds the collection member that meets the conditions required by the predicate.
		/// </summary>
		public TContainer Find(Func<TContainer, bool> predicate) =>
			selectionBox.EntryChain.Find(predicate);

		/// <summary>
		/// Finds the index of the collection member that meets the conditions required by the predicate.
		/// </summary>
		public int FindIndex(Func<TContainer, bool> predicate) =>
			selectionBox.EntryChain.FindIndex(predicate);

		/// <summary>
		/// Adds an element of type TContainer at the given index.
		/// </summary>
		public void Insert(int index, TContainer container) =>
			selectionBox.EntryChain.Insert(index, container);

		/// <summary>
		/// Insert the given range into the collection.
		/// </summary>
		public void InsertRange(int index, IReadOnlyList<TContainer> newContainers) =>
			selectionBox.EntryChain.InsertRange(index, newContainers);

		/// <summary>
		/// Removes the specified element from the collection.
		/// </summary>
		public bool Remove(TContainer collectionElement) =>
			selectionBox.EntryChain.Remove(collectionElement);

		/// <summary>
		/// Removes the collection member that meets the conditions required by the predicate.
		/// </summary>
		public bool Remove(Func<TContainer, bool> predicate) =>
			selectionBox.EntryChain.Remove(predicate);

		/// <summary>
		/// Remove the collection element at the given index.
		/// </summary>
		public bool RemoveAt(int index) =>
			selectionBox.EntryChain.RemoveAt(index);

		/// <summary>
		/// Removes the specfied range from the collection. Normal child elements not affected.
		/// </summary>
		public void RemoveRange(int index, int count) =>
			selectionBox.EntryChain.RemoveRange(index, count);
	}

	/// <summary>
	/// Tree box supporting custom entry types of arbitrary height, provided they have a label.
	/// <para>Alias of <see cref="TreeBox{TContainer, TElement}"/> with 
	/// <see cref="LabelElementBase"/> as the element and <see cref="SelectionBoxEntryTuple{TElement, TValue}"/> as the container.</para>
	/// </summary>
	/// <typeparam name="TValue">
	/// Value type associated with each entry.
	/// </typeparam>
	public class TreeBox<TValue> : TreeBox<SelectionBoxEntryTuple<LabelElementBase, TValue>, LabelElementBase>
	{
		/// <summary>
		/// Enables collection-initializer syntax (e.g., new MyTreeBox { ListContainer = { entry1, entry2 } })
		/// </summary>
		public new TreeBox<TValue> ListContainer => this;

		/// <summary>
		/// Adds an entry container with the given element and key value.
		/// </summary>
		public void Add(LabelElementBase keyElement, TValue value, bool allowHighlighting = true)
		{
			var container = new SelectionBoxEntryTuple<LabelElementBase, TValue>();
			container.SetElement(keyElement);
			container.AssocMember = value;
			container.AllowHighlighting = allowHighlighting;
			selectionBox.EntryChain.Add(container);
		}
	}
}