using System;
using VRageMath;

namespace RichHudFramework.UI
{
	using static NodeConfigIndices;

	/// <summary>
	/// Scrollable list of hud elements. Can be oriented vertically or horizontally.
	/// </summary>
	public class ScrollBox<TElementContainer, TElement> : HudChain<TElementContainer, TElement>
		where TElementContainer : IScrollBoxEntry<TElement>, new()
		where TElement : HudElementBase
	{
		/// <summary>
		/// Minimum number of visible elements allowed. Zero/disabled by default.
		/// </summary>
		public int MinVisibleCount { get; set; }

		/// <summary>
		/// Minimum total length (on the align axis) of visible members allowed in the scrollbox.
		/// Zero/disabled by default.
		/// </summary>
		public float MinLength { get; set; }

		/// <summary>
		/// Index of the first element in the visible range in the chain.
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
		/// Index of the last element in the visible range in the chain.
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
		/// Range of elements including elements immediately before and after the logical visible
		/// range to allow for clipping.
		/// </summary>
		public Vector2I ClipRange => new Vector2I(_start, _end);

		/// <summary>
		/// Position of the first visible element as it appears in the UI. Does not correspond to actual index.
		/// </summary>
		public int VisStart { get; private set; }

		/// <summary>
		/// Number of elements visible starting from the Start index
		/// </summary>
		public int VisCount { get; private set; }

		/// <summary>
		/// Total number of enabled elements
		/// </summary>
		public int EnabledCount { get; private set; }

		/// <summary>
		/// Background color of the scroll box.
		/// </summary>
		public Color Color { get { return Background.Color; } set { Background.Color = value; } }

		/// <summary>
		/// Color of the slider bar
		/// </summary>
		public Color BarColor { get { return ScrollBar.slide.BarColor; } set { ScrollBar.slide.BarColor = value; } }

		/// <summary>
		/// Bar color when moused over
		/// </summary>
		public Color BarHighlight { get { return ScrollBar.slide.BarHighlight; } set { ScrollBar.slide.BarHighlight = value; } }

		/// <summary>
		/// Color of the slider box when not moused over
		/// </summary>
		public Color SliderColor { get { return ScrollBar.slide.SliderColor; } set { ScrollBar.slide.SliderColor = value; } }

		/// <summary>
		/// Color of the slider button when moused over
		/// </summary>
		public Color SliderHighlight { get { return ScrollBar.slide.SliderHighlight; } set { ScrollBar.slide.SliderHighlight = value; } }

		/// <summary>
		/// If enabled scrolling using the scrollbar and mousewheel will be allowed
		/// </summary>
		public bool EnableScrolling { get; set; }

		/// <summary>
		/// Enable/disable smooth scrolling and range clipping
		/// </summary>
		public bool UseSmoothScrolling { get; set; }

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

		public ScrollBar ScrollBar { get; protected set; }
		public TexturedBox Divider { get; protected set; }
		public TexturedBox Background { get; protected set; }

		protected float scrollBarPadding;
		protected int _intStart, _intEnd, _start, _end, firstEnabled;

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
		}

		public ScrollBox(HudParentBase parent) : this(true, parent)
		{ }

		public ScrollBox() : this(true, null)
		{ }

		public override Vector2 GetRangeSize(int start = 0, int end = -1)
		{
			Vector2 size = base.GetRangeSize(start, end);
			size[offAxis] += scrollBarPadding;
			return size;
		}

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

		protected override void HandleInput(Vector2 cursorPos)
		{
			ScrollBar.MouseInput.InputEnabled = EnableScrolling;

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

		protected override void UpdateSize()
		{
			if ((SizingMode & HudChainSizingModes.IsSelfResizing) == 0 && (MinVisibleCount > 0 || MinLength > 0))
				SizingMode |= HudChainSizingModes.FitChainAlignAxis;

			// If self-resizing or size is uninitialized
			if ((SizingMode & HudChainSizingModes.IsSelfResizing) > 0 || (UnpaddedSize.X == 0f || UnpaddedSize.Y == 0f))
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
							if (visCount >= MinVisibleCount && rangeSize[alignAxis] >= MinLength)
								break;

							Vector2 elementSize = element.UnpaddedSize + element.Padding;
							rangeSize[offAxis] = Math.Max(rangeSize[offAxis], elementSize[offAxis]);
							rangeSize[alignAxis] += elementSize[alignAxis];
							visCount++;
						}
					}

					rangeSize[alignAxis] += Spacing * (visCount - 1);
				}

				rangeSize[offAxis] += scrollBarPadding;
				Vector2 chainSize = UnpaddedSize;

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

				UpdateElementRange(rangeLength, out totalEnabledLength, out scrollOffset);

				if (UseSmoothScrolling)
				{
					if (hudCollectionList.Count > 0)
					{
						hudCollectionList[_start].Element.Visible = true;
						hudCollectionList[_end].Element.Visible = true;
					}
				}
				else
					scrollOffset = 0f;

				float elementSpanLength;

				if (TryGetVisibleRange(chainSize[alignAxis], chainSize[offAxis], out elementSpanLength))
				{
					Vector2 startOffset, endOffset;
					float rcpSpanLength = 1f / Math.Max(elementSpanLength, 1E-6f);

					if (alignAxis == 1) // Vertical
					{
						startOffset = new Vector2(-.5f * scrollBarPadding, .5f * chainSize.Y + scrollOffset);
						endOffset = new Vector2(startOffset.X, startOffset.Y - elementSpanLength);
					}
					else
					{
						startOffset = new Vector2(-.5f * chainSize.X - scrollOffset, .5f * scrollBarPadding);
						endOffset = new Vector2(startOffset.X + elementSpanLength, startOffset.Y);
					}

					UpdateMemberOffsets(startOffset, endOffset, rcpSpanLength);

					// Update slider size
					sliderVisRatio = chainSize[alignAxis] / totalEnabledLength;
				}
			}

			Vector2 sliderSize = ScrollBar.slide.BarSize;
			sliderSize[alignAxis] = sliderSize[alignAxis] * sliderVisRatio;
			ScrollBar.slide.SliderSize = sliderSize;
		}

		/// <summary>
		/// Updates the range of visible members starting with the given start index.
		/// </summary>
		private void UpdateElementRange(float maxLength, out float totalEnabledLength, out float scrollOffset)
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

					if (UseSmoothScrolling)
						element.Config[StateID] |= (uint)HudElementStates.IsSelectivelyMasked;
					else
						element.Config[StateID] &= ~(uint)HudElementStates.IsSelectivelyMasked;

					totalEnabledLength += elementSize + Spacing;
					EnabledCount++;
				}
			}

			totalEnabledLength -= Spacing;
			ScrollBar.Percent = (float)Math.Round(ScrollBar.Percent, 2);
			ScrollBar.Max = (float)Math.Round(Math.Max(totalEnabledLength - maxLength, 0f), 2);

			// Calculate chain layout offset
			float scrollCurrent = ScrollBar.Current,
				epsilon = UseSmoothScrolling ? 1E-2f : 0f,
				scrollDelta = (float)Math.Round(-scrollCurrent - maxLength + epsilon, 2);

			_intEnd = -1;
			scrollOffset = scrollCurrent;

			for (int i = 0; i < hudCollectionList.Count; i++)
			{
				if (hudCollectionList[i].Enabled)
				{
					TElement element = hudCollectionList[i].Element;
					float elementSize = element.UnpaddedSize[alignAxis] + element.Padding[alignAxis];
					scrollDelta += elementSize;

					// Find logical end of visible range
					if (scrollDelta <= 0f)
					{
						scrollOffset -= elementSize + Spacing;
						_intEnd = i;
					}
					else
						break;

					scrollDelta += Spacing;
				}
			}

			VisCount = 0;

			// Update logical range
			// Clamp indices
			int max = hudCollectionList.Count - 1;
			firstEnabled = MathHelper.Clamp(firstEnabled, 0, max);
			_intEnd = MathHelper.Clamp(_intEnd, firstEnabled, max);
			_intStart = MathHelper.Clamp(_intEnd, firstEnabled, max);

			// Find start of visible range
			for (int i = _intEnd; i >= firstEnabled; i--)
			{
				if (hudCollectionList[i].Enabled)
				{
					TElement element = hudCollectionList[i].Element;
					float elementSize = element.UnpaddedSize[alignAxis] + element.Padding[alignAxis];

					if (maxLength >= elementSize)
					{
						scrollOffset += elementSize + Spacing;
						_intStart = i;
						VisCount++;
					}
					else
						break;

					maxLength -= elementSize + Spacing;
				}
			}

			// Find indices of nearest enabled element(s) before and after the logical visible range
			_start = _intStart;
			_end = _intEnd;

			if (UseSmoothScrolling)
			{
				for (int i = _intStart - 1; i >= firstEnabled; i--)
				{
					if (hudCollectionList[i].Enabled)
					{
						_start = i;
						break;
					}
				}

				for (int i = _intEnd + 1; i < hudCollectionList.Count; i++)
				{
					if (hudCollectionList[i].Enabled)
					{
						_end = i;
						break;
					}
				}

				if (_start != _intStart)
					scrollOffset += hudCollectionList[_start].Element.Size[alignAxis] + Spacing;
			}

			VisStart = GetVisibleIndex(_intStart);

			// Set collection visibility
			for (int i = 0; i < hudCollectionList.Count; i++)
			{
				var element = hudCollectionList[i].Element;
				element.Visible = (i >= _intStart && i <= _intEnd) && hudCollectionList[i].Enabled;
			}
		}

		/// <summary>
		/// Returns the number of enabled elements before the one at the given index
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
		/// end of the scrollbox.
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
	/// Scrollable list of hud elements. Can be oriented vertically or horizontally.
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
	/// Scrollable list of hud elements. Can be oriented vertically or horizontally.
	/// </summary>
	public class ScrollBox : ScrollBox<ScrollBoxEntry>
	{
		public ScrollBox(bool alignVertical, HudParentBase parent = null) : base(alignVertical, parent)
		{ }

		public ScrollBox(HudParentBase parent = null) : base(parent)
		{ }
	}
}