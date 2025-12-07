using System;
using System.Collections;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI
{
	using Rendering;

	/// <summary>
	/// Abstract, generic base class for tree-style dropdown lists
	/// Provides a collapsible hierarchy of entries with a header that toggles visibility of the list.
	/// <para>
	/// Alias for <see cref="TreeBoxBase{TSelectionBox, TChain, TContainer, TElement}"/>
	/// using default non-scrolling chain and selection box types
	/// </para>
	/// </summary>
	/// <typeparam name="TContainer">
	/// Container type that wraps each entry's UI element and provides selection/association data.
	/// </typeparam>
	/// <typeparam name="TElement">
	/// The actual UI element displayed for each entry (must support minimal labeling).
	/// </typeparam>
	public abstract class TreeBoxBase<TContainer, TElement> : TreeBoxBase<
		TreeBoxBase<TContainer, TElement>.TreeChainSelectionBox,
		HudChain<TContainer, TElement>,
		TContainer,
		TElement>
		where TContainer : class, ISelectionBoxEntry<TElement>, new()
		where TElement : HudElementBase, IMinLabelElement
	{
		public TreeBoxBase(HudParentBase parent) : base(parent) { }
		public TreeBoxBase() : base(null) { }

		/// <summary>
		/// Minimal <see cref="ChainSelectionBoxBase{TContainer, TElement}"/> used internally 
		/// by <see cref="TreeBoxBase{TContainer, TElement}"/>.
		/// </summary>
		public class TreeChainSelectionBox : ChainSelectionBoxBase<TContainer, TElement>
		{ }
	}

	/// <summary>
	/// Fully generic abstract base for tree box controls. Combines a clickable header (with expand/collapse arrow)
	/// and a dropdown selection list that appears below it when opened.
	/// </summary>
	/// <typeparam name="TSelectionBox">Type of the selection box that manages the dropdown list.</typeparam>
	/// <typeparam name="TChain">Type of <see cref="HudChain{TContainer, TElement}"/> used inside the selection box.</typeparam>
	/// <typeparam name="TContainer">Container type wrapping each entry's UI element.</typeparam>
	/// <typeparam name="TElement">UI element type displayed for each entry.</typeparam>
	public abstract class TreeBoxBase<TSelectionBox, TChain, TContainer, TElement>
		: LabelElementBase, IEntryBox<TContainer, TElement>, IClickableElement
		where TElement : HudElementBase, IMinLabelElement
		where TContainer : class, ISelectionBoxEntry<TElement>, new()
		where TChain : HudChain<TContainer, TElement>, new()
		where TSelectionBox : SelectionBoxBase<TChain, TContainer, TElement>, new()
	{
		/// <summary>
		/// Raised when the selected entry changes.
		/// </summary>
		public event EventHandler ValueChanged
		{
			add { selectionBox.ValueChanged += value; }
			remove { selectionBox.ValueChanged -= value; }
		}

		/// <summary>
		/// Allows registering a <see cref="ValueChanged"/> in an initializer block.
		/// </summary>
		public EventHandler UpdateValueCallback { set { selectionBox.ValueChanged += value; } }

		/// <summary>
		/// Read-only access to the full list of entries currently in the tree box.
		/// </summary>
		public IReadOnlyList<TContainer> EntryList => selectionBox.EntryList;

		/// <summary>
		/// Enables nested collection-initializer syntax (e.g., new MyTreeBox { ListContainer = { entry1, entry2 } })
		/// </summary>
		public TreeBoxBase<TSelectionBox, TChain, TContainer, TElement> ListContainer => this;

		/// <summary>
		/// Indicates whether the dropdown list is currently visible.
		/// </summary>
		public bool ListOpen { get; protected set; }

		/// <summary>
		/// Gets or sets the height of the dropdown list when open.
		/// </summary>
		public float DropdownHeight
		{
			get { return selectionBox.Height; }
			set { selectionBox.Height = value; }
		}

		/// <summary>
		/// Horizontal offset applied to each level of nested/child entries
		/// </summary>
		public float IndentSize { get; set; }

		/// <summary>
		/// Text displayed in the header of the tree box.
		/// </summary>
		public RichText Name
		{
			get { return labelButton.Name; }
			set { labelButton.Name = value; }
		}

		/// <summary>
		/// Provides direct access to the <see cref="ITextBoard"/> used by the header label.
		/// </summary>
		public override ITextBoard TextBoard => labelButton.nameLabel.TextBoard;

		/// <summary>
		/// Default text formatting applied to both the header and all list entries.
		/// </summary>
		public GlyphFormat Format
		{
			get { return labelButton.Format; }
			set
			{
				labelButton.Format = value;
				selectionBox.Format = value;
			}
		}

		/// <summary>
		/// Height of the clickable header
		/// </summary>
		public float LabelHeight
		{
			get { return labelButton.Height; }
			set { labelButton.Height = value; }
		}

		/// <summary>
		/// Text color used for the entry that currently has input focus.
		/// </summary>
		public Color FocusTextColor
		{
			get { return selectionBox.FocusTextColor; }
			set { selectionBox.FocusTextColor = value; }
		}

		/// <summary>
		/// Background color of the header.
		/// </summary>
		public Color HeaderColor
		{
			get { return labelButton.Color; }
			set { labelButton.Color = value; }
		}

		/// <summary>
		/// Default highlight color for entries (when hovered without input focus).
		/// </summary>
		public Color HighlightColor
		{
			get { return selectionBox.HighlightColor; }
			set { selectionBox.HighlightColor = value; }
		}

		/// <summary>
		/// Color used for highlighting focused entries
		/// </summary>
		public Color FocusColor
		{
			get { return selectionBox.FocusColor; }
			set { selectionBox.FocusColor = value; }
		}

		/// <summary>
		/// Color of the vertical tab next to a selection
		/// </summary>
		public Color TabColor
		{
			get { return selectionBox.TabColor; }
			set { selectionBox.TabColor = value; }
		}

		/// <summary>
		/// Padding around the selection highlight in the dropdown list.
		/// </summary>
		public Vector2 HighlightPadding
		{
			get { return selectionBox.HighlightPadding; }
			set { selectionBox.HighlightPadding = value; }
		}

		/// <summary>
		/// Currently selected entry, or <c>null</c> if nothing is selected.
		/// </summary>
		public TContainer Value => selectionBox.Value;

		/// <summary>
		/// Number of entries currently in the dropdown list.
		/// </summary>
		public int Count => selectionBox.Count;

		/// <summary>
		/// Handles keyboard/input focus for the entire tree box (header + list).
		/// </summary>
		public IFocusHandler FocusHandler => labelButton.FocusHandler;

		/// <summary>
		/// Mouse input handler for the header (used for clicking to open/close the list).
		/// </summary>
		public IMouseInput MouseInput => labelButton.MouseInput;

		/// <summary>
		/// The dropdown selection box that contains all list entries and manages selection logic.
		/// </summary>
		public readonly TSelectionBox selectionBox;

		/// <summary>
		/// Custom header control with expand/collapse arrow and label.
		/// </summary>
		/// <exclude/>
		protected readonly TreeBoxDisplay labelButton;

        /// <exclude/>
        protected TreeBoxBase(HudParentBase parent) : base(parent)
		{
			labelButton = new TreeBoxDisplay(this)
			{
				ParentAlignment = ParentAlignments.PaddedInnerTop,
				DimAlignment = DimAlignments.UnpaddedWidth
			};

			selectionBox = new TSelectionBox()
			{
				Visible = false,
				ParentAlignment = ParentAlignments.Bottom,
				HighlightPadding = Vector2.Zero
			};

			selectionBox.Register(labelButton);
			selectionBox.FocusHandler.InputOwner = this;
			selectionBox.EntryChain.SizingMode = HudChainSizingModes.FitMembersOffAxis;

			Width = 200f;
			LabelHeight = 34f;
			IndentSize = 20f;
			DropdownHeight = 100f;
			FocusHandler.InputOwner = this;
			Format = GlyphFormat.Blueish;
			labelButton.Name = "NewTreeBox";

			labelButton.MouseInput.LeftClicked += ToggleList;
		}

        /// <exclude/>
        protected TreeBoxBase() : this(null) { }

		/// <summary>
		/// Selects the specified entry.
		/// </summary>
		public void SetSelection(TContainer member) => selectionBox.SetSelection(member);

		/// <summary>
		/// Selects the entry at the given index.
		/// </summary>
		public void SetSelectionAt(int index) => selectionBox.SetSelectionAt(index);

		/// <summary>
		/// Clears the current selection.
		/// </summary>
		public void ClearSelection() => selectionBox.ClearSelection();

        /// <summary>
        /// Toggles the visibility of the dropdown list.
        /// </summary>
        /// <exclude/>
        protected virtual void ToggleList(object sender, EventArgs args)
		{
			if (!ListOpen)
				OpenList();
			else
				CloseList();
		}

		/// <summary>
		/// Opens the dropdown list.
		/// </summary>
		public void OpenList()
		{
			labelButton.Open = true;
			ListOpen = true;
		}

		/// <summary>
		/// Closes the dropdown list.
		/// </summary>
		public void CloseList()
		{
			labelButton.Open = false;
			ListOpen = false;
		}

        /// <summary>
        /// Updates the size of the tree box and its dropdown list.
        /// </summary>
        /// <exclude/>
        protected override void Measure()
		{
			selectionBox.Visible = ListOpen;

			if (ListOpen)
			{
				Height = selectionBox.GetRangeSize().Y + labelButton.Height + Padding.Y;
				selectionBox.Width = Size.X - Padding.X - 2f * IndentSize;
				selectionBox.Offset = new Vector2(IndentSize, 0f);
				selectionBox.Height = Size.Y - labelButton.Height - Padding.Y;
			}
			else
			{
				Height = labelButton.Height + Padding.Y;
			}
		}

		public IEnumerator<TContainer> GetEnumerator() => selectionBox.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => selectionBox.GetEnumerator();

        /// <summary>
        /// Custom header element for the tree box featuring a rotating arrow (right = closed, down = open),
        /// a vertical divider, and a label.
        /// </summary>
        /// <exclude/>
        protected class TreeBoxDisplay : HudElementBase, IClickableElement
		{
			/// <summary>
			/// Text displayed in the header.
			/// </summary>
			public RichText Name
			{
				get { return nameLabel.Text; }
				set { nameLabel.Text = value; }
			}

			/// <summary>
			/// Text formatting for the header label.
			/// </summary>
			public GlyphFormat Format
			{
				get { return nameLabel.Format; }
				set { nameLabel.Format = value; }
			}

			/// <summary>
			/// Background color of the header.
			/// </summary>
			public Color Color
			{
				get { return background.Color; }
				set { background.Color = value; }
			}

			public IFocusHandler FocusHandler { get; }
			public IMouseInput MouseInput => mouseInput;

			/// <summary>
			/// Gets or sets whether the dropdown is open. Setting this rotates the arrow accordingly.
			/// </summary>
			public bool Open
			{
				get { return _open; }
				set
				{
					_open = value;
					arrow.Material = _open ? downArrow : rightArrow;
				}
			}

			private bool _open;

			public readonly Label nameLabel;
			private readonly TexturedBox arrow, divider, background;
			private readonly MouseInputElement mouseInput;

			private static readonly Material downArrow = new Material("RichHudDownArrow", new Vector2(64f, 64f));
			private static readonly Material rightArrow = new Material("RichHudRightArrow", new Vector2(64f, 64f));

			public TreeBoxDisplay(HudParentBase parent) : base(parent)
			{
				background = new TexturedBox(this)
				{
					Color = TerminalFormatting.EbonyClay,
					DimAlignment = DimAlignments.Size,
				};

				nameLabel = new Label()
				{
					AutoResize = false,
					Padding = new Vector2(10f, 0f),
					Format = GlyphFormat.Blueish.WithSize(1.1f),
				};

				divider = new TexturedBox()
				{
					Padding = new Vector2(2f, 6f),
					Size = new Vector2(2f, 39f),
					Color = new Color(104, 113, 120),
				};

				arrow = new TexturedBox()
				{
					Width = 20f,
					Padding = new Vector2(8f, 0f),
					MatAlignment = MaterialAlignment.FitHorizontal,
					Color = new Color(227, 230, 233),
					Material = rightArrow,
				};

				var layout = new HudChain(false, this)
				{
					SizingMode = HudChainSizingModes.FitMembersOffAxis,
					DimAlignment = DimAlignments.UnpaddedSize,
					CollectionContainer = { arrow, divider, { nameLabel, 1f } }
				};

				FocusHandler = new InputFocusHandler(this);
				mouseInput = new MouseInputElement(this)
				{
					DimAlignment = DimAlignments.Size
				};
			}
		}
	}
}