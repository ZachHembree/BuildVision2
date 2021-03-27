using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
    using UI.Server;

    namespace UI
    {
        /// <summary>
        /// A collection of UI elements wrapped in container objects. UI elements in the containers are parented
        /// to the collection, like any other HUD element.
        /// </summary>
        public class HudCollection<TElementContainer, TElement> : HudElementBase, IHudCollection<TElementContainer, TElement>
            where TElementContainer : IHudElementContainer<TElement>, new()
            where TElement : HudNodeBase
        {
            /// <summary>
            /// UI elements in the collection
            /// </summary>
            public IReadOnlyList<TElementContainer> Collection => hudCollectionList;

            /// <summary>
            /// Used to allow the addition of child elements using collection-initializer syntax in
            /// conjunction with normal initializers.
            /// </summary>
            public HudCollection<TElementContainer, TElement> CollectionContainer => this;

            /// <summary>
            /// Retrieves the element container at the given index.
            /// </summary>
            public TElementContainer this[int index] => hudCollectionList[index];

            /// <summary>
            /// Returns the number of containers in the collection.
            /// </summary>
            int IReadOnlyCollection<TElementContainer>.Count => hudCollectionList.Count;

            /// <summary>
            /// Returns the number of containers in the collection.
            /// </summary>
            public int Count => hudCollectionList.Count;

            /// <summary>
            /// Indicates whether the collection is read-only
            /// </summary>
            public bool IsReadOnly => false;

            /// <summary>
            /// UI elements in the chain
            /// </summary>
            protected readonly List<TElementContainer> hudCollectionList;

            /// <summary>
            /// Used internally by HUD collection for bulk entry removal
            /// </summary>
            protected bool skipCollectionRemove;

            public HudCollection(HudParentBase parent) : base(parent)
            {
                hudCollectionList = new List<TElementContainer>();
            }

            public HudCollection() : this(null)
            { }

            public IEnumerator<TElementContainer> GetEnumerator() =>
                hudCollectionList.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                GetEnumerator();

            /// <summary>
            /// Adds an element of type <see cref="TElement"/> to the chain.
            /// </summary>
            public void Add(TElement element)
            {
                var newContainer = new TElementContainer();
                newContainer.SetElement(element);
                Add(newContainer);
            }

            /// <summary>
            /// Adds an element of type <see cref="TElementContainer"/> to the chain.
            /// </summary>
            public void Add(TElementContainer container)
            {
                if (container.Element.Registered)
                    throw new Exception("HUD Element already registered!");

                if (container.Element.Register(this, false))
                    hudCollectionList.Add(container);
                else
                    throw new Exception("HUD Element registration failed.");
            }

            /// <summary>
            /// Add the given range to the end of the chain.
            /// </summary>
            public void AddRange(IReadOnlyList<TElementContainer> newContainers)
            {
                NodeUtils.RegisterNodes<TElementContainer, TElement>(this, children, newContainers, false, false);
                hudCollectionList.AddRange(newContainers);
            }

            /// <summary>
            /// Adds an element of type <see cref="TElementContainer"/> at the given index.
            /// </summary>
            public void Insert(int index, TElementContainer container)
            {
                if (container.Element.Register(this, false))
                    hudCollectionList.Insert(index, container);
                else
                    throw new Exception("HUD Element registration failed.");
            }

            /// <summary>
            /// Insert the given range into the chain.
            /// </summary>
            public void InsertRange(int index, IReadOnlyList<TElementContainer> newContainers)
            {
                NodeUtils.RegisterNodes<TElementContainer, TElement>(this, children, newContainers, false, false);
                hudCollectionList.InsertRange(index, newContainers);
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
                if (entry.Element.Parent == this && hudCollectionList.Count > 0)
                {
                    if (hudCollectionList.Remove(entry))
                    {
                        skipCollectionRemove = !fast;
                        bool success = entry.Element.Unregister(fast);

                        return success;
                    }
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
                if (hudCollectionList.Count > 0)
                {
                    int index = hudCollectionList.FindIndex(x => predicate(x));
                    TElement element = hudCollectionList[index].Element;
                    bool success = false;

                    if (index != -1 && index < hudCollectionList.Count)
                    {
                        skipCollectionRemove = !fast;
                        hudCollectionList.RemoveAt(index);
                        success = element.Unregister(fast);
                    }

                    return success;
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
                if (hudCollectionList[index].Element.Parent == this && hudCollectionList.Count > 0)
                {
                    TElement element = hudCollectionList[index].Element;
                    hudCollectionList.RemoveAt(index);

                    skipCollectionRemove = !fast;
                    bool success = element.Unregister(fast);

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
                NodeUtils.UnregisterNodes<TElementContainer, TElement>(this, children, hudCollectionList, index, count, fast);
                hudCollectionList.RemoveRange(index, count);
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
                NodeUtils.UnregisterNodes<TElementContainer, TElement>(this, children, hudCollectionList, 0, hudCollectionList.Count, fast);
                hudCollectionList.Clear();
            }

            /// <summary>
            /// Finds the chain member that meets the conditions required by the predicate.
            /// </summary>
            public TElementContainer Find(Func<TElementContainer, bool> predicate)
            {
                return hudCollectionList.Find(x => predicate(x));
            }

            /// <summary>
            /// Finds the index of the chain member that meets the conditions required by the predicate.
            /// </summary>
            public int FindIndex(Func<TElementContainer, bool> predicate)
            {
                return hudCollectionList.FindIndex(x => predicate(x));
            }

            /// <summary>
            /// Returns true if the given element is in the collection.
            /// </summary>
            public bool Contains(TElementContainer item) =>
                hudCollectionList.Contains(item);

            /// <summary>
            /// Copies the contents of the collection to the given array starting at the index specified in the target array.
            /// </summary>
            public void CopyTo(TElementContainer[] array, int arrayIndex) =>
                hudCollectionList.CopyTo(array, arrayIndex);

            public override bool RemoveChild(HudNodeBase child, bool fast = false)
            {
                if (child.Parent == this)
                {
                    bool success = child.Unregister(fast);

                    if (success && fast)
                        RemoveChild(child);

                    return success;
                }
                else if (child.Parent == null && children.Remove(child))
                {
                    if (!skipCollectionRemove)
                    {
                        for (int n = 0; n < hudCollectionList.Count; n++)
                        {
                            if (hudCollectionList[n].Element == child)
                            {
                                hudCollectionList.RemoveAt(n);
                                break;
                            }
                        }
                    }
                    else
                        skipCollectionRemove = false;

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
        public class HudCollection<TElementContainer> : HudCollection<TElementContainer, HudNodeBase>
            where TElementContainer : IHudElementContainer<HudNodeBase>, new()
        {
            public HudCollection(HudParentBase parent = null) : base(parent)
            { }
        }

        /// <summary>
        /// A collection of UI elements wrapped in container objects. UI elements in the containers are parented
        /// to the collection, like any other HUD element.
        /// </summary>
        public class HudCollection : HudCollection<HudElementContainer<HudNodeBase>, HudNodeBase>
        {
            public HudCollection(HudParentBase parent = null) : base(parent)
            { }
        }
    }
}
