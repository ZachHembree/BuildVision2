using System;
using VRageMath;

namespace RichHudFramework.UI
{
	using static NodeConfigIndices;

	/// <summary>
	/// A scrollable container for HUD elements, based on <see cref="HudChain{TElementContainer, TElement}"/>. 
	/// It clips content that exceeds its bounds and provides a scrollbar for navigation.
	/// </summary>
	public class ScrollBox<TElementContainer, TElement> : HudChain<TElementContainer, TElement>
		where TElementContainer : IScrollBoxEntry<TElement>, new()
		where TElement : HudElementBase
	{
		/// <summary>
		/// Minimum number of visible elements allowed in the viewport. 
		/// <para>If non-zero, the ScrollBox will expand to fit at least this many elements if possible.</para>
		/// </summary>
		public int MinVisibleCount { get; set; }

		/// <summary>
		/// Minimum total length (on the align axis) of visible members allowed in the scrollbox.
		/// <para>If non-zero, the ScrollBox will expand to meet this length if the content supports it.</para>
		/// </summary>
		public float MinLength { get; set; }

		/// <summary>
		/// The index of the first element within the visible range of the chain.
		/// Setting this updates the scrollbar position.
		/// </summary>
		public int Start
		{
			get { return MathHelper.Clamp(_intStart, 0, hudCollectionList.Count - 1); }
			set
			{
				if (value != _intStart)
				{
					_intStart = MathHelper.Clamp(value, 0, hudCollectionList.Count - 1);
					ScrollBar.Current = GetMinScrollOffset(_intStart, false);
				}
			}
		}

		/// <summary>
		/// The index of the last element within the visible range of the chain.
		/// Setting this updates the scrollbar position.
		/// </summary>
		public int End
		{
			get { return MathHelper.Clamp(_intEnd, 0, hudCollectionList.Count - 1); }
			set
			{
				if (value != _intEnd)
				{
					_intEnd = MathHelper.Clamp(value, 0, hudCollectionList.Count - 1);
					ScrollBar.Current = GetMinScrollOffset(_intEnd, true);
				}
			}
		}

		/// <summary>
		/// Range of element indices representing the visible area, plus padding to allow for smooth clipping.
		/// Used for scissor rect masking.
		/// </summary>
		public Vector2I ClipRange => new Vector2I(_start, _end);

		/// <summary>
		/// The index relative to the *visible* elements, not the absolute collection index.
		/// </summary>
		public int VisStart { get; private set; }

		/// <summary>
		/// Number of elements currently visible (or partially visible) in the viewport.
		/// </summary>
		public int VisCount { get; private set; }

		/// <summary>
		/// Total number of enabled elements in the list.
		/// </summary>
		public int EnabledCount { get; private set; }

		/// <summary>
		/// Background color of the scroll box.
		/// </summary>
		public Color Color { get { return Background.Color; } set { Background.Color = value; } }

		/// <summary>
		/// Color of the scrollbar track.
		/// </summary>
		public Color BarColor { get { return ScrollBar.SlideInput.BarColor; } set { ScrollBar.SlideInput.BarColor = value; } }

		/// <summary>
		/// Color of the scrollbar track when moused over.
		/// </summary>
		public Color BarHighlight { get { return ScrollBar.SlideInput.BarHighlight; } set { ScrollBar.SlideInput.BarHighlight = value; } }

		/// <summary>
		/// Color of the slider (thumb) when not moused over.
		/// </summary>
		public Color SliderColor { get { return ScrollBar.SlideInput.SliderColor; } set { ScrollBar.SlideInput.SliderColor = value; } }

		/// <summary>
		/// Color of the slider (thumb) when moused over.
		/// </summary>
		public Color SliderHighlight { get { return ScrollBar.SlideInput.SliderHighlight; } set { ScrollBar.SlideInput.SliderHighlight = value; } }

		/// <summary>
		/// If true, scrolling via mouse wheel or scrollbar interaction is enabled.
		/// </summary>
		public bool EnableScrolling { get; set; }

		/// <summary>
		/// Enables pixel-perfect scrolling and range masking.
		/// <para>If false, scrolling snaps to element indices.</para>
		/// </summary>
		public bool UseSmoothScrolling { get; set; }

		/// <summary>
		/// If true, ScrollBox members are arranged vertically (top to bottom).
		/// If false, members are arranged horizontally (left to right).
		/// <para>Also reconfigures the ScrollBar and Divider orientation.</para>
		/// </summary>
		public override bool AlignVertical
		{
			set
			{
				if (ScrollBar == null)
					ScrollBar = new ScrollBar(this);

				if (Divider == null)
					Divider = new TexturedBox(ScrollBar) { Color = new Color(53, 66, 75) };

				ScrollBar.Vertical = value;
				base.AlignVertical = value;

				if (value)
				{
					ScrollBar.DimAlignment = DimAlignments.Height;
					Divider.DimAlignment = DimAlignments.Height;

					ScrollBar.ParentAlignment = ParentAlignments.InnerRight;
					Divider.ParentAlignment = ParentAlignments.InnerLeft;

					Divider.Padding = new Vector2(2f, 0f);
					Divider.Width = 1f;

					ScrollBar.Padding = new Vector2(30f, 10f);
					ScrollBar.Width = 43f;
				}
				else
				{
					ScrollBar.DimAlignment = DimAlignments.Width;
					Divider.DimAlignment = DimAlignments.Width;

					ScrollBar.ParentAlignment = ParentAlignments.InnerBottom;
					Divider.ParentAlignment = ParentAlignments.InnerBottom;

					Divider.Padding = new Vector2(16f, 2f);
					Divider.Height = 1f;

					ScrollBar.Padding = new Vector2(16f);
					ScrollBar.Height = 24f;
				}
			}
		}

		/// <summary>
		/// The slider UI element controlling the scroll position.
		/// </summary>
		public ScrollBar ScrollBar { get; protected set; }

		/// <summary>
		/// Visual divider line between the content area and the scrollbar.
		/// </summary>
		public TexturedBox Divider { get; protected set; }

		/// <summary>
		/// Textured, tintable background behind the scrollbox content.
		/// </summary>
		public TexturedBox Background { get; protected set; }

		/// <summary>
		/// Additional vertical or horizontal padding representing the area taken by the scrollbar.
		/// </summary>
		/// <exclude/>
		private float scrollBarPadding;
		private int _intStart;
		private int _intEnd;
		private int _start;
		private int _end;

		/// <summary>
		/// Index of the first enabled element in the list.
		/// </summary>
		private int firstEnabled;

		public ScrollBox(bool alignVertical, HudParentBase parent = null) : base(alignVertical, parent)
		{
			Background = new TexturedBox(this)
			{
				Color = TerminalFormatting.DarkSlateGrey,
				DimAlignment = DimAlignments.Size,
				ZOffset = -1,
			};

			UseCursor = true;
			ShareCursor = false;
			EnableScrolling = true;
			UseSmoothScrolling = true;
			ZOffset = 1;
			AlignVertical = alignVertical;

			MinVisibleCount = 0;
			MinLength = 0f;
			SizingMode = HudChainSizingModes.FitChainOffAxis;
		}

		public ScrollBox(HudParentBase parent) : this(true, parent)
		{ }

		public ScrollBox() : this(true, null)
		{ }

		/// <summary>
		/// Returns the total size of the ScrollBox's contents, including the scrollbar area.
		/// </summary>
		public override Vector2 GetRangeSize(int start = 0, int end = -1)
		{
			Vector2 size = base.GetRangeSize(start, end);
			size[offAxis] += scrollBarPadding;
			return size;
		}

		/// <summary>
		/// Returns the index of the last enabled element that fits within the count limit, 
		/// starting from the given index.
		/// </summary>
		public int GetRangeEnd(int count, int start = 0)
		{
			start = MathHelper.Clamp(start, 0, hudCollectionList.Count - 1);
			count = MathHelper.Clamp(count, 0, hudCollectionList.Count - start);

			if ((start + count) <= hudCollectionList.Count)
			{
				int end = start,
					enCount = 0;

				for (int i = start; i < hudCollectionList.Count; i++)
				{
					if (hudCollectionList[i].Enabled)
					{
						end = i;
						enCount++;
					}

					if (enCount >= count)
						break;
				}

				return end;
			}

			return -1;
		}

		/// <summary>
		/// Handles mouse input for scrolling (wheel, drag).
		/// </summary>
		/// <exclude/>
		protected override void HandleInput(Vector2 cursorPos)
		{
			ScrollBar.InputEnabled = EnableScrolling;
			ShareCursor = ScrollBar.Max <= 0f;

			if (hudCollectionList.Count > 0 && EnableScrolling && (IsMousedOver || ScrollBar.IsMousedOver))
			{
				if (UseSmoothScrolling)
				{
					if (SharedBinds.MousewheelUp.IsPressed)
						ScrollBar.Current -= hudCollectionList[_intEnd].Element.Size[alignAxis] + Spacing;
					else if (SharedBinds.MousewheelDown.IsPressed)
						ScrollBar.Current += hudCollectionList[_intStart].Element.Size[alignAxis] + Spacing;
				}
				else
				{
					if (SharedBinds.MousewheelUp.IsPressed)
						Start--;
					else if (SharedBinds.MousewheelDown.IsPressed)
						End++;
				}
			}
		}

		/// <summary>
		/// Updates the size of the ScrollBox based on its contents and constraints (MinVisibleCount/MinLength).
		/// </summary>
		/// <exclude/>
		protected override void Measure()
		{
			if (UseSmoothScrolling)
				_config[StateID] |= (uint)HudElementStates.IsMasking;

			if ((SizingMode & chainAutoAlignAxisMask) == 0 && (MinVisibleCount > 0 || MinLength > 0))
				SizingMode |= HudChainSizingModes.FitChainAlignAxis;

			// If self-resizing or size is uninitialized
			if ((SizingMode & chainSelfSizingMask) > 0 || (UnpaddedSize.X == 0f || UnpaddedSize.Y == 0f))
			{
				Vector2 rangeSize = Vector2.Zero;

				// Get minimum range size
				if (hudCollectionList.Count > 0)
				{
					int visCount = 0;

					for (int i = _intStart; i < hudCollectionList.Count; i++)
					{
						var entry = hudCollectionList[i];
						TElement element = entry.Element;

						if (entry.Enabled)
						{
							if ((MinVisibleCount != 0 || MinLength != 0) &&
								(MinVisibleCount == 0 || visCount >= MinVisibleCount) &&
								(MinLength == 0 || rangeSize[alignAxis] >= MinLength))
							{ break; }

							Vector2 elementSize = element.UnpaddedSize + element.Padding;
							rangeSize[offAxis] = Math.Max(rangeSize[offAxis], elementSize[offAxis]);
							rangeSize[alignAxis] += elementSize[alignAxis];
							visCount++;
						}
					}

					rangeSize[alignAxis] += Spacing * (visCount - 1);
				}

				rangeSize[offAxis] += scrollBarPadding;
				Vector2 chainBounds = UnpaddedSize;

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
		/// Calculates layout, updates the scrollbar, determines the visible range, and positions elements.
		/// </summary>
		/// <exclude/>
		protected override void Layout()
		{
			Vector2 effectivePadding = Padding;
			scrollBarPadding = ScrollBar.Size[offAxis];
			effectivePadding[offAxis] += scrollBarPadding;

			Vector2 chainSize = (UnpaddedSize + Padding) - effectivePadding;
			float sliderVisRatio = 0f;

			if (hudCollectionList.Count > 0)
			{
				// Update visible range
				float totalEnabledLength, scrollOffset,
					rangeLength = chainSize[alignAxis];

				if (UseSmoothScrolling)
				{
					UpdateSmoothRange(rangeLength, out totalEnabledLength, out scrollOffset);
				}
				else
				{
					UpdateNormalRange(rangeLength, out totalEnabledLength);
					scrollOffset = 0f;
				}

				UpdateRangeSize(chainSize);

				if (rangeLength > 0)
				{
					Vector2 startOffset, endOffset;
					float rcpSpanLength = 1f / Math.Max(rangeSize[alignAxis], 1E-6f);

					if (alignAxis == 1) // Vertical
					{
						startOffset = new Vector2(-.5f * scrollBarPadding, .5f * chainSize.Y + scrollOffset);
						endOffset = new Vector2(startOffset.X, startOffset.Y - rangeSize[alignAxis]);
					}
					else
					{
						startOffset = new Vector2(-.5f * chainSize.X - scrollOffset, .5f * scrollBarPadding);
						endOffset = new Vector2(startOffset.X + rangeSize[alignAxis], startOffset.Y);
					}

					UpdateMemberOffsets(startOffset, endOffset, rcpSpanLength, 0.5f * scrollBarPadding);

					// Update slider size
					sliderVisRatio = chainSize[alignAxis] / totalEnabledLength;
				}
			}

			ScrollBar.VisiblePercent = sliderVisRatio;
		}

		/// <summary>
		/// Updates the visible range and scrollbar for smooth scrolling.
		/// </summary>
		private void UpdateSmoothRange(float maxLength, out float totalEnabledLength, out float scrollOffset)
		{
			// Get enabld range size and update scrollbar bounds
			EnabledCount = 0;
			firstEnabled = -1;
			totalEnabledLength = 0f;

			for (int i = 0; i < hudCollectionList.Count; i++)
			{
				if (hudCollectionList[i].Enabled)
				{
					// Get first enabled element
					if (firstEnabled == -1)
						firstEnabled = i;

					TElement element = hudCollectionList[i].Element;
					float elementSize = element.UnpaddedSize[alignAxis] + element.Padding[alignAxis];

					totalEnabledLength += elementSize;
					EnabledCount++;
				}
			}

			totalEnabledLength += (EnabledCount - 1) * Spacing;
			ScrollBar.Percent = (float)Math.Round(ScrollBar.Percent, 6);
			ScrollBar.Max = (float)Math.Round(Math.Max(totalEnabledLength - maxLength, 0f), 6);

			// Calculate chain layout offset
			float scrollCurrent = ScrollBar.Current,
				epsilon = 1E-3f,
				viewTop = scrollCurrent,
				viewBottom = scrollCurrent + maxLength - epsilon;

			_intStart = -1;
			_intEnd = -1;
			VisCount = 0;
			scrollOffset = 0f;
			float currentPos = 0f;

			// Find all visible elements
			for (int i = 0; i < hudCollectionList.Count; i++)
			{
				if (hudCollectionList[i].Enabled)
				{
					TElement element = hudCollectionList[i].Element;
					float elementSize = element.UnpaddedSize[alignAxis] + element.Padding[alignAxis];
					float elementTop = currentPos;
					float elementBottom = currentPos + elementSize;

					// Check if the element overlaps the visible viewport
					bool isInRange = (elementBottom > viewTop) && (elementTop < viewBottom);

					if (isInRange)
					{
						// Set start element
						if (_intStart == -1)
						{
							_intStart = i;
							scrollOffset = elementTop - scrollCurrent;
						}

						// Track last end
						_intEnd = i;
						VisCount++;
					}
					// Found a start point
					else if (_intStart != -1)
						break;

					// Move to next element pos
					currentPos += elementSize + Spacing;
				}
			}

			int max = hudCollectionList.Count - 1;

			if (firstEnabled == -1) // Empty list or scrolled past everything
			{
				_intStart = 0;
				_intEnd = 0;
				VisStart = 0;
				scrollOffset = 0f;
				return;
			}

			if (_intStart == -1) // No elements were in view
			{
				_intStart = firstEnabled;
				_intEnd = firstEnabled;
			}

			// Negative offset expected
			scrollOffset *= -1f;
			_intStart = MathHelper.Clamp(_intStart, firstEnabled, max);
			_intEnd = MathHelper.Clamp(_intEnd, _intStart, max);
			_start = _intStart;
			_end = _intEnd;

			for (int i = _start - 1; i >= firstEnabled; i--)
			{
				if (hudCollectionList[i].Enabled)
				{ _start = i; break; }
			}

			for (int i = _end + 1; i < hudCollectionList.Count; i++)
			{
				if (hudCollectionList[i].Enabled)
				{ _end = i; break; }
			}

			if (_start != _intStart)
				scrollOffset += hudCollectionList[_start].Element.Size[alignAxis] + Spacing;

			VisStart = GetVisibleIndex(_intStart);

			// Set collection visibility
			for (int i = 0; i < hudCollectionList.Count; i++)
			{
				var element = hudCollectionList[i].Element;
				bool isInRange = (i >= _start && i <= _end) && hudCollectionList[i].Enabled;
				bool isVisible = (element.Config[StateID] & (uint)HudElementStates.IsVisible) > 0;

				if (isVisible != isInRange)
					element.Visible = isInRange;
			}
		}

		private void UpdateNormalRange(float maxLength, out float totalEnabledLength)
		{
			// Get enabld range size and update scrollbar bounds
			EnabledCount = 0;
			firstEnabled = -1;
			totalEnabledLength = 0f;

			for (int i = 0; i < hudCollectionList.Count; i++)
			{
				if (hudCollectionList[i].Enabled)
				{
					// Get first enabled element
					if (firstEnabled == -1)
						firstEnabled = i;

					TElement element = hudCollectionList[i].Element;
					float elementSize = element.UnpaddedSize[alignAxis] + element.Padding[alignAxis];

					totalEnabledLength += elementSize + Spacing;
					EnabledCount++;
				}
			}

			totalEnabledLength -= Spacing;
			ScrollBar.Percent = (float)Math.Round(ScrollBar.Percent, 2);
			ScrollBar.Max = (float)Math.Round(Math.Max(totalEnabledLength - maxLength, 0f), 2);
			_intEnd = -1;

			float scrollCurrent = ScrollBar.Current,
				scrollDelta = (float)Math.Round(-scrollCurrent - maxLength, 2);

			for (int i = 0; i < hudCollectionList.Count; i++)
			{
				if (hudCollectionList[i].Enabled)
				{
					TElement element = hudCollectionList[i].Element;
					float elementSize = element.UnpaddedSize[alignAxis] + element.Padding[alignAxis];
					scrollDelta += elementSize;

					// Find logical end of visible range
					if (scrollDelta <= 0f)
						_intEnd = i;
					else
						break;

					scrollDelta += Spacing;
				}
			}

			// Update logical range
			int max = hudCollectionList.Count - 1;
			firstEnabled = MathHelper.Clamp(firstEnabled, 0, max);
			_intEnd = MathHelper.Clamp(_intEnd, firstEnabled, max);
			_intStart = MathHelper.Clamp(_intEnd, firstEnabled, max);
			VisCount = 0;

			// Find start of visible range
			for (int i = _intEnd; i >= firstEnabled; i--)
			{
				if (hudCollectionList[i].Enabled)
				{
					TElement element = hudCollectionList[i].Element;
					float elementSize = element.UnpaddedSize[alignAxis] + element.Padding[alignAxis];

					if (maxLength >= elementSize)
					{
						_intStart = i;
						VisCount++;
					}
					else
						break;

					maxLength -= elementSize + Spacing;
				}
			}

			_start = _intStart;
			_end = _intEnd;
			VisStart = GetVisibleIndex(_intStart);

			// Set collection visibility
			for (int i = 0; i < hudCollectionList.Count; i++)
			{
				var element = hudCollectionList[i].Element;
				bool isInRange = (i >= _start && i <= _end) && hudCollectionList[i].Enabled;
				bool isVisible = (element.Config[StateID] & (uint)HudElementStates.IsVisible) > 0;

				if (isVisible != isInRange)
					element.Visible = isInRange;
			}
		}

		/// <summary>
		/// Returns the number of enabled elements that occur before the specified index.
		/// </summary>
		private int GetVisibleIndex(int index)
		{
			int count = 0;

			for (int n = 0; n < index; n++)
			{
				if (hudCollectionList[n].Enabled)
					count++;
			}

			return count;
		}

		/// <summary>
		/// Returns the shortest offset required to bring a member at the given index to either
		/// end of the scrollbox (Top/Bottom or Left/Right).
		/// </summary>
		private float GetMinScrollOffset(int index, bool getEnd)
		{
			if (hudCollectionList.Count > 0)
			{
				firstEnabled = MathHelper.Clamp(firstEnabled, 0, hudCollectionList.Count - 1);

				float elementSize,
					offset = .1f;

				if (getEnd)
					offset -= UnpaddedSize[alignAxis] + Spacing;
				else
				{
					index--;
				}

				for (int i = 0; i <= index && i < hudCollectionList.Count; i++)
				{
					if (hudCollectionList[i].Enabled)
					{
						elementSize = hudCollectionList[i].Element.Size[alignAxis];
						offset += (elementSize + Spacing);
					}
				}

				return Math.Max((float)Math.Round(offset, 6), 0f);
			}
			else
				return 0f;
		}
	}

	/// <summary>
	/// A scrollable container for HUD elements, based on <see cref="HudChain{TElementContainer, TElement}"/>. 
	/// It clips content that exceeds its bounds and provides a scrollbar for navigation. 
	/// <para>
	/// Alias of <see cref="ScrollBox{TElementContainer, TElement}"/> with <see cref="HudElementBase"/> as the element type.
	/// </para>
	/// </summary>
	public class ScrollBox<TElementContainer> : ScrollBox<TElementContainer, HudElementBase>
		where TElementContainer : IScrollBoxEntry<HudElementBase>, new()
	{
		public ScrollBox(bool alignVertical, HudParentBase parent = null) : base(alignVertical, parent)
		{ }

		public ScrollBox(HudParentBase parent = null) : base(parent)
		{ }
	}

	/// <summary>
	/// A scrollable container for HUD elements, based on <see cref="HudChain{TElementContainer, TElement}"/>. 
	/// It clips content that exceeds its bounds and provides a scrollbar for navigation.
	/// <para>
	/// Alias of <see cref="ScrollBox{TElementContainer, TElement}"/> with 
	/// <see cref="ScrollBoxEntry{TElement}"/> as the container and <see cref="HudElementBase"/> as the element.
	/// </para>
	/// </summary>
	public class ScrollBox : ScrollBox<ScrollBoxEntry>
	{
		public ScrollBox(bool alignVertical, HudParentBase parent = null) : base(alignVertical, parent)
		{ }

		public ScrollBox(HudParentBase parent = null) : base(parent)
		{ }
	}
}