using System;
using System.Collections.Generic;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Interface for UI elements that support child elments using containers.
        /// </summary>
        public interface IHudCollection<TElementContainer, TElement> : IReadOnlyList<TElementContainer>, ICollection<TElementContainer>
            where TElementContainer : IHudElementContainer<TElement>, new()
            where TElement : HudElementBase
        {
            /// <summary>
            /// UI elements in the collection
            /// </summary>
            IReadOnlyList<TElementContainer> ChainEntries { get; }

            /// <summary>
            /// Adds an element of type <see cref="TElement"/> to the chain.
            /// </summary>
            void Add(TElement element);

            /// <summary>
            /// Add the given range to the end of the chain.
            /// </summary>
            void AddRange(IReadOnlyList<TElementContainer> newChainEntries);

            /// <summary>
            /// Finds the chain member that meets the conditions required by the predicate.
            /// </summary>
            TElementContainer Find(Func<TElementContainer, bool> predicate);

            /// <summary>
            /// Finds the index of the chain member that meets the conditions required by the predicate.
            /// </summary>
            int FindIndex(Func<TElementContainer, bool> predicate);

            /// <summary>
            /// Adds an element of type <see cref="TElement"/> at the given index.
            /// </summary>
            void Insert(int index, TElement element);

            /// <summary>
            /// Adds an element of type <see cref="TElementContainer"/> at the given index.
            /// </summary>
            void Insert(int index, TElementContainer container);

            /// <summary>
            /// Insert the given range into the chain.
            /// </summary>
            void InsertRange(int index, IReadOnlyList<TElementContainer> newChainEntries);

            /// <summary>
            /// Removes the collection member that meets the conditions required by the predicate.
            /// </summary>
            void Remove(Func<TElement, bool> predicate);

            /// <summary>
            /// Removes the collection member that meets the conditions required by the predicate.
            /// </summary>
            void Remove(Func<TElementContainer, bool> predicate);

            /// <summary>
            /// Removes the specified element from the chain.
            /// </summary>
            void Remove(TElement chainElement);

            /// <summary>
            /// Remove the chain element at the given index.
            /// </summary>
            void RemoveAt(int index);

            /// <summary>
            /// Removes the specfied range from the chain. Normal child elements not affected.
            /// </summary>
            void RemoveRange(int index, int count);

            /// <summary>
            /// Sorts the entires using the default comparer.
            /// </summary>
            void Sort();

            /// <summary>
            /// Sorts the entries using the given comparer.
            /// </summary>
            void Sort(Func<TElementContainer, TElementContainer, int> comparison);
        }
    }
}
