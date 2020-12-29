using System;
using System.Collections.Generic;
using VRageMath;
using VRage;

namespace RichHudFramework.UI
{
    using Rendering;
    using Server;
    using System.Collections;

    /// <summary>
    /// Collapsable list box. Designed to mimic the appearance of the dropdown in the SE terminal.
    /// </summary>
    public class Dropdown<T> : HudElementBase, IEntryBox<T>, IClickableElement
    {
        /// <summary>
        /// Invoked when a member of the list is selected.
        /// </summary>
        public event EventHandler OnSelectionChanged { add { listBox.OnSelectionChanged += value; } remove { listBox.OnSelectionChanged -= value; } }

        /// <summary>
        /// List of entries in the dropdown.
        /// </summary>
        public IReadOnlyList<ListBoxEntry<T>> ListEntries => listBox.ListEntries;

        /// <summary>
        /// Used to allow the addition of list entries using collection-initializer syntax in
        /// conjunction with normal initializers.
        /// </summary>
        public Dropdown<T> ListContainer => this;

        /// <summary>
        /// Height of the dropdown list
        /// </summary>
        public float DropdownHeight { get { return listBox.Height; } set { listBox.Height = value; } }

        /// <summary>
        /// Padding applied to list members.
        /// </summary>
        public Vector2 MemberPadding { get { return listBox.MemberPadding; } set { listBox.MemberPadding = value; } }

        /// <summary>
        /// Height of entries in the dropdown.
        /// </summary>
        public float LineHeight { get { return listBox.LineHeight; } set { listBox.LineHeight = value; } }

        /// <summary>
        /// Default format for member text;
        /// </summary>
        public GlyphFormat Format { get { return listBox.Format; } set { listBox.Format = value; } }

        /// <summary>
        /// Background color of the dropdown list
        /// </summary>
        public Color Color { get { return listBox.Color; } set { listBox.Color = value; } }

        /// <summary>
        /// Color of the slider bar
        /// </summary>
        public Color BarColor { get { return listBox.BarColor; } set { listBox.BarColor = value; } }

        /// <summary>
        /// Bar color when moused over
        /// </summary>
        public Color BarHighlight { get { return listBox.BarHighlight; } set { listBox.BarHighlight = value; } }

        /// <summary>
        /// Color of the slider box when not moused over
        /// </summary>
        public Color SliderColor { get { return listBox.SliderColor; } set { listBox.SliderColor = value; } }

        /// <summary>
        /// Color of the slider button when moused over
        /// </summary>
        public Color SliderHighlight { get { return listBox.SliderHighlight; } set { listBox.SliderHighlight = value; } }

        /// <summary>
        /// Background color of the highlight box
        /// </summary>
        public Color HighlightColor { get { return listBox.HighlightColor; } set { listBox.HighlightColor = value; } }

        /// <summary>
        /// Color of the highlight box's tab
        /// </summary>
        public Color TabColor { get { return listBox.TabColor; } set { listBox.TabColor = value; } }

        /// <summary>
        /// Padding applied to the highlight box.
        /// </summary>
        public Vector2 HighlightPadding { get; set; }

        /// <summary>
        /// Minimum number of elements visible in the list at any given time.
        /// </summary>
        public int MinVisibleCount { get { return listBox.MinVisibleCount; } set { listBox.MinVisibleCount = value; } }

        /// <summary>
        /// Current selection. Null if empty.
        /// </summary>
        public ListBoxEntry<T> Selection => listBox.Selection;

        /// <summary>
        /// Mouse input for the dropdown display.
        /// </summary>
        public IMouseInput MouseInput => display.MouseInput;

        /// <summary>
        /// Indicates whether or not the dropdown is moused over.
        /// </summary>
        public override bool IsMousedOver => display.IsMousedOver || listBox.IsMousedOver;

        /// <summary>
        /// Indicates whether or not the list is open.
        /// </summary>
        public bool Open => listBox.Visible;

        public HudElementBase Display => display;

        public readonly ListBox<T> listBox;

        protected readonly DropdownDisplay display;
        protected readonly TexturedBox highlight;

        public Dropdown(HudParentBase parent) : base(parent)
        {
            display = new DropdownDisplay(this)
            {
                Padding = new Vector2(10f, 0f),
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                Text = "None"
            };

            highlight = new TexturedBox(display)
            {
                Color = TerminalFormatting.HighlightOverlayColor,
                DimAlignment = DimAlignments.Both,
                Visible = false,
            };

            listBox = new ListBox<T>(display)
            {
                Visible = false,
                ZOffset = 1,
                MinVisibleCount = 4,
                DimAlignment = DimAlignments.Width,
                ParentAlignment = ParentAlignments.Bottom,
                TabColor = new Color(0, 0, 0, 0),
            };

            Size = new Vector2(331f, 43f);

            display.MouseInput.OnLeftClick += ToggleList;
            OnSelectionChanged += UpdateDisplay;
        }

        public Dropdown() : this(null)
        { }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (SharedBinds.LeftButton.IsNewPressed && !IsMousedOver)
            {
                CloseList();
            }

            highlight.Visible = IsMousedOver || Open;
        }

        private void UpdateDisplay(object sender, EventArgs args)
        {
            if (Selection != null)
            {
                display.Text = Selection.Element.Text;
                CloseList();
            }
        }

        private void ToggleList(object sender, EventArgs args)
        {
            if (!listBox.Visible)
                OpenList();
            else
                CloseList();
        }

        private void OpenList()
        {
            listBox.Visible = true;
        }

        private void CloseList()
        {
            listBox.Visible = false;
        }

        /// <summary>
        /// Adds a new member to the dropdown with the given name and associated
        /// object.
        /// </summary>
        public ListBoxEntry<T> Add(RichText name, T assocMember, bool enabled = true) =>
            listBox.Add(name, assocMember, enabled);

        /// <summary>
        /// Adds the given range of entries to the dropdown.
        /// </summary>
        public void AddRange(IReadOnlyList<MyTuple<RichText, T, bool>> entries) =>
            listBox.AddRange(entries);

        /// <summary>
        /// Inserts an entry at the given index.
        /// </summary>
        public void Insert(int index, RichText name, T assocMember, bool enabled = true) =>
            listBox.Insert(index, name, assocMember, enabled);

        /// <summary>
        /// Removes the given member from the dropdown.
        /// </summary>
        public void RemoveAt(int index) =>
            listBox.RemoveAt(index);

        /// <summary>
        /// Removes the member at the given index from the dropdown.
        /// </summary>
        public bool Remove(ListBoxEntry<T> entry) =>
            listBox.Remove(entry);

        /// <summary>
        /// Removes the specified range of indices from the dropdown.
        /// </summary>
        public void RemoveRange(int index, int count) =>
            listBox.RemoveRange(index, count);

        /// <summary>
        /// Clears the current contents of the dropdown.
        /// </summary>
        public void ClearEntries() =>
            listBox.ClearEntries();

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelection(T assocMember) =>
            listBox.SetSelection(assocMember);

        /// <summary>
        /// Sets the selection to the specified entry.
        /// </summary>
        public void SetSelection(ListBoxEntry<T> member) =>
            listBox.SetSelection(member);

        public object GetOrSetMember(object data, int memberEnum) =>
            listBox.GetOrSetMember(data, memberEnum);

        public IEnumerator<ListBoxEntry<T>> GetEnumerator() =>
            listBox.ListEntries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        protected class DropdownDisplay : HudElementBase
        {
            private static readonly Material arrowMat = new Material("RichHudDownArrow", new Vector2(64f, 64f));

            public RichText Text { get { return name.Text; } set { name.Text = value; } }

            public GlyphFormat Format { get { return name.Format; } set { name.Format = value; } }

            public Color Color { get { return background.Color; } set { background.Color = value; } }

            public override bool IsMousedOver => mouseInput.IsMousedOver;

            public IMouseInput MouseInput => mouseInput;

            private readonly Label name;
            private readonly TexturedBox arrow, divider, background;
            private readonly MouseInputElement mouseInput;
            private readonly HudChain layout;

            public DropdownDisplay(HudParentBase parent = null) : base(parent)
            {
                background = new TexturedBox(this)
                {
                    DimAlignment = DimAlignments.Both,
                };

                var border = new BorderBox(this)
                {
                    Color = TerminalFormatting.BorderColor,
                    Thickness = 1f,
                    DimAlignment = DimAlignments.Both,
                };

                name = new Label()
                {
                    AutoResize = false,   
                };

                divider = new TexturedBox()
                {
                    Padding = new Vector2(4f, 17f),
                    Width = 2f,
                    Color = new Color(104, 113, 120),
                };

                arrow = new TexturedBox()
                {
                    Width = 38f,
                    Color = new Color(227, 230, 233),
                    MatAlignment = MaterialAlignment.FitVertical,
                    Material = arrowMat,
                };

                layout = new HudChain(false, this)
                {
                    SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.FitChainBoth,
                    DimAlignment = DimAlignments.Height | DimAlignments.IgnorePadding,
                    ChainContainer = { name, divider, arrow }
                };

                mouseInput = new MouseInputElement(this) 
                { 
                    DimAlignment = DimAlignments.Both
                };

                Color = new Color(41, 54, 62);
                Format = GlyphFormat.White;
            }

            protected override void Layout()
            {
                name.Width = (Width - Padding.X) - divider.Width - arrow.Width;
            }
        }
    }
}