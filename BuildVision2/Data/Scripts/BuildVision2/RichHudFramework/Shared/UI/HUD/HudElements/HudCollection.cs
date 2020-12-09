using System;
using System.Collections;
using System.Collections.Generic;
using VRage;
using VRageMath;
using HudDrawDelegate = System.Func<object, object>;
using HudLayoutDelegate = System.Func<bool, bool>;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework
{
    using HudInputDelegate = Func<Vector3, HudSpaceDelegate, MyTuple<Vector3, HudSpaceDelegate>>;

    namespace UI
    {
        using HudUpdateAccessors = MyTuple<
            ushort, // ZOffset
            byte, // Depth
            HudInputDelegate, // DepthTest
            HudInputDelegate, // HandleInput
            HudLayoutDelegate, // BeforeLayout
            HudDrawDelegate // BeforeDraw
        >;

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
            public IReadOnlyList<TElementContainer> ChainEntries => chainElements;

            /// <summary>
            /// Used to allow the addition of child elements using collection-initializer syntax in
            /// conjunction with normal initializers.
            /// </summary>
            public HudCollection<TElementContainer, TElement> ChainContainer => this;

            /// <summary>
            /// Retrieves the element container at the given index.
            /// </summary>
            public TElementContainer this[int index] => chainElements[index];

            /// <summary>
            /// Returns the number of containers in the collection.
            /// </summary>
            public int Count => chainElements.Count;

            /// <summary>
            /// Indicates whether the collection is read-only
            /// </summary>
            public bool IsReadOnly => false;

            /// <summary>
            /// UI elements in the chain
            /// </summary>
            protected readonly List<TElementContainer> chainElements;

            public HudCollection(HudParentBase parent = null) : base(parent)
            {
                chainElements = new List<TElementContainer>();
            }

            public IEnumerator<TElementContainer> GetEnumerator() =>
                chainElements.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                GetEnumerator();

            /// <summary>
            /// Adds an element of type <see cref="TElement"/> to the chain.
            /// </summary>
            public virtual void Add(TElement element) =>
                Add(new TElementContainer { Element = element });

            /// <summary>
            /// Adds an element of type <see cref="TElementContainer"/> to the chain.
            /// </summary>
            public void Add(TElementContainer container)
            {
                blockChildRegistration = true;

                if (container.Element.Parent == this)
                    throw new Exception("HUD Element already registered.");

                container.Element.Register(this);

                if (container.Element.Parent != this)
                    throw new Exception("HUD Element registration failed.");

                chainElements.Add(container);

                blockChildRegistration = false;
            }

            /// <summary>
            /// Add the given range to the end of the chain.
            /// </summary>
            public void AddRange(IReadOnlyList<TElementContainer> newChainEntries)
            {
                blockChildRegistration = true;

                for (int n = 0; n < newChainEntries.Count; n++)
                {
                    if (newChainEntries[n].Element.Parent == this)
                        throw new Exception("HUD Element already registered.");

                    newChainEntries[n].Element.Register(this);

                    if (newChainEntries[n].Element.Parent != this)
                        throw new Exception("HUD Element registration failed.");
                }

                chainElements.AddRange(newChainEntries);
                blockChildRegistration = false;
            }

            /// <summary>
            /// Adds an element of type <see cref="TElement"/> at the given index.
            /// </summary>
            public void Insert(int index, TElement element) =>
                Insert(index, new TElementContainer { Element = element });

            /// <summary>
            /// Adds an element of type <see cref="TElementContainer"/> at the given index.
            /// </summary>
            public void Insert(int index, TElementContainer container)
            {
                blockChildRegistration = true;

                if (container.Element.Parent == this)
                    throw new Exception("HUD Element already registered.");

                container.Element.Register(this);

                if (container.Element.Parent != this)
                    throw new Exception("HUD Element registration failed.");

                chainElements.Insert(index, container);

                blockChildRegistration = false;
            }

            /// <summary>
            /// Insert the given range into the chain.
            /// </summary>
            public void InsertRange(int index, IReadOnlyList<TElementContainer> newChainEntries)
            {
                blockChildRegistration = true;

                for (int n = 0; n < newChainEntries.Count; n++)
                {
                    if (newChainEntries[n].Element.Parent == this)
                        throw new Exception("HUD Element already registered.");

                    newChainEntries[n].Element.Register(this);

                    if (newChainEntries[n].Element.Parent != this)
                        throw new Exception("HUD Element registration failed.");
                }

                chainElements.InsertRange(index, newChainEntries);
                blockChildRegistration = false;
            }

            /// <summary>
            /// Removes the specified element from the chain.
            /// </summary>
            public void Remove(TElement chainElement)
            {
                if (chainElement.Parent == this && chainElements.Count > 0)
                {
                    chainElement.Unregister();
                }
            }

            /// <summary>
            /// Removes the specified element from the collection.
            /// </summary>
            public bool Remove(TElementContainer entry)
            {
                if (entry.Element.Parent == this && chainElements.Count > 0)
                {
                    entry.Element.Unregister();

                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Removes the member that meets the conditions required by the predicate.
            /// </summary>
            public void Remove(Func<TElement, bool> predicate)
            {
                if (chainElements.Count > 0)
                {
                    int index = chainElements.FindIndex(x => predicate(x.Element));

                    if (index != -1)
                        chainElements[index].Element.Unregister();
                }
            }

            /// <summary>
            /// Removes the chain member that meets the conditions required by the predicate.
            /// </summary>
            public void Remove(Func<TElementContainer, bool> predicate)
            {
                if (chainElements.Count > 0)
                {
                    int index = chainElements.FindIndex(x => predicate(x));

                    if (index != -1)
                        chainElements[index].Element.Unregister();
                }
            }

            /// <summary>
            /// Remove the element at the given index.
            /// </summary>
            public void RemoveAt(int index)
            {
                if (chainElements[index].Element.Parent == this && chainElements.Count > 0)
                {
                    blockChildRegistration = true;

                    chainElements[index].Element.Unregister();
                    chainElements.RemoveAt(index);

                    blockChildRegistration = false;
                }
            }

            /// <summary>
            /// Removes the specfied range from the collection. Normal child elements not affected.
            /// </summary>
            public void RemoveRange(int index, int count)
            {
                blockChildRegistration = true;

                for (int n = index; n < index + count; n++)
                    chainElements[n].Element.Unregister();

                chainElements.RemoveRange(index, count);
                blockChildRegistration = false;
            }

            /// <summary>
            /// Remove all elements in the collection. Does not affect normal child elements.
            /// </summary>
            public void Clear()
            {
                blockChildRegistration = true;

                for (int n = 0; n < chainElements.Count; n++)
                    chainElements[n].Element.Unregister();

                chainElements.Clear();
                blockChildRegistration = false;
            }

            /// <summary>
            /// Finds the chain member that meets the conditions required by the predicate.
            /// </summary>
            public TElementContainer Find(Func<TElementContainer, bool> predicate)
            {
                return chainElements.Find(x => predicate(x));
            }

            /// <summary>
            /// Finds the index of the chain member that meets the conditions required by the predicate.
            /// </summary>
            public int FindIndex(Func<TElementContainer, bool> predicate)
            {
                return chainElements.FindIndex(x => predicate(x));
            }

            public override void RemoveChild(HudNodeBase child)
            {
                if (!blockChildRegistration)
                {
                    int index = children.FindIndex(x => x == child);

                    if (index != -1)
                    {
                        if (children[index].Parent == child)
                            children[index].Unregister();
                        else if (children[index].Parent == null)
                            children.RemoveAt(index);
                    }
                    else
                    {
                        index = chainElements.FindIndex(x => x.Element == child);

                        if (index != -1)
                        {
                            if (chainElements[index].Element.Parent == child)
                                chainElements[index].Element.Unregister();
                            else if (chainElements[index].Element.Parent == null)
                                chainElements.RemoveAt(index);
                        }
                    }
                }
            }

            /// <summary>
            /// Sorts the entries using the given comparer.
            /// </summary>
            public void Sort(Func<TElementContainer, TElementContainer, int> comparison) =>
                chainElements.Sort((x, y) => comparison(x, y));

            /// <summary>
            /// Sorts the entires using the default comparer.
            /// </summary>
            public void Sort() =>
                chainElements.Sort();

            /// <summary>
            /// Returns true if the given element is in the collection.
            /// </summary>
            public bool Contains(TElementContainer item) =>
                chainElements.Contains(item);

            /// <summary>
            /// Copies the contents of the collection to the given array starting at the index specified in the target array.
            /// </summary>
            public void CopyTo(TElementContainer[] array, int arrayIndex) =>
                chainElements.CopyTo(array, arrayIndex);

            public override void GetUpdateAccessors(List<HudUpdateAccessors> DrawActions, byte treeDepth)
            {
                fullZOffset = GetFullZOffset(this, _parent);

                DrawActions.EnsureCapacity(DrawActions.Count + children.Count + chainElements.Count + 1);
                DrawActions.Add(new HudUpdateAccessors(fullZOffset, treeDepth, DepthTestAction, InputAction, LayoutAction, DrawAction));

                treeDepth++;

                for (int n = 0; n < chainElements.Count; n++)
                {
                    chainElements[n].Element.GetUpdateAccessors(DrawActions, treeDepth);
                }

                for (int n = 0; n < children.Count; n++)
                {
                    children[n].GetUpdateAccessors(DrawActions, treeDepth);
                }
            }
        }
    }
}
