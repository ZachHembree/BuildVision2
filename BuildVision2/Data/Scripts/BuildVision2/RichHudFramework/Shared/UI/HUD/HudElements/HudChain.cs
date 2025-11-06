using RichHudFramework.Internal;
using System;
using System.ComponentModel;
using VRage;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        using static NodeConfigIndices;

        /// <summary>
        /// Used to control sizing behavior of HudChain members and the containing chain element itself. The align axis
        /// is the axis chain elements are arranged on; the off axis is the perpendicular axis. When vertically aligned, Y is 
        /// the align axis and X is the off axis. Otherwise, it's reversed.
        /// </summary>
        public enum HudChainSizingModes : ushort
        {
			/// <summary>
			/// Indicates if the chain is configured to continuously update its own size. Not a configuration option.
			/// Does nothing by itself.
			/// </summary>
			IsSelfResizing = 1 << 0,

			// Naming: [Clamp/Fit/Align][Chain/Members][OffAxis/AlignAxis/Both]
			// Fit superceeds Clamp
			// Chain Sizing

			/// <summary>
			/// Allows the size of the chain on the off axis to vary freely, so long as it remains large enough 
			/// to contain its members.
			/// </summary>
			ClampChainOffAxis = 1 << 1 | IsSelfResizing,

			/// <summary>
			/// Allows the size of the chain on the align axis to vary freely, so long as it remains large enough 
			/// to contain its members.
			/// </summary>
			ClampChainAlignAxis = 1 << 2 | IsSelfResizing,

			/// <summary>
			/// Allows the chain's size to vary freely in both dimensions, so long as it remains large enough to
            /// contain its members.
			/// </summary>
			ClampChainBoth = ClampChainOffAxis | ClampChainAlignAxis,

			/// <summary>
			/// Alllows the chain to automatically shrink or expand to fit its contents on its off axis.
			/// Supercedes ClampChainOffAxis.
			/// </summary>
			FitChainOffAxis = 1 << 3 | IsSelfResizing,

			/// <summary>
			/// Allows the chain to automatically shrink or expand to fit its contents on its align axis.
			/// Supercedes ClampChainAlignAxis.
			/// </summary>
			FitChainAlignAxis = 1 << 4 | IsSelfResizing,

			/// <summary>
			/// Allows the chain to automatically shrink or expand to fit its contents in both dimensions.
			/// Supercedes ClampChainBoth.
			/// </summary>
			FitChainBoth = FitChainOffAxis | FitChainAlignAxis,

			// Member Sizing

			/// <summary>
			/// Allows member off axis size to vary freely between the minimum and maximum configured sizes.
            /// If no maximum is set, then the chain's overall off axis is used as the upper bound.
			/// </summary>
			ClampMembersOffAxis = 1 << 5,

			/// <summary>
			/// Allows member align axis size to vary freely between the minimum and maximum configured sizes.
			/// If no maximum is set, then the chain's overall align axis is used as the upper bound.
			/// </summary>
			ClampMembersAlignAxis = 1 << 6,

			/// <summary>
			/// Allows member size to vary freely between the minimum and maximum configured sizes.
			/// If no maximum is set, then the chain's overall size is used as the upper bounds.
			/// </summary>
			ClampMembersBoth = ClampMembersAlignAxis | ClampMembersOffAxis,

			/// <summary>
			/// Sets member off axis size equal to the size of the chain or MaximumSize on the off axis.
			/// Superceeds ClampMembers[Axis] and per-member alignAxisScale property.
			/// </summary>
			FitMembersOffAxis = 1 << 7,

			/// <summary>
			/// Sets member align axis size equal to the size of the chain or MaximumSize on the align axis.
			/// Superceeds ClampMembers[Axis] and per-member alignAxisScale property.
			/// </summary>
			FitMembersAlignAxis = 1 << 8,

			/// <summary>
			/// Sets member sizes equal to the maximum allowable size of the chain or MaximumSize.
			/// Superceeds ClampMembers[Axis] and per-member alignAxisScale property.
			/// </summary>
			FitMembersBoth = FitMembersAlignAxis | FitMembersOffAxis,

			// Member alignment - These settings are mutually exclusive. Set one only.

			/// <summary>
			/// Aligns the start of the chain to the left or top inner edge of the chain.
			/// </summary>
			AlignMembersStart = 1 << 10,

			/// <summary>
			/// Aligns the start of the chain to the right or bottom inner edge of the chain.
			/// </summary>
			AlignMembersEnd = 1 << 11,

			/// <summary>
			/// Aligns the start of the chain to the center of the chain.
			/// </summary>
			AlignMembersCenter = 1 << 12,
		}

        /// <summary>
        /// HUD element used to organize other elements into straight lines, either horizontal or vertical.
        /// </summary>
        /*
         Rules:
            1) Chain members must fit inside the chain. How this is accomplished depends on the sizing mode. Chain size
            is determined by the parent node's Layout or internally, on UpdateSize.
            2) Members must be positioned within the chain's bounds.
            3) Members are assumed to be compatible with the specified sizing mode. Otherwise the behavior is undefined
            and incorrect positioning and sizing will likely occur.
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
			/// Maximum chain member size. If no maximum is set, then the last chain size will be used instead.
			/// Requires a Fit/Clamp[Off/Align/Both] member sizing mode to be set for the given axis.
			/// </summary>
			public Vector2 MemberMaxSize { get; set; }

			/// <summary>
			/// Minimum allowable member size. Requires a ClampMembers[Off/Align/Both] modes to be set for a given 
            /// axis. Has no effect if FitMembers is set for a given axis.
			/// </summary>
			public Vector2 MemberMinSize { get; set; }

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

            /// <summary>
            /// Direction of member stacking
            /// </summary>
            protected int alignAxis;

            /// <summary>
            /// Axis perpindicular to alignAxis
            /// </summary>
            protected int offAxis;

            public HudChain(bool alignVertical, HudParentBase parent = null) : base(parent)
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
            /// <param name="alignAxisScale">
            /// Normalized scale of the element relative to the chain along the align axis, less padding and space
            /// required for other chain members. Can be overridden by Fit/ClampMember sizing modes.
            /// 
            /// 0f == constant size (default); 1f = auto
            /// </param>
            public virtual void Add(TElement element, float alignAxisScale)
            {
                var newContainer = new TElementContainer();
                newContainer.SetElement(element);
                newContainer.AlignAxisScale = alignAxisScale;
                Add(newContainer);
            }

            /// <summary>
            /// Sets chain members in the given range to the dimensions given.
            /// Axes set to 0 are ignored.
            /// </summary>
            public virtual Vector2 SetRangeSize(Vector2 newSize, int start = 0, int end = -1)
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

                return listSize + Padding;
            }

            /// <summary>
            /// Calculates the total size of the chain elements in the given range.
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

                return listSize + Padding;
            }

			protected override void UpdateSize()
            {
                // If self-resizing or size is uninitialized
                if ((SizingMode & HudChainSizingModes.IsSelfResizing) > 0 || (UnpaddedSize.X == 0f || UnpaddedSize.Y == 0f))
                {
					Vector2 rangeSize = GetRangeSize(),
                        chainSize = UnpaddedSize;

					if (rangeSize[alignAxis] > 0f)
					{
						// Set align size equal to range size
						if (chainSize[alignAxis] == 0f || (SizingMode & HudChainSizingModes.FitChainAlignAxis) == HudChainSizingModes.FitChainAlignAxis)
							chainSize[alignAxis] = rangeSize[alignAxis];
						// Keep align size at or above range size
						else if ((SizingMode & HudChainSizingModes.ClampChainAlignAxis) == HudChainSizingModes.ClampChainAlignAxis)
							chainSize[alignAxis] = Math.Max(chainSize[alignAxis], rangeSize[alignAxis]);
					}

					if (rangeSize[offAxis] > 0f)
					{
						// Set off axis size equal to range size
						if (chainSize[offAxis] == 0f || (SizingMode & HudChainSizingModes.FitChainOffAxis) == HudChainSizingModes.FitChainOffAxis)
							chainSize[offAxis] = rangeSize[offAxis];
						// Keep off axis size at or above range size
						else if ((SizingMode & HudChainSizingModes.ClampChainOffAxis) == HudChainSizingModes.ClampChainOffAxis)
							chainSize[offAxis] = Math.Max(chainSize[offAxis], rangeSize[offAxis]);
					}

					UnpaddedSize = chainSize;
				}
			}

			protected override void Layout()
            {
                Vector2 chainSize = UnpaddedSize;

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

                        // Place children in the chain
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

                Vector2 minSize = MemberMinSize, 
                    maxSize = MemberMaxSize;

                minSize = Vector2.Max(Vector2.Zero, minSize);
				maxSize = Vector2.Max(Vector2.Zero, maxSize);

				if ((SizingMode & (HudChainSizingModes.FitMembersAlignAxis | HudChainSizingModes.ClampMembersAlignAxis)) > 0)
                {
					if (maxSize[alignAxis] == 0f)
						maxSize[alignAxis] = alignAxisSize;

					maxSize[alignAxis] = Math.Min(maxSize[alignAxis], alignAxisSize);
					alignAxisSize = MathHelper.Clamp(alignAxisSize, minSize[alignAxis], maxSize[alignAxis]);
				}

				if ((SizingMode & (HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.ClampMembersOffAxis)) > 0)
				{
					if (maxSize[offAxis] == 0f)
						maxSize[offAxis] = offAxisSize;

					maxSize[offAxis] = Math.Min(maxSize[offAxis], offAxisSize);
					offAxisSize = MathHelper.Clamp(offAxisSize, minSize[offAxis], maxSize[offAxis]);
				}

				// Get span size
				for (int i = 0; i < hudCollectionList.Count; i++)
                {
                    TElementContainer container = hudCollectionList[i];

                    if ((container.Element.Config[StateID] & (uint)HudElementStates.IsVisible) > 0)
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

                // Update member sizes
                if (visCount > 0)
                {
                    float totalSpacing = Spacing * (visCount - 1),
                        autoSizeLength = Math.Max(alignAxisSize - constantSpanLength - totalSpacing, 0f),
                        rcpTotalScale = Math.Min(1f / Math.Max(totalScale, 1f), 1f);

                    for (int i = 0; i < hudCollectionList.Count; i++)
                    {
                        TElementContainer container = hudCollectionList[i];
                        TElement element = container.Element;

                        if ((element.Config[StateID] & (uint)HudElementStates.IsVisible) > 0)
                        {
                            Vector2 size = element.UnpaddedSize + element.Padding;

							// Variable align axis scaling
							if (container.AlignAxisScale != 0f && autoSizeLength > 0f)
							{
								float effectiveScale = container.AlignAxisScale * rcpTotalScale;
								size[alignAxis] = autoSizeLength * effectiveScale;
							}

							// Uniform align axis constraints
							if ((SizingMode & HudChainSizingModes.FitMembersAlignAxis) > 0)
								size[alignAxis] = alignAxisSize;
							else if ((SizingMode & HudChainSizingModes.ClampMembersAlignAxis) > 0)
								size[alignAxis] = MathHelper.Clamp(size[alignAxis], minSize[alignAxis], alignAxisSize);
                            
							// Update off axis size
							if ((SizingMode & HudChainSizingModes.FitMembersOffAxis) > 0)
                                size[offAxis] = offAxisSize;
                            else if ((SizingMode & HudChainSizingModes.ClampMembersOffAxis) > 0)
                                size[offAxis] = MathHelper.Clamp(size[offAxis], minSize[offAxis], offAxisSize);

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

                    if ((element.Config[StateID] & (uint)HudElementStates.IsVisible) > 0)
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
        /// HUD element used to organize other elements into straight lines, either horizontal or vertical.
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
        /// HUD element used to organize other elements into straight lines, either horizontal or vertical.
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
