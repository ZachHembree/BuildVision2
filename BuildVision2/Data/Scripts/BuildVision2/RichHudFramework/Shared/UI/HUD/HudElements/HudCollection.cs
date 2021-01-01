using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// A collection of UI elements wrapped in container objects. UI elements in the containers are parented
        /// to the collection, like any other HUD element.
        /// </summary>
        public class HudCollection<TElementContainer, TElement> : HudElementBase, IHudCollection<TElementContainer, TElement>
            where TElementContainer : IHudElementContainer<TElement>, new()
            where TElement : HudElementBase
        {
            /// <summary>
            /// UI elements in the collection
            /// </summary>
            public IReadOnlyList<TElementContainer> Collection => hudCollection;

            /// <summary>
            /// Used to allow the addition of child elements using collection-initializer syntax in
            /// conjunction with normal initializers.
            /// </summary>
            public HudCollection<TElementContainer, TElement> CollectionContainer => this;

            /// <summary>
            /// Retrieves the element container at the given index.
            /// </summary>
            public TElementContainer this[int index] => hudCollection[index];

            /// <summary>
            /// Returns the number of containers in the collection.
            /// </summary>
            public int Count => hudCollection.Count;

            /// <summary>
            /// Indicates whether the collection is read-only
            /// </summary>
            public bool IsReadOnly => false;

            /// <summary>
            /// UI elements in the chain
            /// </summary>
            protected readonly List<TElementContainer> hudCollection;

            /// <summary>
            /// Used internally by HUD collection for bulk entry removal
            /// </summary>
            protected bool fastRemove;

            public HudCollection(HudParentBase parent) : base(parent)
            {
                hudCollection = new List<TElementContainer>();
            }

            public HudCollection() : this(null)
            { }

            public IEnumerator<TElementContainer> GetEnumerator() =>
                hudCollection.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                GetEnumerator();

            /// <summary>
            /// Adds an element of type <see cref="TElement"/> to the chain.
            /// </summary>
            public void Add(TElement element) =>
                Add(new TElementContainer { Element = element });

            /// <summary>
            /// Adds an element of type <see cref="TElementContainer"/> to the chain.
            /// </summary>
            public void Add(TElementContainer container)
            {
                if (container.Element.Registered)
                    throw new Exception("HUD Element already registered!");

                if (container.Element.Register(this))
                    hudCollection.Add(container);
                else
                    throw new Exception("HUD Element registration failed.");
            }

            /// <summary>
            /// Add the given range to the end of the chain.
            /// </summary>
            public void AddRange(IReadOnlyList<TElementContainer> newContainers)
            {
                children.EnsureCapacity(children.Count + newContainers.Count);
                hudCollection.EnsureCapacity(hudCollection.Count + newContainers.Count);

                for (int n = 0; n < newContainers.Count; n++)
                {
                    if (newContainers[n].Element.Register(this))
                        hudCollection.Add(newContainers[n]);
                    else
                        throw new Exception("HUD Element registration failed.");
                }
            }

            /// <summary>
            /// Adds an element of type <see cref="TElementContainer"/> at the given index.
            /// </summary>
            public void Insert(int index, TElementContainer container)
            {
                if (container.Element.Register(this))
                    hudCollection.Insert(index, container);
                else
                    throw new Exception("HUD Element registration failed.");
            }

            /// <summary>
            /// Insert the given range into the chain.
            /// </summary>
            public void InsertRange(int index, IReadOnlyList<TElementContainer> newContainers)
            {
                children.EnsureCapacity(children.Count + newContainers.Count);
                hudCollection.EnsureCapacity(hudCollection.Count + newContainers.Count);

                for (int n = 0; n < newContainers.Count; n++)
                {
                    if (newContainers[n].Element.Register(this))
                        hudCollection.Add(newContainers[n]);
                    else
                        throw new Exception("HUD Element registration failed.");
                }

                hudCollection.InsertRange(index, newContainers);
            }

            /// <summary>
            /// Removes the specified element from the collection.
            /// </summary>
            /// <param name="fast">Prevents registration from triggering a draw list
            /// update. Meant to be used in conjunction with pooled elements being
            /// unregistered/reregistered to the same parent.</param>
            public bool Remove(TElementContainer entry) =>
                Remove(entry, false);

            /// <summary>
            /// Removes the specified element from the collection.
            /// </summary>
            /// <param name="fast">Prevents registration from triggering a draw list
            /// update. Meant to be used in conjunction with pooled elements being
            /// unregistered/reregistered to the same parent.</param>
            public bool Remove(TElementContainer entry, bool fast)
            {
                if (entry.Element.Parent == this && hudCollection.Count > 0)
                {
                    if (hudCollection.Remove(entry))
                        return entry.Element.Unregister(fast);
                }

                return false;
            }

            /// <summary>
            /// Removes the chain member that meets the conditions required by the predicate.
            /// </summary>
            /// <param name="fast">Prevents registration from triggering a draw list
            /// update. Meant to be used in conjunction with pooled elements being
            /// unregistered/reregistered to the same parent.</param>
            public bool Remove(Func<TElementContainer, bool> predicate, bool fast = false)
            {
                if (hudCollection.Count > 0)
                {
                    int index = hudCollection.FindIndex(x => predicate(x));
                    TElement element = hudCollection[index].Element;

                    if (index != -1 && hudCollection.Remove(hudCollection[index]))
                        return element.Unregister(fast);
                }

                return false;
            }

            /// <summary>
            /// Remove the element at the given index.
            /// </summary>
            /// <param name="fast">Prevents registration from triggering a draw list
            /// update. Meant to be used in conjunction with pooled elements being
            /// unregistered/reregistered to the same parent.</param>
            public bool RemoveAt(int index, bool fast = false)
            {
                if (hudCollection[index].Element.Parent == this && hudCollection.Count > 0)
                {
                    TElement element = hudCollection[index].Element;
                    hudCollection.RemoveAt(index);
                    fastRemove = true;

                    bool success = element.Unregister(fast);
                    fastRemove = false;

                    return success;
                }

                return false;
            }

            /// <summary>
            /// Removes the specfied range from the collection. Normal child elements not affected.
            /// </summary>
            /// <param name="fast">Prevents registration from triggering a draw list
            /// update. Meant to be used in conjunction with pooled elements being
            /// unregistered/reregistered to the same parent.</param>
            public void RemoveRange(int index, int count, bool fast = false)
            {
                int end = index + count;

                if (!(index >= 0 && count >= 0 && index < hudCollection.Count && end <= hudCollection.Count))
                    throw new Exception("Specified indices are out of range.");

                fastRemove = true;

                for (int n = index; n < end; n++)
                    hudCollection[n].Element.Unregister(fast);

                hudCollection.RemoveRange(index, count);
                fastRemove = false;
            }

            /// <summary>
            /// Remove all elements in the collection. Does not affect normal child elements.
            /// </summary>
            public void Clear() =>
                Clear(false);

            /// <summary>
            /// Remove all elements in the collection. Does not affect normal child elements.
            /// </summary>
            /// <param name="fast">Prevents registration from triggering a draw list
            /// update. Meant to be used in conjunction with pooled elements being
            /// unregistered/reregistered to the same parent.</param>
            public void Clear(bool fast)
            {
                fastRemove = true;

                for (int n = 0; n < hudCollection.Count; n++)
                    hudCollection[n].Element.Unregister(fast);

                hudCollection.Clear();
                fastRemove = false;
            }

            /// <summary>
            /// Finds the chain member that meets the conditions required by the predicate.
            /// </summary>
            public TElementContainer Find(Func<TElementContainer, bool> predicate)
            {
                return hudCollection.Find(x => predicate(x));
            }

            /// <summary>
            /// Finds the index of the chain member that meets the conditions required by the predicate.
            /// </summary>
            public int FindIndex(Func<TElementContainer, bool> predicate)
            {
                return hudCollection.FindIndex(x => predicate(x));
            }

            /// <summary>
            /// Sorts the entries using the given comparer.
            /// </summary>
            public void Sort(Func<TElementContainer, TElementContainer, int> comparison) =>
                hudCollection.Sort((x, y) => comparison(x, y));

            /// <summary>
            /// Sorts the entires using the default comparer.
            /// </summary>
            public void Sort() =>
                hudCollection.Sort();

            /// <summary>
            /// Returns true if the given element is in the collection.
            /// </summary>
            public bool Contains(TElementContainer item) =>
                hudCollection.Contains(item);

            /// <summary>
            /// Copies the contents of the collection to the given array starting at the index specified in the target array.
            /// </summary>
            public void CopyTo(TElementContainer[] array, int arrayIndex) =>
                hudCollection.CopyTo(array, arrayIndex);

            public override bool RemoveChild(HudNodeBase child, bool fast = false)
            {
                if (child.Parent == this)
                    return child.Unregister(fast);
                else if (child.Parent == null && children.Remove(child))
                {
                    if (!fastRemove)
                    {
                        for (int n = 0; n < hudCollection.Count; n++)
                        {
                            if (hudCollection[n].Element == child)
                            {
                                hudCollection.RemoveAt(n);
                                break;
                            }
                        }
                    }

                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// A collection of UI elements wrapped in container objects. UI elements in the containers are parented
        /// to the collection, like any other HUD element.
        /// </summary>
        public class HudCollection<TElementContainer> : HudCollection<TElementContainer, HudElementBase>
            where TElementContainer : IHudElementContainer<HudElementBase>, new()
        {
            public HudCollection(HudParentBase parent = null) : base(parent)
            { }
        }

        /// <summary>
        /// A collection of UI elements wrapped in container objects. UI elements in the containers are parented
        /// to the collection, like any other HUD element.
        /// </summary>
        public class HudCollection : HudCollection<HudElementContainer<HudElementBase>, HudElementBase>
        {
            public HudCollection(HudParentBase parent = null) : base(parent)
            { }
        }
    }
}
