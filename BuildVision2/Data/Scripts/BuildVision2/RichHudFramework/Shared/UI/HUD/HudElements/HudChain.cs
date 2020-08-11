using System.Collections.Generic;
using VRageMath;
using VRage;
using System;
using System.Collections;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;
using HudLayoutDelegate = System.Func<bool, bool>;
using HudDrawDelegate = System.Func<object, object>;

namespace RichHudFramework
{
    using HudInputDelegate = Func<Vector3, HudSpaceDelegate, MyTuple<Vector3, HudSpaceDelegate>>;

    namespace UI
    {
        using Server;
        using Client;
        using HudUpdateAccessors = MyTuple<
            ushort, // ZOffset
            byte, // Depth
            HudInputDelegate, // DepthTest
            HudInputDelegate, // HandleInput
            HudLayoutDelegate, // BeforeLayout
            HudDrawDelegate // BeforeDraw
        >;

        /// <summary>
        /// Used to control sizing behavior of HudChain members and the containing chain element itself. The align axis
        /// is the axis chain elements are arranged on; the off axis is the other axis. When vertically aligned, Y is 
        /// the align axis and X is the off axis. Otherwise, it's reversed.
        /// </summary>
        public enum HudChainSizingModes : int
        {
            // Naming: [Clamp/Fit][Chain/Members][OffAxis/AlignAxis/Both]
            // Fit > Clamp

            // Chain Sizing

            /// <summary>
            /// In this mode, the size of the chain on it's off axis will be allowed to vary freely so long as it
            /// is large enough to contain its members on that axis.
            /// </summary>
            ClampChainOffAxis = 0x1,

            /// <summary>
            /// In this mode, the size of the chain on it's align axis will be allowed to vary freely so long as it
            /// is large enough to contain its members on that axis.
            /// </summary>
            ClampChainAlignAxis = 0x2,

            /// <summary>
            /// In this mode, the chain's size will be allowed to vary freely so long as it remains large enough
            /// to contain them.
            /// </summary>
            ClampChainBoth = ClampChainOffAxis | ClampChainAlignAxis,

            /// <summary>
            /// In this mode, the element will automatically shrink/expand to fit its contents on its off axis.
            /// Supercedes ClampChainOffAxis.
            /// </summary>
            FitChainOffAxis = 0x4,

            /// <summary>
            /// In this mode, the element will automatically shrink/expand to fit its contents on its align axis.
            /// Supercedes ClampChainAlignAxis.
            /// </summary>
            FitChainAlignAxis = 0x8,

            /// <summary>
            /// In this mode, the element will automatically shrink/expand to fit its contents.
            /// Supercedes ClampChainBoth.
            /// </summary>
            FitChainBoth = FitChainOffAxis | FitChainAlignAxis,

            // Member Sizing

            /// <summary>
            /// If this flag is set, then the size of chain members on the off axis will be clamped. 
            /// </summary>
            ClampMembersOffAxis = 0x10,

            /// <summary>
            /// If this flag is set, then the size of chain members on the align axis will be clamped. 
            /// </summary>
            ClampMembersAlignAxis = 0x20,

            /// <summary>
            /// In this mode, chain members will be clamped between the set min/max size on both axes. Superceeds FitToMembers.
            /// </summary>
            ClampMembersBoth = ClampMembersOffAxis | ClampMembersAlignAxis,

            /// <summary>
            /// If this flag is set, chain members will be automatically resized to fill the chain along the off axis. 
            /// Superceeds ClampMembersOffAxis.
            /// </summary>
            FitMembersOffAxis = 0x40,

            /// <summary>
            /// If this flag is set, then the size of chain members on the align axis will be set to the maximum size. 
            /// Superceeds ClampMembersAlignAxis.
            /// </summary>
            FitMembersAlignAxis = 0x80,

            /// <summary>
            /// In this mode, chain members will be set to the maximum size on both axes. Superceeds ClampMembersBoth.
            /// </summary>
            FitMembersBoth = FitMembersOffAxis | FitMembersAlignAxis,
        }

        /// <summary>
        /// HUD element used to organize other elements into straight lines, either horizontal or vertical. Min/Max size
        /// determines the minimum and maximum size of chain members.
        /// </summary>
        /*
         Rules:
            1) Chain members must fit inside the chain. How this is accomplished depends on the sizing mode.
            2) Members must be positioned within the chain's bounds.
            3) Members are assumed to be compatible with the specified sizing mode. Otherwise the behavior is undefined
            and incorrect positioning and sizing will occur.
         */
        public class HudChain<TElementContainer, TElement> : HudElementBase, IEnumerable<TElementContainer>
            where TElementContainer : IHudElementContainer<TElement>, new()
            where TElement : HudElementBase
        {
            /// <summary>
            /// UI elements in the chain
            /// </summary>
            public IReadOnlyList<TElementContainer> ChainEntries => chainElements;

            /// <summary>
            /// Used to allow the addition of child elements using collection-initializer syntax in
            /// conjunction with normal initializers.
            /// </summary>
            public HudChain<TElementContainer, TElement> ChainContainer => this;

            /// <summary>
            /// Width of the chain
            /// </summary>
            public override float Width
            {
                set
                {
                    if (value > Padding.X)
                        value -= Padding.X;

                    _absoluteWidth = value / Scale;

                    if (offAxis == 0 && (SizingMode & (HudChainSizingModes.ClampMembersOffAxis | HudChainSizingModes.FitMembersOffAxis)) > 0)
                        _absMaxSize.X = _absoluteWidth;
                }
            }

            /// <summary>
            /// Height of the chain
            /// </summary>
            public override float Height
            {
                set
                {
                    if (value > Padding.Y)
                        value -= Padding.Y;

                    _absoluteHeight = value / Scale;

                    if (offAxis == 1 && (SizingMode & (HudChainSizingModes.ClampMembersOffAxis | HudChainSizingModes.FitMembersOffAxis)) > 0)
                        _absMaxSize.Y = _absoluteHeight;
                }
            }

            /// <summary>
            /// Maximum chain member size. If no maximum is set, then the currently set size will be used as the maximum.
            /// </summary>
            public Vector2 MemberMaxSize { get { return _absMaxSize * Scale; } set { _absMaxSize = value / Scale; } }

            /// <summary>
            /// Minimum allowable member size.
            /// </summary>
            public Vector2 MemberMinSize { get { return _absMinSize * Scale; } set { _absMinSize = value / Scale; } }

            /// <summary>
            /// Distance between chain elements along their axis of alignment.
            /// </summary>
            public float Spacing { get { return _spacing * Scale; } set { _spacing = value / Scale; } }

            /// <summary>
            /// Determines how/if the chain will attempt to resize member elements. Default sizing mode is 
            /// HudChainSizingModes.FitChainBoth.
            /// </summary>
            public HudChainSizingModes SizingMode { get; set; }

            /// <summary>
            /// Determines whether or not chain elements will be aligned vertically.
            /// </summary>
            public bool AlignVertical => alignAxis == 1;

            /// <summary>
            /// UI elements in the chain
            /// </summary>
            protected readonly List<TElementContainer> chainElements;

            protected float _spacing;
            protected int alignAxis, offAxis;
            protected Vector2 _absMaxSize, _absMinSize;

            public HudChain(bool alignVertical, HudParentBase parent = null) : base(parent)
            {
                Spacing = 0f;
                chainElements = new List<TElementContainer>();
                SizingMode = HudChainSizingModes.FitChainBoth;

                if (alignVertical)
                {
                    alignAxis = 1;
                    offAxis = 0;
                }
                else
                {
                    alignAxis = 0;
                    offAxis = 1;
                }
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
                if (chainElement.Parent == this)
                {
                    int index = chainElements.FindIndex(x => x.Element == chainElement);

                    if (index != -1)
                    {
                        chainElement.Unregister();
                        chainElements.RemoveAt(index);
                    }
                }
            }

            /// <summary>
            /// Removes the specified element from the chain.
            /// </summary>
            public void Remove(TElementContainer entry)
            {
                if (entry.Element.Parent == this)
                {
                    int index = chainElements.FindIndex(x => x.Equals(entry));

                    if (index != -1)
                    {
                        entry.Element.Unregister();
                        chainElements.RemoveAt(index);
                    }
                }
            }

            /// <summary>
            /// Removes the chain member that meets the conditions required by the predicate.
            /// </summary>
            public void Remove(Func<TElement, bool> predicate)
            {
                int index = chainElements.FindIndex(x => predicate(x.Element));
                RemoveAt(index);
            }

            /// <summary>
            /// Removes the chain member that meets the conditions required by the predicate.
            /// </summary>
            public void Remove(Func<TElementContainer, bool> predicate)
            {
                int index = chainElements.FindIndex(x => predicate(x));
                RemoveAt(index);
            }

            /// <summary>
            /// Remove the chain element at the given index.
            /// </summary>
            public void RemoveAt(int index)
            {
                if (chainElements[index].Element.Parent == this)
                {
                    blockChildRegistration = true;

                    chainElements[index].Element.Unregister();
                    chainElements.RemoveAt(index);

                    blockChildRegistration = false;
                }
            }

            /// <summary>
            /// Removes the specfied range from the chain. Normal child elements not affected.
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
            /// Remove all elements in the HudChain. Does not affect normal child elements.
            /// </summary>
            public void ClearChain()
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

            protected override void Layout()
            {
                ClampElementSizeRange();
                UpdateMemberSizes();

                Vector2 visibleTotalSize = GetVisibleTotalSize(),
                    newSize = GetNewSize(cachedSize - cachedPadding, visibleTotalSize);

                cachedSize = newSize;
                _absoluteWidth = cachedSize.X / Scale;
                _absoluteHeight = cachedSize.Y / Scale;
                cachedSize += cachedPadding;

                // Calculate member start offset
                Vector2 startOffset = new Vector2();

                if (alignAxis == 1)
                    startOffset.Y = newSize.Y / 2f;
                else
                    startOffset.X = -newSize.X / 2f;

                UpdateMemberOffsets(startOffset, cachedPadding);
            }

            /// <summary>
            /// Clamps minimum and maximum element sizes s.t min < max and both are always
            /// greater than Vector2.Zero
            /// </summary>
            protected void ClampElementSizeRange()
            {
                _absMinSize = Vector2.Max(Vector2.Zero, _absMinSize);
                _absMaxSize = Vector2.Max(Vector2.Zero, _absMaxSize);

                Vector2 newMin, newMax;
                newMin = Vector2.Min(_absMinSize, _absMaxSize);
                newMax = Vector2.Max(_absMinSize, _absMaxSize);

                _absMinSize = newMin;
                _absMaxSize = newMax;
            }

            /// <summary>
            /// Calculates the chain's current size based on its sizing mode and the total
            /// size of its members (less padding).
            /// </summary>
            protected Vector2 GetNewSize(Vector2 lastSize, Vector2 totalSize)
            {
                if ((SizingMode & HudChainSizingModes.FitChainAlignAxis) > 0)
                {
                    lastSize[alignAxis] = totalSize[alignAxis];
                }
                else // if ClampChainAlignAxis
                {
                    lastSize[alignAxis] = Math.Max(lastSize[alignAxis], totalSize[alignAxis]);
                }

                if ((SizingMode & HudChainSizingModes.FitChainOffAxis) > 0)
                {
                    lastSize[offAxis] = totalSize[offAxis];
                }
                else // if ClampChainOffAxis
                {
                    lastSize[offAxis] = Math.Max(lastSize[offAxis], totalSize[offAxis]);
                }

                return lastSize;
            }

            /// <summary>
            /// Updates chain member offsets to ensure that they're in a straight line.
            /// </summary>
            protected void UpdateMemberOffsets(Vector2 offset, Vector2 padding)
            {
                Vector2 alignMask = new Vector2(offAxis, -alignAxis), offMask = new Vector2(alignAxis, -offAxis);
                ParentAlignments left = (ParentAlignments)((int)ParentAlignments.Left * (2 - alignAxis)),
                    right = (ParentAlignments)((int)ParentAlignments.Right * (2 - alignAxis)),
                    bitmask = left | right;

                for (int n = 0; n < chainElements.Count; n++)
                {
                    TElement element = chainElements[n].Element;

                    if (element.Visible)
                    {
                        // Calculate element size
                        Vector2 elementSize = element.Size;

                        // Enforce alignment restrictions
                        element.ParentAlignment &= bitmask;
                        element.ParentAlignment |= ParentAlignments.Inner;

                        // Calculate element offset
                        Vector2 newOffset = offset + (elementSize * alignMask * .5f);

                        if ((element.ParentAlignment & left) == left)
                        {
                            newOffset += padding * offMask * .5f;
                        }
                        else if ((element.ParentAlignment & right) == right)
                        {
                            newOffset -= padding * offMask * .5f;
                        }

                        // Apply changes
                        element.Offset = newOffset;

                        // Move offset down for the next element
                        elementSize[alignAxis] += Spacing;
                        offset += elementSize * alignMask;
                    }
                }
            }

            /// <summary>
            /// Updates chain member sizes to conform to sizing rules.
            /// </summary>
            protected void UpdateMemberSizes()
            {
                Vector2 minSize = MemberMinSize,
                    maxSize = MemberMaxSize;

                for (int n = 0; n < chainElements.Count; n++)
                {
                    TElement element = chainElements[n].Element;

                    if (element.Visible)
                    {
                        Vector2 elementSize = element.Size;

                        // Adjust element size based on sizing mode
                        if ((SizingMode & HudChainSizingModes.FitMembersOffAxis) > 0)
                            elementSize[offAxis] = maxSize[offAxis];
                        else if ((SizingMode & HudChainSizingModes.ClampMembersOffAxis) > 0)
                            elementSize[offAxis] = MathHelper.Clamp(elementSize[offAxis], minSize[offAxis], maxSize[offAxis]);

                        if ((SizingMode & HudChainSizingModes.FitMembersAlignAxis) > 0)
                            elementSize[alignAxis] = maxSize[alignAxis];
                        else if ((SizingMode & HudChainSizingModes.ClampMembersAlignAxis) > 0)
                            elementSize[alignAxis] = MathHelper.Clamp(elementSize[alignAxis], minSize[alignAxis], maxSize[alignAxis]);

                        element.Size = elementSize;
                    }
                }
            }

            /// <summary>
            /// Calculates the total size of all visible elements in the chain, including spacing and
            /// any resizing that might be required.
            /// </summary>
            protected Vector2 GetVisibleTotalSize()
            {
                Vector2 newSize = new Vector2();

                for (int n = 0; n < chainElements.Count; n++)
                {
                    TElement element = chainElements[n].Element;

                    if (element.Visible)
                    {
                        Vector2 elementSize = element.Size;

                        // Total up the size of elements on the axis of alignment
                        newSize[alignAxis] += elementSize[alignAxis];

                        // Find largest element on the off axis
                        if (elementSize[offAxis] > newSize[offAxis])
                            newSize[offAxis] = elementSize[offAxis];

                        newSize[alignAxis] += Spacing;
                    }
                }

                newSize[alignAxis] -= Spacing;
                return Vector2.Max(newSize, Vector2.Zero);
            }
        }

        /// <summary>
        /// HUD element used to organize other elements into straight lines, either horizontal or vertical. Min/Max size
        /// determines the minimum and maximum size of chain members.
        /// </summary>
        public class HudChain<TElementContainer> : HudChain<TElementContainer, HudElementBase>
            where TElementContainer : IHudElementContainer<HudElementBase>, new()
        {
            public HudChain(bool alignVertical, HudParentBase parent = null) : base(alignVertical, parent)
            { }
        }

        /// <summary>
        /// HUD element used to organize other elements into straight lines, either horizontal or vertical. Min/Max size
        /// determines the minimum and maximum size of chain members.
        /// </summary>
        public class HudChain : HudChain<HudElementContainer<HudElementBase>, HudElementBase>
        {
            public HudChain(bool alignVertical, HudParentBase parent = null) : base(alignVertical, parent)
            { }
        }
    }
}
