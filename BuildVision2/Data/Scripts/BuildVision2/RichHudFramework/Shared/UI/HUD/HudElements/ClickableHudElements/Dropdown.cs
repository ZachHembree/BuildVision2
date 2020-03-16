using System;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI
{
    using Rendering;

    /// <summary>
    /// Collapsable list box. Designed to mimic the appearance of the dropdown in the SE terminal.
    /// </summary>
    public class Dropdown<T> : HudElementBase, IClickableElement, IListBoxEntry
    {
        /// <summary>
        /// Invoked when a member of the list is selected.
        /// </summary>
        public event Action OnSelectionChanged { add { listBox.OnSelectionChanged += value; } remove { listBox.OnSelectionChanged -= value; } }

        /// <summary>
        /// List of entries in the dropdown.
        /// </summary>
        public ReadOnlyCollection<ListBoxEntry<T>> List => listBox.List;

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
        /// Current selection. Null if empty.
        /// </summary>
        public ListBoxEntry<T> Selection => listBox.Selection;

        /// <summary>
        /// Indicates whether or not the element will appear in the list
        /// </summary>
        public bool Enabled { get { return listBox.Enabled; } set { listBox.Enabled = value; } }

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

        protected readonly DropdownDisplay display;
        public readonly ListBox<T> listBox;

        public Dropdown(IHudParent parent = null) : base(parent)
        {
            display = new DropdownDisplay(this)
            {
                Padding = new Vector2(10f, 0f),
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
            };

            listBox = new ListBox<T>(display)
            {
                Visible = false,
                ZOffset = HudLayers.Foreground,
                MinimumVisCount = 4,
                DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding,
                ParentAlignment = ParentAlignments.Bottom,
                MemberPadding = new Vector2(8f, 0f),
                Offset = new Vector2(0f, -1f),
                TabColor = new Color(0, 0, 0, 0),
            };

            Size = new Vector2(331f, 43f);
            display.Text = "Empty";

            display.MouseInput.OnLeftClick += ToggleList;
            OnSelectionChanged += UpdateDisplay;
        }

        protected override void HandleInput()
        {
            if (SharedBinds.LeftButton.IsNewPressed && !IsMousedOver)
            {
                CloseList();
            }
        }

        private void UpdateDisplay()
        {
            if (Selection != null)
            {
                display.Text = Selection.TextBoard.GetText();
                CloseList();
            }
        }

        private void ToggleList()
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
        /// Adds a new member to the list box with the given name and associated
        /// object.
        /// </summary>
        public ListBoxEntry<T> Add(RichText name, T assocMember) =>
            listBox.Add(name, assocMember);

        /// <summary>
        /// Removes the given member from the list box.
        /// </summary>
        public void Remove(ListBoxEntry<T> member) =>
            listBox.Remove(member);

        /// <summary>
        /// Clears the current contents of the list.
        /// </summary>
        public void Clear() =>
            listBox.Clear();

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

        public new object GetOrSetMember(object data, int memberEnum) =>
            listBox.GetOrSetMember(data, memberEnum);

        protected class DropdownDisplay : HudElementBase
        {
            private static readonly Material arrowMat = new Material("RichHudDownArrow", new Vector2(64f, 64f));

            public RichText Text { get { return name.Text; } set { name.Text = value; } }
            public GlyphFormat Format { get { return name.Format; } set { name.Format = value; } }
            public Color Color { get { return background.Color; } set { background.Color = value; } }
            public override bool IsMousedOver => mouseInput.IsMousedOver;
            public IMouseInput MouseInput => mouseInput;

            public readonly Label name;
            public readonly TexturedBox arrow, divider, background;
            private readonly MouseInputElement mouseInput;
            private readonly HudChain<HudElementBase> layout;

            public DropdownDisplay(IHudParent parent = null) : base(parent)
            {
                name = new Label()
                {
                    AutoResize = false,   
                };

                arrow = new TexturedBox()
                {
                    Width = 38f,
                    Color = new Color(227, 230, 233),
                    MatAlignment = MaterialAlignment.FitVertical,
                    Material = arrowMat,
                };

                divider = new TexturedBox()
                {
                    Padding = new Vector2(0f, 17f),
                    Size = new Vector2(2f, 39f),
                    Color = new Color(104, 113, 120),
                    DimAlignment = DimAlignments.Height | DimAlignments.IgnorePadding,
                };

                background = new TexturedBox(this)
                {
                    DimAlignment = DimAlignments.Both,
                };

                layout = new HudChain<HudElementBase>(this)
                {
                    AlignVertical = false,
                    AutoResize = true,
                    DimAlignment = DimAlignments.Height | DimAlignments.IgnorePadding,
                    ChildContainer = { name, divider, arrow }
                };

                mouseInput = new MouseInputElement(this) 
                { 
                    DimAlignment = DimAlignments.Both
                };

                Color = new Color(41, 54, 62);
                Format = GlyphFormat.White;
                Text = "NewDropdown";
            }

            protected override void Layout()
            {
                name.Width = (Width - Padding.X) - divider.Width - arrow.Width;
            }
        }
    }
}