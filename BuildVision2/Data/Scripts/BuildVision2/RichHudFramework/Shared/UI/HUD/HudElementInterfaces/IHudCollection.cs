﻿using System;
using System.Collections.Generic;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Read-only interface for UI elements that support child elments using containers.
        /// </summary>
        public interface IReadOnlyHudCollection<TElementContainer, TElement> : IReadOnlyList<TElementContainer>, ICollection<TElementContainer>
            where TElementContainer : IHudElementContainer<TElement>, new()
            where TElement : HudElementBase
        {
            /// <summary>
            /// UI elements in the collection
            /// </summary>
            IReadOnlyList<TElementContainer> Collection { get; }

            /// <summary>
            /// Finds the chain member that meets the conditions required by the predicate.
            /// </summary>
            TElementContainer Find(Func<TElementContainer, bool> predicate);

            /// <summary>
            /// Finds the index of the chain member that meets the conditions required by the predicate.
            /// </summary>
            int FindIndex(Func<TElementContainer, bool> predicate);
        }

        /// <summary>
        /// Read-only interface for UI elements that support child elments using containers.
        /// </summary>
        public interface IReadOnlyHudCollection<TElementContainer> : IReadOnlyHudCollection<TElementContainer, HudElementBase>
            where TElementContainer : IHudElementContainer<HudElementBase>, new()
        { }

        /// <summary>
        /// Read-only interface for UI elements that support child elments using containers.
        /// </summary>
        public interface IReadOnlyHudCollection : IReadOnlyHudCollection<HudElementContainer<HudElementBase>, HudElementBase>
        { }

        /// <summary>
        /// Interface for UI elements that support child elments using containers.
        /// </summary>
        public interface IHudCollection<TElementContainer, TElement> : IReadOnlyHudCollection<TElementContainer, TElement>
            where TElementContainer : IHudElementContainer<TElement>, new()
            where TElement : HudElementBase
        {
            /// <summary>
            /// Adds an element of type <see cref="TElement"/> to the chain.
            /// </summary>
            void Add(TElement element);

            /// <summary>
            /// Add the given range to the end of the chain.
            /// </summary>
            void AddRange(IReadOnlyList<TElementContainer> newChainEntries);

            /// <summary>
            /// Adds an element of type <see cref="TElementContainer"/> at the given index.
            /// </summary>
            void Insert(int index, TElementContainer container);

            /// <summary>
            /// Insert the given range into the chain.
            /// </summary>
            void InsertRange(int index, IReadOnlyList<TElementContainer> newChainEntries);

            /// <summary>
            /// Removes the specified element from the chain.
            /// </summary>
            /// <param name="fast">Prevents registration from triggering a draw list
            /// update. Meant to be used in conjunction with pooled elements being
            /// unregistered/reregistered to the same parent.</param>
            bool Remove(TElementContainer chainElement, bool fast);

            /// <summary>
            /// Removes the collection member that meets the conditions required by the predicate.
            /// </summary>
            /// <param name="fast">Prevents registration from triggering a draw list
            /// update. Meant to be used in conjunction with pooled elements being
            /// unregistered/reregistered to the same parent.</param>
            bool Remove(Func<TElementContainer, bool> predicate, bool fast = false);

            /// <summary>
            /// Remove the chain element at the given index.
            /// </summary>
            /// <param name="fast">Prevents registration from triggering a draw list
            /// update. Meant to be used in conjunction with pooled elements being
            /// unregistered/reregistered to the same parent.</param>
            bool RemoveAt(int index, bool fast = false);

            /// <summary>
            /// Removes the specfied range from the chain. Normal child elements not affected.
            /// </summary>
            /// <param name="fast">Prevents registration from triggering a draw list
            /// update. Meant to be used in conjunction with pooled elements being
            /// unregistered/reregistered to the same parent.</param>
            void RemoveRange(int index, int count, bool fast = false);

            /// <summary>
            /// Sorts the entires using the default comparer.
            /// </summary>
            void Sort();

            /// <summary>
            /// Sorts the entries using the given comparer.
            /// </summary>
            void Sort(Func<TElementContainer, TElementContainer, int> comparison);
        }

        /// <summary>
        /// Read-only interface for UI elements that support child elments using containers.
        /// </summary>
        public interface IHudCollection<TElementContainer> : IHudCollection<TElementContainer, HudElementBase>
            where TElementContainer : IHudElementContainer<HudElementBase>, new()
        { }

        /// <summary>
        /// Read-only interface for UI elements that support child elments using containers.
        /// </summary>
        public interface IHudCollection : IHudCollection<HudElementContainer<HudElementBase>, HudElementBase>
        { }
    }
}
