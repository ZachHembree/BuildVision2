using System;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        using static NodeConfigIndices;

        /// <summary>
        /// Controls the sizing behavior of the <see cref="HudChain{TElementContainer, TElement}"/> and its members. 
        /// The "Align Axis" is the direction the elements are stacked (Vertical = Y, Horizontal = X).
        /// The "Off Axis" is perpendicular to the stack (Vertical = X, Horizontal = Y).
        /// </summary>
        [Flags]
        public enum HudChainSizingModes : ushort
        {
            // Naming: [Clamp/Fit/Align][Chain/Members][OffAxis/AlignAxis/Both]
            // Fit supersedes Clamp
            // Chain Sizing

            /// <summary>
            /// Disables chain-self sizing and member size constraints, other than per-member proportional scaling.
            /// </summary>
            None = 0,

            /// <summary>
            /// Allows the chain's size on the off axis (width if vertical, height if horizontal) to vary, 
            /// expanding only enough to contain its widest/tallest member.
            /// </summary>
            ClampChainOffAxis = 1 << 2,

            /// <summary>
            /// Allows the chain's size on the align axis (height if vertical, width if horizontal) to vary, 
            /// expanding only enough to contain all members plus spacing.
            /// </summary>
            ClampChainAlignAxis = 1 << 3,

            /// <summary>
            /// Allows the chain's size to vary in both dimensions based on the size of its contents.
            /// </summary>
            ClampChainBoth = ClampChainOffAxis | ClampChainAlignAxis,

            /// <summary>
            /// Forces the chain to match the size of its contents on the off axis.
            /// Supersedes <see cref="ClampChainOffAxis"/>.
            /// </summary>
            FitChainOffAxis = 1 << 4,

            /// <summary>
            /// Forces the chain to match the size of its contents on the align axis.
            /// Supersedes <see cref="ClampChainAlignAxis"/>.
            /// </summary>
            FitChainAlignAxis = 1 << 5,

            /// <summary>
            /// Forces the chain to match the size of its contents in both dimensions.
            /// Supersedes <see cref="ClampChainBoth"/>.
            /// </summary>
            FitChainBoth = FitChainOffAxis | FitChainAlignAxis,

            // Member Sizing

            /// <summary>
            /// Constrains member size on the off axis between <see cref="HudChain{T,T}.MemberMinSize"/> 
            /// and <see cref="HudChain{T,T}.MemberMaxSize"/>. If no maximum is set, the chain's size is used as the upper bound.
            /// </summary>
            ClampMembersOffAxis = 1 << 6,

            /// <summary>
            /// Constrains member size on the align axis between <see cref="HudChain{T,T}.MemberMinSize"/> and 
            /// <see cref="HudChain{T,T}.MemberMaxSize"/>. If no maximum is set, the chain's size is used as the upper bound.
            /// </summary>
            ClampMembersAlignAxis = 1 << 7,

            /// <summary>
            /// Constrains member size in both dimensions.
            /// If no maximum is set, the chain's size is used as the upper bounds.
            /// </summary>
            ClampMembersBoth = ClampMembersAlignAxis | ClampMembersOffAxis,

            /// <summary>
            /// Forces member off-axis size to equal the chain's off-axis size (or <see cref="HudChain{T,T}.MemberMaxSize"/> if set).
            /// Supersedes <see cref="ClampMembersOffAxis"/>.
            /// </summary>
            FitMembersOffAxis = 1 << 8,

            /// <summary>
            /// Forces member align-axis size to equal the maximum proportional align-axis size 
            /// (or <see cref="HudChain{T,T}.MemberMaxSize"/> if set). Supersedes <see cref="ClampMembersAlignAxis"/> and 
            /// per-member alignAxisScale.
            /// </summary>
            FitMembersAlignAxis = 1 << 9,

            /// <summary>
            /// Forces members to fill the available space in the chain (or up to <see cref="HudChain{T,T}.MemberMaxSize"/>).
            /// Supersedes <see cref="ClampMembersBoth"/>.
            /// </summary>
            FitMembersBoth = FitMembersAlignAxis | FitMembersOffAxis,

            // Member alignment - These settings are mutually exclusive. Set one only.

            /// <summary>
            /// Aligns the start of the item stack to the Left (Horizontal) or Top (Vertical) of the chain.
            /// </summary>
            AlignMembersStart = 1 << 10,

            /// <summary>
            /// Aligns the start of the item stack to the Right (Horizontal) or Bottom (Vertical) of the chain.
            /// </summary>
            AlignMembersEnd = 1 << 11,

            /// <summary>
            /// Centers the item stack within the chain.
            /// </summary>
            AlignMembersCenter = 1 << 12,
        }

        /// <summary>
        /// Organizes child elements into a linear stack, either horizontally or vertically.
        /// Conceptually similar to a CSS Flexbox or a UI StackPanel.
        /// </summary>
        /// <remarks>
        /// Layout Constraints:
        /// <para> 1) Chain members must fit inside the chain. How this is accomplished depends on the <see cref="HudChainSizingModes"/>. 
        /// Chain size is determined either by the parent node (if fixed) or internally via <see cref="Measure"/> (if flexible). </para>
        /// <para> 2) Members are positioned within the chain's bounds based on the alignment axis. </para>
        /// <para> 3) Undefined behavior may occur if members are incompatible with the specific sizing mode. 
        /// </para>
        /// </remarks>
        public class HudChain<TElementContainer, TElement> : HudCollection<TElementContainer, TElement>
            where TElementContainer : IChainElementContainer<TElement>, new()
            where TElement : HudElementBase
        {
            /// <exclude/>
            protected const HudElementStates nodeSetVisible = HudElementStates.IsVisible | HudElementStates.IsRegistered;
            /// <exclude/>
            protected const HudChainSizingModes chainSelfSizingMask = HudChainSizingModes.FitChainBoth | HudChainSizingModes.ClampChainBoth;
            /// <exclude/>
            protected const HudChainSizingModes chainAutoAlignAxisMask = HudChainSizingModes.FitChainAlignAxis | HudChainSizingModes.ClampChainAlignAxis;
            /// <exclude/>
            protected const HudChainSizingModes chainAutoOffAxisMask = HudChainSizingModes.FitChainOffAxis | HudChainSizingModes.ClampChainOffAxis;
            /// <exclude/>
            protected const HudChainSizingModes memberVariableSizeMask = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.ClampMembersBoth;
            /// <exclude/>
            protected const HudChainSizingModes memberVariableAlignAxisMask = HudChainSizingModes.FitMembersAlignAxis | HudChainSizingModes.ClampMembersAlignAxis;
            /// <exclude/>
            protected const HudChainSizingModes memberVariableOffAxisMask = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampMembersOffAxis;

            /// <summary>
            /// Enables nested collection-initializer syntax.
            /// </summary>
            public new HudChain<TElementContainer, TElement> CollectionContainer => this;

            /// <summary>
            /// The gap between chain elements along the alignment axis.
            /// </summary>
            public float Spacing { get; set; }

            /// <summary>
            /// The maximum size allowed for a chain member. 
            /// <para>Note: Requires a Fit/Clamp member sizing mode to be effective on a specific axis.</para>
            /// </summary>
            public Vector2 MemberMaxSize { get; set; }

            /// <summary>
            /// The minimum size allowed for a chain member.
            /// <para>Note: Requires a Clamp member sizing mode. Has no effect if FitMembers is set.</para>
            /// </summary>
            public Vector2 MemberMinSize { get; set; }

            /// <summary>
            /// Defines how the chain resizes itself and how it resizes its children.
            /// Default is <see cref="HudChainSizingModes.FitChainBoth"/>.
            /// </summary>
            public HudChainSizingModes SizingMode { get; set; }

            /// <summary>
            /// If true, elements are stacked vertically (Top to Bottom). 
            /// If false, elements are stacked horizontally (Left to Right).
            /// </summary>
            public virtual bool AlignVertical
            {
                get { return _alignVertical; }
                set
                {
                    if (value)
                    {
                        alignAxis = 1; // Y
                        offAxis = 0;   // X
                    }
                    else
                    {
                        alignAxis = 0; // X
                        offAxis = 1;   // Y
                    }

                    _alignVertical = value;
                }
            }

            /// <exclude/>
            protected bool _alignVertical;

            /// <summary>
            /// Index of the axis elements are stacked on (0 for X/Horizontal, 1 for Y/Vertical).
            /// </summary>
            /// <exclude/>
            protected int alignAxis;

            /// <summary>
            /// Index of the axis perpendicular to the stack (1 for Y/Horizontal, 0 for X/Vertical).
            /// </summary>
            /// <exclude/>
            protected int offAxis;

            /// <summary>
            /// The total size of all visible members combined (including spacing).
            /// </summary>
            /// <exclude/>
            protected Vector2 rangeSize;

            /// <summary>
            /// The number of visible elements in the current calculation pass.
            /// </summary>
            /// <exclude/>
            protected int rangeLength;

            public HudChain(bool alignVertical = false, HudParentBase parent = null) : base(parent)
            {
                Spacing = 0f;
                SizingMode = HudChainSizingModes.FitChainBoth;
                AlignVertical = alignVertical;
            }

            public HudChain(HudParentBase parent) : this(false, parent)
            { }

            public HudChain() : this(false, null)
            { }

            /// <summary>
            /// Adds a UI element to the end of the chain.
            /// </summary>
            /// <param name="element">The element to add.</param>
            /// <param name="alignAxisScale">
            /// Determines how excess space is distributed on the alignment axis.
            /// <para>0f = Element keeps its fixed size (default behavior).</para>
            /// <para>> 0f = Element scales proportionally relative to other weighted elements to fill remaining space.</para>
            /// <para>Note: Overridden if Fit/ClampMemberAlignAxis sizing flags are set.</para>
            /// </param>
            public virtual void Add(TElement element, float alignAxisScale)
            {
                var newContainer = new TElementContainer();
                newContainer.SetElement(element);
                newContainer.AlignAxisScale = alignAxisScale;
                Add(newContainer);
            }

            /// <summary>
            /// Forces the size of members within the specified index range.
            /// <para>Axes set to 0 in <paramref name="newSize"/> retain their original size.</para>
            /// </summary>
            /// <returns>The calculated total size of the affected members.</returns>
            public virtual Vector2 SetRangeSize(Vector2 newSize, int start = 0, int end = -1)
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

                        if ((element.Config[StateID] & (uint)HudElementStates.IsVisible) > 0)
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

                return listSize;
            }

            /// <summary>
            /// Calculates the combined size of the chain elements within the given index range.
            /// </summary>
            /// <param name="start">Start index.</param>
            /// <param name="end">End index (-1 for last element).</param>
            /// <returns>Total size vector (Width, Height) required to fit the range.</returns>
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

                        if ((element.Config[StateID] & (uint)HudElementStates.IsVisible) > 0)
                        {
                            Vector2 elementSize = element.UnpaddedSize + element.Padding;
                            listSize[offAxis] = Math.Max(listSize[offAxis], elementSize[offAxis]);
                            listSize[alignAxis] += elementSize[alignAxis];
                            visCount++;
                        }
                    }

                    listSize[alignAxis] += Spacing * (visCount - 1);
                }

                return listSize;
            }

            /// <summary>
            /// Calculates the total size of the visible range with member sizing rules applied
            /// </summary>
            /// <exclude/>
            protected virtual Vector2 GetBoundedRangeSize()
            {
                Vector2 minSize = MemberMinSize,
                    maxSize = MemberMaxSize,
                    listSize = Vector2.Zero;
                int visCount = 0;

                maxSize[offAxis] = (maxSize[offAxis] == 0f) ? UnpaddedSize[offAxis] : maxSize[offAxis];

                for (int i = 0; i < hudCollectionList.Count; i++)
                {
                    var entry = hudCollectionList[i];
                    TElement element = entry.Element;

                    if ((element.Config[StateID] & (uint)HudElementStates.IsVisible) > 0)
                    {
                        Vector2 size = element.UnpaddedSize + element.Padding;

                        if ((SizingMode & HudChainSizingModes.FitMembersAlignAxis) > 0)
                            size[alignAxis] = (maxSize[alignAxis] > 0f) ? maxSize[alignAxis] : size[alignAxis];
                        else if ((SizingMode & HudChainSizingModes.ClampMembersAlignAxis) > 0)
                        {
                            if (maxSize[alignAxis] > 0f)
                                size[alignAxis] = MathHelper.Clamp(size[alignAxis], minSize[alignAxis], maxSize[alignAxis]);
                            else
                                size[alignAxis] = Math.Max(size[alignAxis], minSize[alignAxis]);
                        }

                        if ((SizingMode & HudChainSizingModes.FitMembersOffAxis) > 0)
                            size[offAxis] = (maxSize[offAxis] > 0f) ? maxSize[offAxis] : size[offAxis];
                        else if ((SizingMode & HudChainSizingModes.ClampMembersOffAxis) > 0)
                        {
                            if (maxSize[offAxis] > 0f)
                                size[offAxis] = MathHelper.Clamp(size[offAxis], minSize[offAxis], maxSize[offAxis]);
                            else
                                size[offAxis] = Math.Max(size[offAxis], minSize[offAxis]);
                        }

                        listSize[offAxis] = Math.Max(listSize[offAxis], size[offAxis]);
                        listSize[alignAxis] += size[alignAxis];
                        visCount++;
                    }
                }

                listSize[alignAxis] += Spacing * (visCount - 1);

                return listSize;
            }

            /// <summary>
            /// Determines the size of the container based on its children (if self-sizing is enabled).
            /// This is the first pass of the layout process.
            /// </summary>
            /// <exclude/>
            protected override void Measure()
            {
                bool isSelfSizing = (SizingMode & chainSelfSizingMask) > 0;
                bool isMemberSizeVariable = (SizingMode & memberVariableSizeMask) > 0;
                bool isSizeUninitialized = (UnpaddedSize.X == 0f || UnpaddedSize.Y == 0f);

                if (isSelfSizing || isSizeUninitialized)
                {
                    Vector2 chainBounds = UnpaddedSize;

                    if (isMemberSizeVariable)
                        rangeSize = GetBoundedRangeSize();
                    else
                        rangeSize = GetRangeSize();

                    if (rangeSize[alignAxis] > 0f)
                    {
                        // Set align size equal to range size
                        if (chainBounds[alignAxis] == 0f || (SizingMode & HudChainSizingModes.FitChainAlignAxis) == HudChainSizingModes.FitChainAlignAxis)
                            chainBounds[alignAxis] = rangeSize[alignAxis];
                        // Keep align size at or above range size
                        else if ((SizingMode & HudChainSizingModes.ClampChainAlignAxis) == HudChainSizingModes.ClampChainAlignAxis)
                            chainBounds[alignAxis] = Math.Max(chainBounds[alignAxis], rangeSize[alignAxis]);
                    }

                    if (rangeSize[offAxis] > 0f)
                    {
                        // Set off axis size equal to range size
                        if (chainBounds[offAxis] == 0f || (SizingMode & HudChainSizingModes.FitChainOffAxis) == HudChainSizingModes.FitChainOffAxis)
                            chainBounds[offAxis] = rangeSize[offAxis];
                        // Keep off axis size at or above range size
                        else if ((SizingMode & HudChainSizingModes.ClampChainOffAxis) == HudChainSizingModes.ClampChainOffAxis)
                            chainBounds[offAxis] = Math.Max(chainBounds[offAxis], rangeSize[offAxis]);
                    }

                    UnpaddedSize = chainBounds;
                }
            }

            /// <summary>
            /// Calculates the positions of UI elements inside the chain and sets their sizes if dynamic sizing is enabled.
            /// This is the second pass of the layout process.
            /// </summary>
            /// <exclude/>
            protected override void Layout()
            {
                Vector2 chainBounds = UnpaddedSize;

                if (hudCollectionList.Count > 0 && (chainBounds.X > 0f && chainBounds.Y > 0f))
                {
                    UpdateRangeSize(chainBounds);

                    if (rangeLength > 0)
                    {
                        // Find the start and end points of the span within the chain element
                        Vector2 startOffset = Vector2.Zero,
                            endOffset = Vector2.Zero;
                        float elementSpanLength = rangeSize[alignAxis];
                        float rcpSpanLength = 1f / Math.Max(elementSpanLength, 1E-6f);

                        elementSpanLength = Math.Min(elementSpanLength, chainBounds[alignAxis]);

                        // Determine alignment
                        if (alignAxis == 1) // Vertical
                        {
                            if ((SizingMode & HudChainSizingModes.AlignMembersCenter) > 0)
                            {
                                startOffset.Y = .5f * elementSpanLength;
                                endOffset.Y = startOffset.Y - elementSpanLength;
                            }
                            else if ((SizingMode & HudChainSizingModes.AlignMembersEnd) > 0)
                            {
                                endOffset.Y = -.5f * chainBounds.Y;
                                startOffset.Y = endOffset.Y + elementSpanLength;
                            }
                            else
                            {
                                startOffset.Y = .5f * chainBounds.Y;
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
                                endOffset.X = .5f * chainBounds.X;
                                startOffset.X = endOffset.X - elementSpanLength;
                            }
                            else
                            {
                                startOffset.X = -.5f * chainBounds.X;
                                endOffset.X = startOffset.X + elementSpanLength;
                            }
                        }

                        // Place children in the chain
                        UpdateMemberOffsets(startOffset, endOffset, rcpSpanLength);
                    }
                }

            }

            /// <summary>
            /// Recalculates member sizes and total range size given the chain's bounds.
            /// Handles the logic for <see cref="HudChainSizingModes"/> related to member sizing.
            /// </summary>
            /// <param name="chainBounds">The total available space for the chain.</param>
			/// <exclude/>
            protected void UpdateRangeSize(Vector2 chainBounds)
            {
                // 1. Reset and Initialization
                rangeSize = Vector2.Zero;
                rangeLength = 0;

                int start = 0;
                int end = -1;

                float totalScale = 0f;
                float constantSpanLength = 0f;

                // 2. Measure Pass: Calculate visible items and scaling weights
                for (int i = 0; i < hudCollectionList.Count; i++)
                {
                    TElementContainer container = hudCollectionList[i];

                    // Skip invisible elements
                    if ((container.Element.Config[StateID] & (uint)HudElementStates.IsVisible) == 0)
                        continue;

                    // Track indices for the second loop
                    if (end == -1) start = i;
                    end = i;

                    rangeLength++;
                    totalScale += container.AlignAxisScale;

                    // Track fixed size usage for proportional calculation
                    if (container.AlignAxisScale == 0f)
                    {
                        Vector2 size = container.Element.UnpaddedSize + container.Element.Padding;
                        constantSpanLength += size[alignAxis];
                    }
                }

                // If nothing is visible, exit early
                if (rangeLength == 0) return;

                // 3. Preparation: Calculate constraints and ratios
                float totalSpacing = Spacing * (rangeLength - 1);

                // Determine Sizing Modes
                bool reqPropScaling = (SizingMode & memberVariableAlignAxisMask) == 0;
                bool fitAlign = (SizingMode & HudChainSizingModes.FitMembersAlignAxis) > 0;
                bool clampAlign = (SizingMode & HudChainSizingModes.ClampMembersAlignAxis) > 0;
                bool fitOff = (SizingMode & HudChainSizingModes.FitMembersOffAxis) > 0;
                bool clampOff = (SizingMode & HudChainSizingModes.ClampMembersOffAxis) > 0;

                Vector2 minLimit = MemberMinSize;
                Vector2 maxLimit = MemberMaxSize;

                // Variables for Proportional Logic
                float propFixedSpace = 0f;
                float rcpTotalScale = 0f;

                if (reqPropScaling)
                {
                    propFixedSpace = Math.Max(chainBounds[alignAxis] - constantSpanLength - totalSpacing, 0f);
                    rcpTotalScale = Math.Min(1f / Math.Max(totalScale, 1f), 1f);
                }
                else
                {
                    // Update Upper Limits for Non-Proportional Align Axis
                    float maxAllowedAlign = (chainBounds[alignAxis] - totalSpacing) / rangeLength;

                    if (maxAllowedAlign > 0f && (SizingMode & memberVariableAlignAxisMask) > 0)
                    {
                        maxLimit[alignAxis] = (maxLimit[alignAxis] == 0f || maxAllowedAlign < maxLimit[alignAxis])
                            ? maxAllowedAlign
                            : maxLimit[alignAxis];
                    }
                }

                // Update Upper Limits for Off Axis
                if (chainBounds[offAxis] > 0f && (SizingMode & memberVariableOffAxisMask) > 0)
                {
                    maxLimit[offAxis] = (maxLimit[offAxis] == 0f || chainBounds[offAxis] < maxLimit[offAxis])
                        ? chainBounds[offAxis]
                        : maxLimit[offAxis];
                }

                // 4. Application Pass: Apply sizes to elements
                for (int i = start; i <= end; i++)
                {
                    TElementContainer container = hudCollectionList[i];
                    TElement element = container.Element;

                    // Skip invisible elements
                    if ((element.Config[StateID] & (uint)HudElementStates.IsVisible) == 0)
                        continue;

                    Vector2 size = element.UnpaddedSize + element.Padding;

                    // Align Axis Sizing
                    if (reqPropScaling)
                    {
                        if (container.AlignAxisScale != 0f && propFixedSpace > 0f)
                            size[alignAxis] = propFixedSpace * (container.AlignAxisScale * rcpTotalScale);
                    }
                    else
                    {
                        if (fitAlign)
                            size[alignAxis] = maxLimit[alignAxis];
                        else if (clampAlign)
                        {
                            if (maxLimit[alignAxis] > 0f)
                                size[alignAxis] = MathHelper.Clamp(size[alignAxis], minLimit[alignAxis], maxLimit[alignAxis]);
                            else
                                size[alignAxis] = Math.Max(size[alignAxis], minLimit[alignAxis]);
                        }
                    }

                    // Off Axis Sizing
                    if (fitOff)
                        size[offAxis] = maxLimit[offAxis];
                    else if (clampOff)
                    {
                        if (maxLimit[offAxis] > 0f)
                            size[offAxis] = MathHelper.Clamp(size[offAxis], minLimit[offAxis], maxLimit[offAxis]);
                        else
                            size[offAxis] = Math.Max(size[offAxis], minLimit[offAxis]);
                    }

                    // Apply changes
                    element.UnpaddedSize = size - element.Padding;

                    // Accumulate total chain size
                    rangeSize[alignAxis] += size[alignAxis];
                    rangeSize[offAxis] = Math.Max(size[offAxis], rangeSize[offAxis]);
                }

                // Add total spacing to the alignment axis
                rangeSize[alignAxis] += totalSpacing;
            }

            /// <summary>
            /// Calculates and applies position offsets to arrange members in a linear stack.
            /// </summary>
            /// <exclude/>
            protected void UpdateMemberOffsets(Vector2 startOffset, Vector2 endOffset, float rcpSpanLength, float offAxisOffset = 0f)
            {
                ParentAlignments left = (ParentAlignments)((int)ParentAlignments.Left * (2 - alignAxis)),
                    right = (ParentAlignments)((int)ParentAlignments.Right * (2 - alignAxis)),
                    bitmask = left | right;
                float j = 0f, spacingInc = Spacing * rcpSpanLength;

                for (int i = 0; i < hudCollectionList.Count; i++)
                {
                    TElementContainer container = hudCollectionList[i];
                    TElement element = container.Element;

                    if ((element.Config[StateID] & (uint)HudElementStates.IsVisible) > 0)
                    {
                        Vector2 size = element.UnpaddedSize + element.Padding;

                        // Enforce alignment restrictions
                        element.ParentAlignment &= bitmask;
                        element.ParentAlignment |= ParentAlignments.Inner | ParentAlignments.UsePadding;

                        float increment = size[alignAxis] * rcpSpanLength;
                        Vector2 offset = Vector2.Lerp(startOffset, endOffset, j + (.5f * increment));

                        if ((element.ParentAlignment & left) == left)
                            offset[offAxis] += offAxisOffset;
                        else if ((element.ParentAlignment & right) == right)
                            offset[offAxis] -= offAxisOffset;

                        element.Offset = offset;
                        j += increment + spacingInc;
                    }
                }
            }
        }

        /// <summary>
        /// Organizes child elements into a linear stack, either horizontally or vertically.
        /// <para>Alias of <see cref="HudChain{TElementContainer, TElement}"/>.</para>
        /// </summary>
        public class HudChain<TElementContainer> : HudChain<TElementContainer, HudElementBase>
            where TElementContainer : IChainElementContainer<HudElementBase>, new()
        {
            public HudChain(bool alignVertical = false, HudParentBase parent = null) : base(alignVertical, parent)
            { }

            public HudChain(HudParentBase parent) : base(true, parent)
            { }
        }

        /// <summary>
        /// Organizes child elements into a linear stack, either horizontally or vertically.
        /// <para>Alias of <see cref="HudChain{TElementContainer, TElement}"/>.</para>
        /// </summary>
        public class HudChain : HudChain<HudElementContainer<HudElementBase>, HudElementBase>
        {
            public HudChain(bool alignVertical = false, HudParentBase parent = null) : base(alignVertical, parent)
            { }

            public HudChain(HudParentBase parent) : base(true, parent)
            { }
        }
    }
}