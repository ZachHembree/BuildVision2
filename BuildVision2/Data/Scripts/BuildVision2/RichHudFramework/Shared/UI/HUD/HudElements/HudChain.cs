using System;
using System.ComponentModel;
using VRage;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Used to control sizing behavior of HudChain members and the containing chain element itself. The align axis
        /// is the axis chain elements are arranged on; the off axis is the other axis. When vertically aligned, Y is 
        /// the align axis and X is the off axis. Otherwise, it's reversed.
        /// </summary>
        public enum HudChainSizingModes : int
        {
            // Naming: [Clamp/Fit/Align]Members[OffAxis/AlignAxis/Both]
            // Fit > Clamp

            /// <summary>
            /// If this flag is set, then member size along the off axis will be allowed to vary freely, provided they
            /// fit inside the chain. For vertical chains, width will be clamped. For horizontal chains, height is clamped.
            /// </summary>
            ClampMembersOffAxis = 0x1,

            /// <summary>
            /// If this flag is set, member size will be set to be equal to the size of the chain on the off axis, less
            /// padding. For vertical chains, width will be matched. For horizontal chains, height is matched.
            /// </summary>
            FitMembersOffAxis = 0x2,

            /// <summary>
            /// Aligns the start of the chain to the left or top inner edge of the chain.
            /// </summary>
            AlignMembersStart = 0x4,

            /// <summary>
            /// Aligns the start of the chain to the right or bottom inner edge of the chain.
            /// </summary>
            AlignMembersEnd = 0x8,

            /// <summary>
            /// Aligns the start of the chain to the center of the chain.
            /// </summary>
            AlignMembersCenter = 0x10,
        }

        /// <summary>
        /// HUD element used to organize other elements into straight lines, either horizontal or vertical.
        /// </summary>
        /*
         Rules:
            1) Chain members must fit inside the chain. How this is accomplished depends on the sizing mode. Chain size
            and position is determined before Layout by parent nodes or on initialization. The chain resizes and positions
            its children, not itself.
            2) Members must be positioned within the chain's bounds.
            3) Members are assumed to be compatible with the specified sizing mode. Otherwise the behavior is undefined
            and incorrect positioning and sizing will occur.
        */
        public class HudChain<TElementContainer, TElement> : HudCollection<TElementContainer, TElement>
            where TElementContainer : IChainElementContainer<TElement>, new()
            where TElement : HudElementBase
        {
            protected const HudElementStates nodeSetVisible = HudElementStates.IsVisible | HudElementStates.IsRegistered;

            /// <summary>
            /// Used to allow the addition of child elements using collection-initializer syntax in
            /// conjunction with normal initializers.
            /// </summary>
            public new HudChain<TElementContainer, TElement> CollectionContainer => this;

            /// <summary>
            /// Distance between chain elements along their axis of alignment.
            /// </summary>
            public float Spacing { get; set; }

            /// <summary>
            /// Determines how/if the chain will attempt to resize member elements. Default sizing mode is 
            /// HudChainSizingModes.FitChainBoth.
            /// </summary>
            public HudChainSizingModes SizingMode { get; set; }

            /// <summary>
            /// Determines whether or not chain elements will be aligned vertically.
            /// </summary>
            public virtual bool AlignVertical 
            { 
                get { return _alignVertical; }
                set 
                {
                    if (value)
                    {
                        alignAxis = 1;
                        offAxis = 0;
                    }
                    else
                    {
                        alignAxis = 0;
                        offAxis = 1;
                    }

                    _alignVertical = value;
                }
            }

            protected bool _alignVertical;
            protected int alignAxis, offAxis;

            public HudChain(bool alignVertical, HudParentBase parent = null) : base(parent)
            {
                Init();

                Spacing = 0f;
                SizingMode = HudChainSizingModes.ClampMembersOffAxis;
                AlignVertical = alignVertical;
            }

            public HudChain(HudParentBase parent) : this(false, parent)
            { }

            public HudChain() : this(false, null)
            { }

            /// <summary>
            /// Initialzer called before the constructor.
            /// </summary>
            protected virtual void Init() { }

            /// <summary>
            /// Adds a UI element to the end of the chain.
            /// </summary>
            /// <param name="alignAxisScale">Scale of the element relative to the chain along the align axis, less padding and space
            /// required for other chain members. 0f == constant size (default); 1f = auto</param>
            /// <param name="preload"></param>
            public virtual void Add(TElement element, float alignAxisScale, bool preload = false)
            {
                var newContainer = new TElementContainer();
                newContainer.SetElement(element);
                newContainer.AlignAxisScale = alignAxisScale;
                Add(newContainer, preload);
            }

            public virtual Vector2 SetMemberSize(Vector2 newSize, int start = 0, int end = -1)
            {
                Vector2 listSize = Vector2.Zero;
                int visCount = 0;

                if (hudCollectionList.Count > 0)
                {
                    if (end == -1)
                        end = hudCollectionList.Count - 1;

                    for (int i = start; i <= end ; i++)
                    {
                        TElement element = hudCollectionList[i].Element;

                        if ((element.State & HudElementStates.IsVisible) > 0)
                        {
                            Vector2 elementSize = element.UnpaddedSize + element.Padding;

                            if (newSize[alignAxis] != 0)
                                elementSize[alignAxis] = newSize[alignAxis];

                            if (newSize[offAxis] != 0)
                                elementSize[offAxis] = newSize[offAxis];

                            element.UnpaddedSize = elementSize - element.Padding;
                            listSize[offAxis] = Math.Max(listSize[offAxis], elementSize[offAxis]);
                            listSize[alignAxis] += elementSize[alignAxis];
                            visCount++;
                        }
                    }

                    listSize[alignAxis] += Spacing * (visCount - 1);
                }

                return listSize + Padding;
            }

            /// <summary>
            /// Returns the most recent total size of the chain elements in the given range.
            /// </summary>
            public virtual Vector2 GetRangeSize(int start = 0, int end = -1)
            {
                Vector2 listSize = Vector2.Zero;
                int visCount = 0;

                if (hudCollectionList.Count > 0)
                {
                    if (end == -1)
                        end = hudCollectionList.Count - 1;

                    for (int i = start; i <= end; i++)
                    {
                        TElement element = hudCollectionList[i].Element;

                        if ((element.State & HudElementStates.IsVisible) > 0)
                        {
                            Vector2 elementSize = element.UnpaddedSize + element.Padding;
                            listSize[offAxis] = Math.Max(listSize[offAxis], elementSize[offAxis]);
                            listSize[alignAxis] += elementSize[alignAxis];
                            visCount++;
                        }
                    }

                    listSize[alignAxis] += Spacing * (visCount - 1);
                }

                return listSize + Padding;
            }

            protected override void Layout()
            {
                Vector2 chainSize = CachedSize - Padding;

                if (hudCollectionList.Count > 0 && (chainSize.X > 0f && chainSize.Y > 0f))
                {
                    float elementSpanLength;

                    if (TryGetVisibleRange(chainSize[alignAxis], chainSize[offAxis], out elementSpanLength))
                    {
                        // Find the start and end points of the span within the chain element
                        Vector2 startOffset = Vector2.Zero,
                            endOffset = Vector2.Zero;
                        float rcpSpanLength = 1f / Math.Max(elementSpanLength, 1E-6f);

                        elementSpanLength = Math.Min(elementSpanLength, chainSize[alignAxis]);

                        if (alignAxis == 1) // Vertical
                        {
                            if ((SizingMode & HudChainSizingModes.AlignMembersCenter) > 0)
                            {
                                startOffset.Y = .5f * elementSpanLength;
                                endOffset.Y = startOffset.Y - elementSpanLength;
                            }
                            else if ((SizingMode & HudChainSizingModes.AlignMembersEnd) > 0)
                            {
                                endOffset.Y = -.5f * chainSize.Y;
                                startOffset.Y = endOffset.Y + elementSpanLength;
                            }
                            else
                            {
                                startOffset.Y = .5f * chainSize.Y;
                                endOffset.Y = startOffset.Y - elementSpanLength;
                            }                            
                        }
                        else
                        {
                            if ((SizingMode & HudChainSizingModes.AlignMembersCenter) > 0)
                            {
                                startOffset.X = -.5f * elementSpanLength;
                                endOffset.X = startOffset.X + elementSpanLength;
                            }
                            else if ((SizingMode & HudChainSizingModes.AlignMembersEnd) > 0)
                            {
                                endOffset.X = .5f * chainSize.X;
                                startOffset.X = endOffset.X - elementSpanLength;
                            }
                            else
                            {
                                startOffset.X = -.5f * chainSize.X;
                                endOffset.X = startOffset.X + elementSpanLength;
                            }
                        }

                        UpdateMemberOffsets(startOffset, endOffset, rcpSpanLength);
                    }
                }
            }

            /// <summary>
            /// Finds the total number of elements visible as well as the total length of the span along the align axis.
            /// Returns false if no elements are visible.
            /// </summary>
            protected virtual bool TryGetVisibleRange(float alignAxisSize, float offAxisSize, out float elementSpanLength)
            {
                float totalScale = 0f,
                    constantSpanLength = 0f;
                int visCount = 0;

                elementSpanLength = 0f;

                for (int i = 0; i < hudCollectionList.Count; i++)
                {
                    TElementContainer container = hudCollectionList[i];

                    if ((container.Element.State & HudElementStates.IsVisible) > 0)
                    {
                        totalScale += container.AlignAxisScale;
                        visCount++;

                        if (container.AlignAxisScale == 0f)
                        {
                            Vector2 size = container.Element.UnpaddedSize + container.Element.Padding;
                            constantSpanLength += size[alignAxis];
                        }
                    }
                }

                if (visCount > 0)
                {
                    float totalSpacing = Spacing * (visCount - 1),
                        autoSizeLength = Math.Max(alignAxisSize - constantSpanLength - totalSpacing, 0f),
                        rcpTotalScale = Math.Min(1f / Math.Max(totalScale, 1f), 1f);

                    for (int i = 0; i < hudCollectionList.Count; i++)
                    {
                        TElementContainer container = hudCollectionList[i];
                        TElement element = container.Element;

                        if ((element.State & HudElementStates.IsVisible) > 0)
                        {
                            Vector2 size = element.UnpaddedSize + element.Padding;

                            if (container.AlignAxisScale != 0f && autoSizeLength > 0f)
                            {
                                float effectiveScale = container.AlignAxisScale * rcpTotalScale;
                                size[alignAxis] = autoSizeLength * effectiveScale;
                            }

                            // Update off axis size
                            if ((SizingMode & HudChainSizingModes.FitMembersOffAxis) > 0)
                                size[offAxis] = offAxisSize;
                            else if ((SizingMode & HudChainSizingModes.ClampMembersOffAxis) > 0)
                                size[offAxis] = Math.Min(size[offAxis], offAxisSize);

                            elementSpanLength += size[alignAxis];
                            element.UnpaddedSize = size - element.Padding;
                        }
                    }

                    elementSpanLength += totalSpacing;

                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Arrange chain members in a straight line
            /// </summary>
            protected void UpdateMemberOffsets(Vector2 startOffset, Vector2 endOffset, float rcpSpanLength)
            {
                ParentAlignments left = (ParentAlignments)((int)ParentAlignments.Left * (2 - alignAxis)),
                    right = (ParentAlignments)((int)ParentAlignments.Right * (2 - alignAxis)),
                    bitmask = left | right;
                float j = 0f, spacingInc = Spacing * rcpSpanLength;

                for (int i = 0; i < hudCollectionList.Count; i++)
                {
                    TElementContainer container = hudCollectionList[i];
                    TElement element = container.Element;

                    if ((element.State & HudElementStates.IsVisible) > 0)
                    {
                        Vector2 size = element.UnpaddedSize + element.Padding;
                        
                        // Enforce alignment restrictions
                        element.ParentAlignment &= bitmask;
                        element.ParentAlignment |= ParentAlignments.Inner | ParentAlignments.UsePadding;

                        float increment = size[alignAxis] * rcpSpanLength;
                        element.Offset = Vector2.Lerp(startOffset, endOffset, j + (.5f * increment));

                        j += increment + spacingInc;
                    }
                }
            }
        }

        /// <summary>
        /// HUD element used to organize other elements into straight lines, either horizontal or vertical. Min/Max size
        /// determines the minimum and maximum size of chain members.
        /// </summary>
        public class HudChain<TElementContainer> : HudChain<TElementContainer, HudElementBase>
            where TElementContainer : IChainElementContainer<HudElementBase>, new()
        {
            public HudChain(bool alignVertical, HudParentBase parent = null) : base(alignVertical, parent)
            { }

            public HudChain(HudParentBase parent) : base(true, parent)
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

            public HudChain(HudParentBase parent) : base(true, parent)
            { }
        }
    }
}
