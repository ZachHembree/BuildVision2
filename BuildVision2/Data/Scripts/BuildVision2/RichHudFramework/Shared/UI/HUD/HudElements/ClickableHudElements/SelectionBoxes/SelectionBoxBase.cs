using RichHudFramework.UI.Rendering;
using System.Collections;
using System.Collections.Generic;
using VRage;
using VRageMath;

namespace RichHudFramework.UI
{
	/// <summary>
	/// Abstract selection box that uses a <see cref="HudChain{TContainer, TElement}"/> as its backing list.
	/// Intended for fixed-size lists that do not require scrolling.
	/// </summary>
	/// <typeparam name="TContainer">
	/// Container type that wraps each list entry and implements <see cref="ISelectionBoxEntry{TElement}"/>
	/// </typeparam>
	/// <typeparam name="TElement">
	/// The actual UI element displayed for each entry (must have at least a minimal label)
	/// </typeparam>
	public abstract class ChainSelectionBoxBase<TContainer, TElement>
		: SelectionBoxBase<HudChain<TContainer, TElement>, TContainer, TElement>
		where TContainer : class, ISelectionBoxEntry<TElement>, new()
		where TElement : HudElementBase, IMinLabelElement
	{
		public ChainSelectionBoxBase(HudParentBase parent = null) : base(parent) { }
	}

	/// <summary>
	/// Abstract selection box that uses a <see cref="ScrollBox{TContainer, TElement}"/> as its backing list.
	/// Supports scrolling (mouse wheel + scrollbar).
	/// </summary>
	/// <typeparam name="TContainer">
	/// Container type that wraps each list entry and implements <see cref="ISelectionBoxEntry{TElement}"/>
	/// </typeparam>
	/// <typeparam name="TElement">
	/// The actual UI element displayed for each entry (must have at least a minimal label)
	/// </typeparam>
	public abstract class ScrollSelectionBoxBase<TContainer, TElement>
		: SelectionBoxBase<ScrollBox<TContainer, TElement>, TContainer, TElement>
		where TElement : HudElementBase, IMinLabelElement
		where TContainer : class, ISelectionBoxEntry<TElement>, new()
	{
		/// <summary>
		/// Background color of the scrollable area.
		/// </summary>
		public Color Color { get { return EntryChain.Color; } set { EntryChain.Color = value; } }

		/// <summary>
		/// If true, scrolling via mouse wheel and the scrollbar is enabled.
		/// </summary>
		public virtual bool EnableScrolling { get { return EntryChain.EnableScrolling; } set { EntryChain.EnableScrolling = value; } }

		/// <summary>
		/// If true, enables smooth scrolling and clips content to the visible range.
		/// </summary>
		public virtual bool UseSmoothScrolling { get { return EntryChain.UseSmoothScrolling; } set { EntryChain.UseSmoothScrolling = value; } }

		/// <summary>
		/// Width available for the highlight/selection overlay (accounts for padding and scrollbar).
		/// </summary>
		protected override float HighlightWidth =>
			EntryChain.Size.X - Padding.X - EntryChain.ScrollBar.Width - EntryChain.Padding.X - HighlightPadding.X;

		public ScrollSelectionBoxBase(HudParentBase parent = null) : base(parent) { }

		/// <summary>
		/// Scrolls the view when navigation moves the highlight outside the currently visible range.
		/// </summary>
		protected override void HandleInput(Vector2 cursorPos)
		{
			if (listInput.KeyboardScroll)
			{
				if (listInput.HighlightIndex > EntryChain.End)
					EntryChain.End = listInput.HighlightIndex;
				else if (listInput.HighlightIndex < EntryChain.Start)
					EntryChain.Start = listInput.HighlightIndex;
			}
		}
	}

	/// <summary>
	/// Core abstract base class for selection box controls. Manages a list of entries,
	/// keyboard/mouse selection, highlighting, and visual feedback (selection box + highlight box).
	/// </summary>
	/// <typeparam name="TChain">
	/// The concrete <see cref="HudChain{TContainer, TElement}"/> type used as the list container 
	/// (e.g. <see cref="HudChain{TContainer, TElement}"/> or  <see cref="ScrollBox{TContainer, TElement}"/>)
	/// </typeparam>
	/// <typeparam name="TContainer">Container type wrapping each entry</typeparam>
	/// <typeparam name="TElement">UI element type displayed inside each container</typeparam>
	public abstract class SelectionBoxBase<TChain, TContainer, TElement>
		: HudElementBase, IEntryBox<TContainer, TElement>, IClickableElement
		where TElement : HudElementBase, IMinLabelElement
		where TChain : HudChain<TContainer, TElement>, new()
		where TContainer : class, ISelectionBoxEntry<TElement>, new()
	{
		/// <summary>
		/// Raised when the selected entry changes.
		/// </summary>
		public event EventHandler ValueChanged
		{
			add { listInput.SelectionChanged += value; }
			remove { listInput.SelectionChanged -= value; }
		}

		/// <summary>
		/// Convenience property for adding a selection callback during object initialization.
		/// </summary>
		public EventHandler UpdateValueCallback { set { listInput.SelectionChanged += value; } }

		/// <summary>
		/// Allows nested collection-initializer syntax (e.g., new MyListBox { ListContainer = { entry1, entry2 } });
		/// </summary>
		public SelectionBoxBase<TChain, TContainer, TElement> ListContainer => this;

		/// <summary>
		/// Read-only access to the list of entry containers.
		/// </summary>
		public IReadOnlyList<TContainer> EntryList => EntryChain.Collection;

		/// <summary>
		/// Default background color of the highlight/selection overlay when not focused.
		/// </summary>
		public Color HighlightColor { get; set; }

		/// <summary>
		/// Background color of the selection overlay when the list has input focus.
		/// </summary>
		public Color FocusColor { get; set; }

		/// <summary>
		/// Color of the small vertical tab drawn on the left side of the highlight/selection box.
		/// </summary>
		public Color TabColor
		{
			get { return selectionBox.TabColor; }
			set
			{
				selectionBox.TabColor = value;
				highlightBox.TabColor = value;
			}
		}

		/// <summary>
		/// Additional padding applied inside the highlight/selection overlay.
		/// </summary>
		public Vector2 HighlightPadding { get; set; }

		/// <summary>
		/// Default text format applied to entry labels.
		/// </summary>
		public GlyphFormat Format { get; set; }

		/// <summary>
		/// Text color used for the currently focused entry (keyboard navigation).
		/// </summary>
		public Color FocusTextColor { get; set; }

		/// <summary>
		/// Currently selected entry, or null if nothing is selected.
		/// </summary>
		public TContainer Value => listInput.Selection;

		/// <summary>
		/// Index of the currently selected entry (-1 if none).
		/// </summary>
		public int SelectionIndex => listInput.SelectionIndex;

		/// <summary>
		/// Number of entries in the list.
		/// </summary>
		public int Count => EntryChain.Count;

		/// <summary>
		/// Handles keyboard/mouse focus for the entire control.
		/// </summary>
		public IFocusHandler FocusHandler { get; }

		/// <summary>
		/// Mouse input handler for the list (used for hover/click detection).
		/// </summary>
		public IMouseInput MouseInput => listInput;

		/// <summary>
		/// True when the mouse cursor is over any part of the list.
		/// </summary>
		public override bool IsMousedOver => listInput.IsMousedOver;

        /// <summary>
        /// Range of entry indices currently considered visible. Override in derived classes
        /// (e.g., scrollable versions) to limit highlighting to the visible portion.
        /// </summary>
        /// <exclude/>
        protected virtual Vector2I ListRange => new Vector2I(0, EntryChain.Count - 1);

        /// <summary>
        /// Size of the rendered list area (including padding).
        /// </summary>
        /// <exclude/>
        protected virtual Vector2 ListSize => EntryChain.Size;

        /// <summary>
        /// Position of the list's center.
        /// </summary>
        /// <exclude/>
        protected virtual Vector2 ListPos => EntryChain.Position;

        /// <summary>
        /// Width available for the highlight/selection overlay (excludes outer padding and scrollbar).
        /// </summary>
        /// <exclude/>
        protected virtual float HighlightWidth => EntryChain.Size.X - Padding.X - EntryChain.Padding.X - HighlightPadding.X;

        /// <summary>
        /// UI element that owns and handles entry layout
        /// </summary>
        /// <exclude/>
        public readonly TChain EntryChain;

        /// <summary>
        /// Highlight and focus boxes for entries
        /// </summary>
        /// <exclude/>
        protected readonly HighlightBox selectionBox, highlightBox;

        /// <summary>
        /// Input handler for scrolling and selecting entries
        /// </summary>
        /// <exclude/>
        protected readonly ListInputElement<TContainer, TElement> listInput;

        /// <summary>
        /// True if the entry chain doesn't automatically hide disabled entries
        /// </summary>
        /// <exclude/>
        protected readonly bool chainHidesDisabled;

        /// <summary>
        /// Stores the previously focused/selected entry and its original text format so it can be restored.
        /// </summary>
        /// <exclude/>
        protected MyTuple<TContainer, GlyphFormat> lastSelection;

        /// <exclude/>
        protected SelectionBoxBase(HudParentBase parent = null) : base(parent)
		{
			EntryChain = new TChain
			{
				AlignVertical = true,
				SizingMode = HudChainSizingModes.FitMembersOffAxis,
				DimAlignment = DimAlignments.UnpaddedSize,
			};
			EntryChain.Register(this);

			chainHidesDisabled = EntryChain is ScrollBox<TContainer, TElement>;

			selectionBox = new HighlightBox(EntryChain) { Visible = false };
			highlightBox = new HighlightBox(EntryChain) { Visible = false, CanDrawTab = false };

			FocusHandler = new InputFocusHandler(this);
			listInput = new ListInputElement<TContainer, TElement>(this, EntryChain) { ZOffset = 1 };

			HighlightColor = TerminalFormatting.Atomic;
			FocusColor = TerminalFormatting.Mint;
			Format = TerminalFormatting.ControlFormat;
			FocusTextColor = TerminalFormatting.Charcoal;

			Size = new Vector2(335f, 203f);
			HighlightPadding = new Vector2(8f, 0f);
		}

		public IEnumerator<TContainer> GetEnumerator() => EntryChain.Collection.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Sets the selection to the member associated with the given object.
		/// </summary>
		public void SetSelectionAt(int index) =>
			listInput.SetSelectionAt(index);

		/// <summary>
		/// Offsets selection index in the direction of the offset. If wrap == true, the index will wrap around
		/// if the offset places it out of range.
		/// </summary>
		public void OffsetSelectionIndex(int offset, bool wrap = false) =>
			listInput.OffsetSelectionIndex(offset, wrap);

		/// <summary>
		/// Sets the selection to the specified entry.
		/// </summary>
		public void SetSelection(TContainer member) =>
			listInput.SetSelection(member);

		/// <summary>
		/// Clears the current selection
		/// </summary>
		public void ClearSelection() =>
			listInput.ClearSelection();

		/// <summary>
		/// Calculates the combined size of the list entries within the given index range.
		/// </summary>
		/// <param name="start">Start index.</param>
		/// <param name="end">End index (-1 for last element).</param>
		/// <returns>Total size vector (Width, Height) required to fit the range.</returns>
		public virtual Vector2 GetRangeSize(int start = 0, int end = -1) => EntryChain.GetRangeSize(start, end);

        /// <summary>
        /// Updates visibility of disabled entries (if not handled by chain),
        /// positions highlight/selection boxes, and refreshes input bounding info.
        /// </summary>
        /// <exclude/>
        protected override void Layout()
		{
			if (!chainHidesDisabled)
			{
				foreach (TContainer entry in EntryChain)
					entry.Element.Visible = entry.Enabled;
			}

			highlightBox.Visible = false;
			selectionBox.Visible = false;

			if (EntryChain.Count > 0)
				UpdateSelection();

			listInput.ListSize = ListSize;
			listInput.ListPos = ListPos;
			listInput.ListRange = ListRange;
		}

        /// <summary>
        /// Updates highlight/selection overlay positions and text formatting based on current input state.
        /// </summary>
        /// <exclude/>
        protected virtual void UpdateSelection()
		{
			UpdateSelectionPositions();
			UpdateSelectionFormatting();
		}

        /// <summary>
        /// Repositions the selection and highlight overlay boxes to match the current selection/highlight.
        /// </summary>
        /// <exclude/>
        protected virtual void UpdateSelectionPositions()
		{
			float entryWidth = HighlightWidth;

			// Selection box (always follows the selected entry)
			if (Value != null && Value.Element.Visible)
			{
				Vector2 offset = Value.Element.Position - selectionBox.Origin;
				offset.X -= (ListSize.X - entryWidth - HighlightPadding.X) / 2f;

				selectionBox.Offset = offset;
				selectionBox.Height = Value.Element.Height - HighlightPadding.Y;
				selectionBox.Width = entryWidth;
				selectionBox.Visible = Value.Element.Visible && Value.AllowHighlighting;
			}

			// Highlight box (mouse hover or keyboard focus)
			if (listInput.HighlightIndex != listInput.SelectionIndex)
			{
				var entry = EntryChain[listInput.HighlightIndex];
				Vector2 offset = entry.Element.Position - highlightBox.Origin;
				offset.X -= (ListSize.X - entryWidth - HighlightPadding.X) / 2f;

				highlightBox.Visible = (listInput.IsMousedOver || FocusHandler.HasFocus)
					&& entry.Element.Visible && entry.AllowHighlighting;

				highlightBox.Height = entry.Element.Height - HighlightPadding.Y;
				highlightBox.Width = entryWidth;
				highlightBox.Offset = offset;
			}
		}

        /// <summary>
        /// Applies appropriate colors and text formatting based on focus, keyboard scrolling, and mouse hover state.
        /// </summary>
        /// <exclude/>
        protected virtual void UpdateSelectionFormatting()
		{
			if (lastSelection.Item1 != null)
			{
				ITextBoard textBoard = lastSelection.Item1.Element.TextBoard;
				textBoard.SetFormatting(lastSelection.Item2);
				lastSelection.Item1 = null;
			}

			if ((SelectionIndex == listInput.FocusIndex) && SelectionIndex != -1)
			{
				if (
					(listInput.KeyboardScroll ^ (SelectionIndex != listInput.HighlightIndex)) ||
					(!MouseInput.IsMousedOver && SelectionIndex == listInput.HighlightIndex)
				)
				{
					if (EntryChain[listInput.SelectionIndex].AllowHighlighting)
					{
						SetFocusFormat(listInput.SelectionIndex);
						selectionBox.Color = FocusColor;
					}
				}
				else
					selectionBox.Color = HighlightColor;

				highlightBox.Color = HighlightColor;
			}
			else
			{
				if (listInput.KeyboardScroll)
				{
					if (EntryChain[listInput.HighlightIndex].AllowHighlighting)
					{
						SetFocusFormat(listInput.HighlightIndex);
						highlightBox.Color = FocusColor;
					}
				}
				else
					highlightBox.Color = HighlightColor;

				selectionBox.Color = HighlightColor;
			}
		}

		/// <summary>
		/// Temporarily changes the text color of the entry at the given index to <see cref="FocusTextColor"/>
		/// while storing its original format for later restoration.
		/// </summary>
		/// <exclude/>
		protected void SetFocusFormat(int index)
		{
			var entry = EntryChain[index];
			var textBoard = entry.Element.TextBoard;

			lastSelection.Item1 = entry;
			lastSelection.Item2 = textBoard.Format;

			textBoard.SetFormatting(textBoard.Format.WithColor(FocusTextColor));
		}

		/// <summary>
		/// Internal textured box used for selection and highlight overlays. Draws a colored background
		/// with an optional thin vertical tab on the left side.
		/// </summary>
		/// <exclude/>
		protected class HighlightBox : TexturedBox
		{
			/// <summary>
			/// If false, the left-side tab will not be drawn (used for the transient highlight box).
			/// </summary>
			public bool CanDrawTab { get; set; } = true;

			/// <summary>
			/// Color of the left-side tab.
			/// </summary>
			public Color TabColor { get { return tabBoard.Color; } set { tabBoard.Color = value; } }

			private readonly MatBoard tabBoard;

            /// <exclude/>
            public HighlightBox(HudParentBase parent = null) : base(parent)
			{
				tabBoard = new MatBoard() { Color = TerminalFormatting.Mercury };
				Color = TerminalFormatting.Atomic;
				IsSelectivelyMasked = true;
			}

            /// <exclude/>
            protected override void Draw()
			{
				var box = default(CroppedBox);
				Vector2 size = UnpaddedSize,
						halfSize = size * 0.5f;

				box.bounds = new BoundingBox2(Position - halfSize, Position + halfSize);
				box.mask = MaskingBox;

				if (hudBoard.Color.A > 0)
					hudBoard.Draw(ref box, HudSpace.PlaneToWorldRef);

				if (CanDrawTab && tabBoard.Color.A > 0)
				{
					Vector2 tabPos = Position;
					Vector2 tabSize = new Vector2(4f, size.Y - Padding.Y) * 0.5f;
					tabPos.X += (-size.X + tabSize.X) * 0.5f; // left align

					box.bounds = new BoundingBox2(tabPos - tabSize, tabPos + tabSize);
					tabBoard.Draw(ref box, HudSpace.PlaneToWorldRef);
				}
			}
		}
	}
}