using System;
using System.Collections.Generic;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Generic tree box supporting custom entry types of arbitrary height.
    /// </summary>
    /// <typeparam name="TContainer">Container element type wrapping the UI element</typeparam>
    /// <typeparam name="TElement">UI element in the list</typeparam>
    public class TreeBox<TContainer, TElement> : TreeBoxBase<TContainer, TElement>
        where TElement : HudElementBase, IMinLabelElement
        where TContainer : class, ISelectionBoxEntry<TElement>, new()
    {
        public TContainer this[int index] => selectionBox.hudChain[index];

        /// <summary>
        /// UI elements in the collection
        /// </summary>
        public IReadOnlyList<TContainer> Collection => selectionBox.hudChain.Collection;

        public TreeBox(HudParentBase parent) : base(parent)
        { }

        public TreeBox() : base(null)
        { }

        /// <summary>
        /// Adds an element of type <see cref="TElement"/> to the collection.
        /// </summary>
        public void Add(TElement element) =>
            selectionBox.hudChain.Add(element);

        /// <summary>
        /// Adds an element of type <see cref="TContainer"/> to the collection.
        /// </summary>
        public void Add(TContainer element) =>
            selectionBox.hudChain.Add(element);

        /// <summary>
        /// Add the given range to the end of the collection.
        /// </summary>
        public void AddRange(IReadOnlyList<TContainer> newContainers) =>
            selectionBox.hudChain.AddRange(newContainers);

        /// <summary>
        /// Remove all elements in the collection. Does not affect normal child elements.
        /// </summary>
        /// <param name="fast">Prevents registration from triggering a draw list
        /// update. Meant to be used in conjunction with pooled elements being
        /// unregistered/reregistered to the same parent.</param>
        public void Clear(bool fast = false) =>
            selectionBox.hudChain.Clear(fast);

        /// <summary>
        /// Finds the collection member that meets the conditions required by the predicate.
        /// </summary>
        public TContainer Find(Func<TContainer, bool> predicate) =>
            selectionBox.hudChain.Find(predicate);

        /// <summary>
        /// Finds the index of the collection member that meets the conditions required by the predicate.
        /// </summary>
        public int FindIndex(Func<TContainer, bool> predicate) =>
            selectionBox.hudChain.FindIndex(predicate);

        /// <summary>
        /// Adds an element of type <see cref="TContainer"/> at the given index.
        /// </summary>
        public void Insert(int index, TContainer container) =>
            selectionBox.hudChain.Insert(index, container);

        /// <summary>
        /// Insert the given range into the collection.
        /// </summary>
        public void InsertRange(int index, IReadOnlyList<TContainer> newContainers) =>
            selectionBox.hudChain.InsertRange(index, newContainers);

        /// <summary>
        /// Removes the specified element from the collection.
        /// </summary>
        /// <param name="fast">Prevents registration from triggering a draw list
        /// update. Meant to be used in conjunction with pooled elements being
        /// unregistered/reregistered to the same parent.</param>
        public bool Remove(TContainer collectionElement, bool fast) =>
            selectionBox.hudChain.Remove(collectionElement, fast);

        /// <summary>
        /// Removes the collection member that meets the conditions required by the predicate.
        /// </summary>
        /// <param name="fast">Prevents registration from triggering a draw list
        /// update. Meant to be used in conjunction with pooled elements being
        /// unregistered/reregistered to the same parent.</param>
        public bool Remove(Func<TContainer, bool> predicate, bool fast = false) =>
            selectionBox.hudChain.Remove(predicate, fast);

        /// <summary>
        /// Remove the collection element at the given index.
        /// </summary>
        /// <param name="fast">Prevents registration from triggering a draw list
        /// update. Meant to be used in conjunction with pooled elements being
        /// unregistered/reregistered to the same parent.</param>
        public bool RemoveAt(int index, bool fast = false) =>
            selectionBox.hudChain.RemoveAt(index, fast);

        /// <summary>
        /// Removes the specfied range from the collection. Normal child elements not affected.
        /// </summary>
        /// <param name="fast">Prevents registration from triggering a draw list
        /// update. Meant to be used in conjunction with pooled elements being
        /// unregistered/reregistered to the same parent.</param>
        public void RemoveRange(int index, int count, bool fast = false) =>
            selectionBox.hudChain.RemoveRange(index, count, fast);
    }
}