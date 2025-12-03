using RichHudFramework.UI.Client;
using RichHudFramework.UI.Server;
using RichHudFramework.UI.Rendering;
using System;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI
{
	using static NodeConfigIndices;

	/// <summary>
	/// Generic radial selection wheel (pie-menu style). Displays a collection of entries arranged
	/// in a circular pattern around a central point. Supports both cursor-based and gesture-based
	/// (drag-to-select) input methods.
	/// </summary>
	/// <typeparam name="TContainer">
	/// Container type that wraps each entry's UI element and provides selection/association data.
	/// </typeparam>
	/// <typeparam name="TElement">
	/// The actual UI element displayed for each entry (must support minimal labeling).
	/// </typeparam>
	public class RadialSelectionBox<TContainer, TElement> : HudCollection<TContainer, TElement>
		where TContainer : IScrollBoxEntry<TElement>, new()
		where TElement : HudElementBase
	{
		/// <summary>
		/// Read-only access to the full list of entries currently in the wheel.
		/// </summary>
		public virtual IReadOnlyList<TContainer> EntryList => hudCollectionList;

		/// <summary>
		/// The currently selected entry. Returns null/default if nothing is selected.
		/// </summary>
		public virtual TContainer Selection
		{
			get
			{
				if (SelectionIndex >= 0 && SelectionIndex < hudCollectionList.Count)
					return hudCollectionList[SelectionIndex];
				return default(TContainer);
			}
		}

		/// <summary>
		/// The entry currently under the cursor or highlighted by gesture. Returns null/default if none.
		/// </summary>
		public virtual TContainer HighlightedEntry
		{
			get
			{
				if (HighlightIndex >= 0 && HighlightIndex < hudCollectionList.Count)
					return hudCollectionList[HighlightIndex];
				return default(TContainer);
			}
		}

		/// <summary>
		/// Index of the currently selected entry in <see cref="EntryList"/>. -1 if no selection.
		/// </summary>
		public virtual int SelectionIndex { get; protected set; } = -1;

		/// <summary>
		/// Index of the currently highlighted entry in <see cref="EntryList"/>. -1 if nothing highlighted.
		/// </summary>
		public virtual int HighlightIndex { get; protected set; } = -1;

		/// <summary>
		/// Desired maximum number of visible slices. Determines polygon subdivision density.
		/// If the number of enabled entries exceeds this value, the actual enabled count takes precedence.
		/// </summary>
		public virtual int MaxEntryCount { get; set; } = 8;

		/// <summary>
		/// Number of entries that are currently enabled and visible on the wheel.
		/// </summary>
		public virtual int EnabledCount { get; protected set; }

		/// <summary>
		/// When true, selection is driven by drag gestures instead of absolute cursor position.
		/// </summary>
		public virtual bool UseGestureInput { get; set; } = false;

		/// <summary>
		/// Background color of the entire radial wheel.
		/// </summary>
		public virtual Color BackgroundColor { get; set; }

		/// <summary>
		/// Color used for the slice under the cursor (or gesture highlight).
		/// </summary>
		public virtual Color HighlightColor { get; set; }

		/// <summary>
		/// Color used for the currently selected slice.
		/// </summary>
		public virtual Color SelectionColor { get; set; }

		/// <summary>
		/// Sensitivity of cursor/gesture movement when selecting slices. Range: 0.3–2.0.
		/// Higher values make the wheel react faster to movement.
		/// </summary>
		public float CursorSensitivity { get; set; }

		/// <summary>
		/// The <see cref="PuncturedPolyBoard"/> responsible for rendering the circular background
		/// and colored selection/highlight slices.
		/// </summary>
		/// <exclude/>
		protected readonly PuncturedPolyBoard polyBoard;

		// Internal tracking

		/// <summary>
		/// Visible (enabled-only) position of the selection
		/// </summary>
		/// <exclude/>
		protected int selectionVisPos;

		/// <summary>
		/// Visible (enabled-only) position of the highlight
		/// </summary>
		/// <exclude/>
		protected int highlightVisPos;

		/// <summary>
		/// Max(MaxEntryCount, EnabledCount) - determines slice size
		/// </summary>
		/// <exclude/>
		protected int effectiveMaxCount;

		/// <summary>
		/// Minimum number of polygon sides for smooth appearance
		/// </summary>
		/// <exclude/>
		protected int minPolySize = 64;

		/// <summary>
		/// True when lastCursorPos needs reset
		/// </summary>
		/// <exclude/>
		protected bool isStartPosStale = true;

		/// <exclude/>
		protected Vector2 lastCursorPos;

		/// <summary>
		/// Used for gesture accumulation and direction
		/// </summary>
		/// <exclude/>
		protected Vector2 cursorNormal;

		public RadialSelectionBox(HudParentBase parent = null) : base(parent)
		{
			polyBoard = new PuncturedPolyBoard()
			{
				Sides = 64
			};

			// Default color scheme
			BackgroundColor = new Color(70, 78, 86);
			HighlightColor = TerminalFormatting.DarkSlateGrey;
			SelectionColor = TerminalFormatting.Mint;

			Size = new Vector2(512f);
			MaxEntryCount = 8;
			CursorSensitivity = 0.5f;
			UseGestureInput = false;
			UseCursor = true;
			isStartPosStale = true;
		}

		/// <summary>
		/// Sets the selection to the entry at the specified index (clamped to valid range).
		/// </summary>
		public void SetSelectionAt(int index)
		{
			SelectionIndex = MathHelper.Clamp(index, 0, hudCollectionList.Count - 1);
			lastCursorPos = new Vector2(HudSpace.CursorPos.X, HudSpace.CursorPos.Y);
		}

		/// <summary>
		/// Sets the selection to the given entry if it exists in the collection.
		/// </summary>
		public void SetSelection(TContainer container)
		{
			int index = FindIndex(x => x.Equals(container));
			if (index != -1)
				SelectionIndex = index;

			lastCursorPos = new Vector2(HudSpace.CursorPos.X, HudSpace.CursorPos.Y);
		}

		/// <summary>
		/// Highlights the entry at the specified index (clamped to valid range).
		/// </summary>
		public void SetHighlightAt(int index)
		{
			HighlightIndex = MathHelper.Clamp(index, 0, hudCollectionList.Count - 1);
			lastCursorPos = new Vector2(HudSpace.CursorPos.X, HudSpace.CursorPos.Y);
		}

		/// <summary>
		/// Highlights the given entry if it exists in the collection.
		/// </summary>
		public void SetHighlight(TContainer container)
		{
			int index = FindIndex(x => x.Equals(container));
			if (index != -1)
				HighlightIndex = index;

			lastCursorPos = new Vector2(HudSpace.CursorPos.X, HudSpace.CursorPos.Y);
		}

		/// <summary>
		/// Removes all entries and clears both selection and highlight.
		/// </summary>
		public override void Clear()
		{
			HighlightIndex = -1;
			SelectionIndex = -1;
			base.Clear();
		}

		/// <summary>
		/// Clears only the highlight (cursor/gesture hover).
		/// </summary>
		public void ClearHighlight() => HighlightIndex = -1;

		/// <summary>
		/// Clears only the current selection.
		/// </summary>
		public void ClearSelection() => SelectionIndex = -1;

		/// <summary>
		/// Updates selection highlighting, visibility and layout
		/// </summary>
		/// <exclude/>
		protected override void Layout()
		{
			// Count enabled entries and sanitize indices
			EnabledCount = 0;
			SelectionIndex = MathHelper.Clamp(SelectionIndex, -1, hudCollectionList.Count - 1);
			HighlightIndex = MathHelper.Clamp(HighlightIndex, -1, hudCollectionList.Count - 1);
			CursorSensitivity = MathHelper.Clamp(CursorSensitivity, 0.3f, 2f);

			for (int i = 0; i < hudCollectionList.Count; i++)
			{
				if (hudCollectionList[i].Enabled)
				{
					hudCollectionList[i].Element.Visible = true;
					EnabledCount++;
				}
				else
				{
					hudCollectionList[i].Element.Visible = false;
				}
			}

			effectiveMaxCount = Math.Max(MaxEntryCount, EnabledCount);

			// Position each enabled entry in its slice
			int sliceSize = polyBoard.Sides / effectiveMaxCount;
			Vector2I slice = new Vector2I(0, sliceSize - 1);
			Vector2 size = UnpaddedSize;

			for (int i = 0; i < hudCollectionList.Count; i++)
			{
				TContainer container = hudCollectionList[i];
				if (container.Enabled)
				{
					container.Element.Offset = 1.05f * polyBoard.GetSliceOffset(size, slice);
					slice += sliceSize;
				}
			}

			// Ensure polygon is detailed enough for the current slice count
			polyBoard.Sides = Math.Max(effectiveMaxCount * 6, minPolySize);
		}

		/// <summary>
		/// Performs cursor bounds checking for the selection box
		/// </summary>
		/// <exclude/>
		protected override void InputDepth()
		{
			_config[StateID] &= ~(uint)HudElementStates.IsMouseInBounds;

			if (HudMain.InputMode == HudInputMode.NoInput || !(HudSpace?.IsFacingCamera ?? false))
				return;

			Vector2 size = UnpaddedSize;
			Vector2 aspect = new Vector2(size.Y / size.X, size.X / size.Y);
			Vector2 cursorPos = new Vector2(HudSpace.CursorPos.X, HudSpace.CursorPos.Y) - Position;
			cursorPos *= aspect;

			float outerRadius = 0.5f * size.X;
			float innerRadius = polyBoard.InnerRadius * outerRadius;
			float distance = cursorPos.Length();

			// Mouse is inside the active ring area
			if (distance > innerRadius && distance < outerRadius)
			{
				_config[StateID] |= (uint)HudElementStates.IsMouseInBounds;
				HudMain.Cursor.TryCaptureHudSpace(HudSpace.CursorPos.Z, HudSpace.GetHudSpaceFunc);
			}
		}

		/// <summary>
		/// Updates selection input
		/// </summary>
		/// <exclude/>
		protected override void HandleInput(Vector2 cursorPos)
		{
			if (UseGestureInput || IsMousedOver)
			{
				if (isStartPosStale)
				{
					cursorNormal = Vector2.Zero;
					lastCursorPos = cursorPos;
					isStartPosStale = false;
				}

				UpdateSelection(cursorPos);
			}
			else
			{
				isStartPosStale = true;
			}
		}

		/// <summary>
		/// Updates <see cref="HighlightIndex"/> based on cursor position or drag gesture.
		/// </summary>
		/// <exclude/>
		protected virtual void UpdateSelection(Vector2 cursorPos)
		{
			Vector2 offset = UseGestureInput ? (cursorPos - lastCursorPos) : (cursorPos - Position);

			// Require a minimum movement to avoid jitter
			if (offset.LengthSquared() > 64f)
			{
				if (UseGestureInput)
				{
					// Accumulate direction for smoother gesture control
					Vector2 normalized = CursorSensitivity * 0.4f * Vector2.Normalize(offset);
					cursorNormal = Vector2.Normalize(cursorNormal + normalized);
				}
				else
				{
					cursorNormal = Vector2.Normalize(offset);
				}

				float bestDot = 0.5f;
				int bestIndex = -1;

				// Find the enabled entry whose offset most closely aligns with cursor direction
				for (int i = 0; i < hudCollectionList.Count; i++)
				{
					var container = hudCollectionList[i];
					if (container.Enabled)
					{
						float dot = (float)Math.Round(Vector2.Dot(container.Element.Offset, cursorNormal), 4);
						if (dot > bestDot)
						{
							bestDot = dot;
							bestIndex = i;
						}
					}
				}

				HighlightIndex = bestIndex;
				lastCursorPos = cursorPos;
			}
		}

		/// <summary>
		/// Converts logical selection/highlight indices into visible slice positions
		/// (skipping disabled entries).
		/// </summary>
		/// <exclude/>
		protected void UpdateVisPos()
		{
			selectionVisPos = -1;
			highlightVisPos = -1;

			SelectionIndex = MathHelper.Clamp(SelectionIndex, -1, hudCollectionList.Count - 1);
			HighlightIndex = MathHelper.Clamp(HighlightIndex, -1, hudCollectionList.Count - 1);

			if (hudCollectionList.Count == 0)
				return;

			if (SelectionIndex != -1)
			{
				for (int i = 0; i <= SelectionIndex; i++)
					if (hudCollectionList[i].Enabled)
						selectionVisPos++;
			}

			if (HighlightIndex != -1)
			{
				for (int i = 0; i <= HighlightIndex; i++)
					if (hudCollectionList[i].Enabled)
						highlightVisPos++;
			}
		}

		/// <summary>
		/// Renders the selection box and highlighting using the polyboard
		/// </summary>
		/// <exclude/>
		protected override void Draw()
		{
			Vector2 size = UnpaddedSize;
			int sliceSize = polyBoard.Sides / effectiveMaxCount;

			polyBoard.Color = BackgroundColor;
			UpdateVisPos();
			polyBoard.Draw(size, Position, HudSpace.PlaneToWorldRef);

			if (sliceSize <= 0)
				return;

			// Draw selection slice (skip if gesture mode and highlight overlaps)
			if (selectionVisPos != -1 && (highlightVisPos != selectionVisPos || !UseGestureInput))
			{
				Vector2I slice = new Vector2I(0, sliceSize - 1) + (selectionVisPos * sliceSize);
				polyBoard.Color = SelectionColor;
				polyBoard.Draw(size, Position, slice, HudSpace.PlaneToWorldRef);
			}

			// Draw highlight slice (skip if it would overlap selection in cursor mode)
			if (highlightVisPos != -1 && (highlightVisPos != selectionVisPos || UseGestureInput))
			{
				Vector2I slice = new Vector2I(0, sliceSize - 1) + (highlightVisPos * sliceSize);
				polyBoard.Color = HighlightColor;
				polyBoard.Draw(size, Position, slice, HudSpace.PlaneToWorldRef);
			}
		}
	}

	/// <summary>
	/// Non-generic radial selection box using the default <see cref="ScrollBoxEntry"/> container
	/// with plain <see cref="HudElementBase"/> elements.
	/// <para>
	/// Alias of <see cref="RadialSelectionBox{TContainer, TElement}"/> using 
	/// <see cref="ScrollBoxEntry"/> and <see cref="HudElementBase"/> as the container and element, respectively.
	/// </para>
	/// </summary>
	public class RadialSelectionBox : RadialSelectionBox<ScrollBoxEntry>
	{
		public RadialSelectionBox(HudParentBase parent = null) : base(parent) { }
	}

	/// <summary>
	/// Generic radial selection box allowing custom containers while keeping
	/// <see cref="HudElementBase"/> as the element type.
	/// <para>
	/// Alias of <see cref="RadialSelectionBox{TContainer, TElement}"/> using 
	/// <see cref="HudElementBase"/> as the element.
	/// </para>
	/// </summary>
	public class RadialSelectionBox<TContainer> : RadialSelectionBox<TContainer, HudElementBase>
		where TContainer : IScrollBoxEntry<HudElementBase>, new()
	{
		public RadialSelectionBox(HudParentBase parent = null) : base(parent) { }
	}
}