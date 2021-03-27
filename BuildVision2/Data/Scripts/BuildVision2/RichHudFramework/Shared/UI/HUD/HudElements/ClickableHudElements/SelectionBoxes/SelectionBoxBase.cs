using System;
using System.Text;
using VRage;
using VRageMath;
using System.Collections.Generic;
using RichHudFramework.UI.Rendering;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;
using System.Collections;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Generic SelectionBox using HudChain
    /// </summary>
    /// <typeparam name="TContainer">Container element type wrapping the UI element</typeparam>
    /// <typeparam name="TElement">UI element in the list</typeparam>
    public class ChainSelectionBoxBase<TContainer, TElement>
        : SelectionBoxBase<HudChain<TContainer, TElement>, TContainer, TElement>
        where TContainer : class, ISelectionBoxEntry<TElement>, new()
        where TElement : HudElementBase, IMinLabelElement
    {
        public ChainSelectionBoxBase(HudParentBase parent) : base(parent)
        { }

        public ChainSelectionBoxBase() : base(null)
        { }
    }

    /// <summary>
    /// Generic SelectionBox using ScrollBox
    /// </summary>
    /// <typeparam name="TContainer">Container element type wrapping the UI element</typeparam>
    /// <typeparam name="TElement">UI element in the list</typeparam>
    public class ScrollSelectionBoxBase<TContainer, TElement>
        : SelectionBoxBase<ScrollBox<TContainer, TElement>, TContainer, TElement>
        where TElement : HudElementBase, IMinLabelElement
        where TContainer : class, ISelectionBoxEntry<TElement>, new()
    {
        public ScrollSelectionBoxBase(HudParentBase parent) : base(parent)
        { }

        public ScrollSelectionBoxBase() : base(null)
        { }
    }

    /// <summary>
    /// Abstract generic list of selectable UI elements of arbitrary size size.
    /// </summary>
    /// <typeparam name="TContainer">Container element type wrapping the UI element</typeparam>
    /// <typeparam name="TElement">UI element in the list</typeparam>
    /// <typeparam name="TChain">HudChain type used by the SelectionBox as the list container</typeparam>
    public class SelectionBoxBase<TChain, TContainer, TElement> 
        : HudElementBase, IEntryBox<TContainer, TElement>, IClickableElement
        where TElement : HudElementBase, IMinLabelElement
        where TChain : HudChain<TContainer, TElement>, new()
        where TContainer : class, ISelectionBoxEntry<TElement>, new()
    {
        /// <summary>
        /// Invoked when an entry is selected.
        /// </summary>
        public event EventHandler SelectionChanged
        {
            add { listInput.SelectionChanged += value; }
            remove { listInput.SelectionChanged -= value; }
        }

        /// <summary>
        /// Used to allow the addition of list entries using collection-initializer syntax in
        /// conjunction with normal initializers.
        /// </summary>
        public SelectionBoxBase<TChain, TContainer, TElement> ListContainer => this;

        /// <summary>
        /// Read-only collection of list entries.
        /// </summary>
        public IReadOnlyList<TContainer> EntryList => hudChain.Collection;

        /// <summary>
        /// Read-only collection of list entries.
        /// </summary>
        public IReadOnlyHudCollection<TContainer, TElement> HudCollection => hudChain;

        /// <summary>
        /// Default background color of the highlight box
        /// </summary>
        public Color HighlightColor { get; set; }

        /// <summary>
        /// Background color used for selection/highlighting when the list has input focus
        /// </summary>
        public Color FocusColor { get; set; }

        /// <summary>
        /// Color of the highlight box's tab
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
        /// Padding applied to the highlight box.
        /// </summary>
        public Vector2 HighlightPadding { get; set; }

        /// <summary>
        /// Default format for member text;
        /// </summary>
        public GlyphFormat Format { get; set; }

        /// <summary>
        /// Text formatting used for entries that have input focus
        /// </summary>
        public GlyphFormat FocusFormat { get; set; }

        /// <summary>
        /// Current selection. Null if empty.
        /// </summary>
        public TContainer Selection => listInput.Selection;

        /// <summary>
        /// Index of the current selection. -1 if empty.
        /// </summary>
        public int SelectionIndex => listInput.SelectionIndex;

        /// <summary>
        /// Size of the entry collection.
        /// </summary>
        public int Count => hudChain.Count;

        /// <summary>
        /// Mouse input element for the selection box
        /// </summary>
        public IMouseInput MouseInput => listInput;

        /// <summary>
        /// Defines the range of elements visible
        /// </summary>
        protected virtual Vector2I ListRange => new Vector2I(0, hudChain.Count - 1);

        /// <summary>
        /// Size of the list, as rendered
        /// </summary>
        protected virtual Vector2 ListSize => hudChain.Size;

        /// <summary>
        /// Position of the list's center
        /// </summary>
        protected virtual Vector2 ListPos => hudChain.Position;

        public readonly TChain hudChain;
        protected readonly HighlightBox selectionBox, highlightBox;
        protected readonly ListInputElement<TContainer, TElement> listInput;
        protected readonly bool chainHidesDisabled;

        public SelectionBoxBase(HudParentBase parent) : base(parent)
        {
            hudChain = new TChain()
            {
                AlignVertical = true,
                SizingMode = 
                    HudChainSizingModes.FitMembersOffAxis | 
                    HudChainSizingModes.ClampMembersAlignAxis | 
                    HudChainSizingModes.ClampChainOffAxis,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
            };
            hudChain.Register(this);
            chainHidesDisabled = hudChain is ScrollBox<TContainer, TElement>;

            selectionBox = new HighlightBox() { Visible = false };
            highlightBox = new HighlightBox() { Visible = false, CanDrawTab = false };

            selectionBox.Register(hudChain, false, true);
            highlightBox.Register(hudChain, false, true);

            listInput = new ListInputElement<TContainer, TElement>(hudChain);

            HighlightColor = TerminalFormatting.Atomic;
            FocusColor = TerminalFormatting.Mint;

            Format = TerminalFormatting.ControlFormat;
            FocusFormat = TerminalFormatting.InvControlFormat;
            Size = new Vector2(335f, 203f);

            HighlightPadding = new Vector2(8f, 0f);
        }

        public SelectionBoxBase() : this(null)
        { }

        public IEnumerator<TContainer> GetEnumerator() =>
            hudChain.Collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelectionAt(int index) =>
            listInput.SetSelectionAt(index);

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

        protected override void Layout()
        {
            if (!chainHidesDisabled)
            {
                foreach (TContainer entry in hudChain)
                {
                    entry.Element.Visible = entry.Enabled;
                }
            }
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            highlightBox.Visible = false;
            selectionBox.Visible = false;

            if (hudChain.Count > 0)
            {
                UpdateSelection();
            }

            listInput.ListSize = ListSize;
            listInput.ListPos = ListPos;
            listInput.ListRange = ListRange;
        }

        /// <summary>
        /// Update indices for selections, highlight and focus
        /// </summary>
        protected virtual void UpdateSelection()
        {
            UpdateSelectionPositions();
            UpdateSelectionFormatting();
        }

        protected virtual void UpdateSelectionPositions()
        {
            // Make sure the selection box highlights the current selection
            if (Selection != null && Selection.Element.Visible)
            {
                selectionBox.Offset = Selection.Element.Position - selectionBox.Origin;
                selectionBox.Size = Selection.Element.Size - HighlightPadding;
                selectionBox.Visible = Selection.Element.Visible && Selection.AllowHighlighting;
            }

            // If highlight and selection indices dont match, draw highlight box
            if (listInput.HighlightIndex != listInput.SelectionIndex)
            {
                TContainer entry = hudChain[listInput.HighlightIndex];

                highlightBox.Visible = 
                    (listInput.IsMousedOver || listInput.HasFocus) 
                    && entry.Element.Visible && entry.AllowHighlighting;

                highlightBox.Size = entry.Element.Size - HighlightPadding;
                highlightBox.Offset = entry.Element.Position - highlightBox.Origin;
            }
        }

        protected virtual void UpdateSelectionFormatting()
        {
            // Update Selection/Highlight Formatting
            for (int i = ListRange.X; i <= ListRange.Y; i++)
                hudChain[i].Element.TextBoard.SetFormatting(Format);

            if ((SelectionIndex == listInput.FocusIndex) && SelectionIndex != -1)
            {
                if (
                    (listInput.KeyboardScroll ^ (SelectionIndex != listInput.HighlightIndex)) ||
                    (!MouseInput.IsMousedOver && SelectionIndex == listInput.HighlightIndex)
                )
                {
                    if (hudChain[SelectionIndex].AllowHighlighting)
                    {
                        selectionBox.Color = FocusColor;
                        hudChain[SelectionIndex].Element.TextBoard.SetFormatting(FocusFormat);
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
                    if (hudChain[SelectionIndex].AllowHighlighting)
                    {
                        highlightBox.Color = FocusColor;
                        hudChain[listInput.HighlightIndex].Element.TextBoard.SetFormatting(FocusFormat);
                    }
                }
                else
                    highlightBox.Color = HighlightColor;

                selectionBox.Color = HighlightColor;
            }
        }

        protected override void Draw()
        {
            Size = hudChain.Size + Padding;
        }

        /// <summary>
        /// A textured box with a white tab positioned on the left hand side.
        /// </summary>
        protected class HighlightBox : TexturedBox
        {
            public bool CanDrawTab { get; set; }

            public Color TabColor { get { return tabBoard.Color; } set { tabBoard.Color = value; } }

            private readonly MatBoard tabBoard;

            public HighlightBox(HudParentBase parent = null) : base(parent)
            {
                tabBoard = new MatBoard() { Color = TerminalFormatting.Mercury };
                Color = TerminalFormatting.Atomic;
                CanDrawTab = true;
            }

            protected override void Layout()
            {
                hudBoard.Size = cachedSize - cachedPadding;
                tabBoard.Size = new Vector2(4f * Scale, cachedSize.Y - cachedPadding.Y);
            }

            protected override void Draw()
            {
                var ptw = HudSpace.PlaneToWorld;

                if (hudBoard.Color.A > 0)
                    hudBoard.Draw(cachedPosition, ref ptw);

                // Left align the tab
                Vector2 tabPos = cachedPosition;
                tabPos.X += (-hudBoard.Size.X + tabBoard.Size.X) / 2f;

                if (CanDrawTab && tabBoard.Color.A > 0)
                    tabBoard.Draw(tabPos, ref ptw);
            }
        }
    }
}
