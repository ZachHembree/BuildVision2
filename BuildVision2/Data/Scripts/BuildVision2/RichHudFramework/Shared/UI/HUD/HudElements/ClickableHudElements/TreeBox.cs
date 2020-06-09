using System;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI
{
    using Rendering;

    /// <summary>
    /// Indented, collapsable list. Designed to fit in with SE UI elements.
    /// </summary>
    public class TreeBox<T> : HudElementBase, IClickableElement, IListBoxEntry
    {
        /// <summary>
        /// Invoked when a list member is selected.
        /// </summary>
        public event Action OnSelectionChanged;

        /// <summary>
        /// List of entries in the treebox.
        /// </summary>
        public HudList<ListBoxEntry<T>> List => chain;

        /// <summary>
        /// Height of the treebox in pixels.
        /// </summary>
        public override float Height
        {
            get
            {
                if (!chain.Visible)
                    return display.Height + Padding.Y;
                else
                    return display.Height + chain.Height + Padding.Y;
            }
            set
            {
                if (Padding.Y < value)
                    value = (value - Padding.Y) / _scale;
                else
                    value = (value / _scale);

                if (!chain.Visible)
                {
                    display.Height = value;                   
                }
            }
        }

        /// <summary>
        /// Name of the element as rendered on the display
        /// </summary>
        public RichText Name { get { return display.Name; } set { display.Name = value; } }

        /// <summary>
        /// Default format for member text;
        /// </summary>
        public GlyphFormat Format { get { return display.Format; } set { display.Format = value; } }

        /// <summary>
        /// Determines the color of the header's background/
        /// </summary>
        public Color HeaderColor { get { return display.Color; } set { display.Color = value; } }

        /// <summary>
        /// Color of the list's highlight box.
        /// </summary>
        public Color HighlightColor { get { return highlight.Color; } set { highlight.Color = value; selectionBox.Color = value; } }

        /// <summary>
        /// Current selection. Null if empty.
        /// </summary>
        public ListBoxEntry<T> Selection { get; private set; }

        /// <summary>
        /// Size of the collection.
        /// </summary>
        public int Count => chain.Count;

        /// <summary>
        /// Determines how far to the right list members should be offset from the position of the header.
        /// </summary>
        public float IndentSize { get { return indent * Scale; } set { indent = value / Scale; } }

        /// <summary>
        /// Indicates whether or not the element will appear in the list
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Handles mouse input for the header.
        /// </summary>
        public IMouseInput MouseInput => display.MouseInput;

        private readonly TreeBoxDisplay display;
        private readonly HighlightBox highlight, selectionBox;
        private readonly HudList<ListBoxEntry<T>> chain;
        private float indent;

        public TreeBox(IHudParent parent = null) : base(parent)
        {
            display = new TreeBoxDisplay(this)
            {
                Size = new Vector2(200f, 32f),
                Offset = new Vector2(3f, 0f),
                ParentAlignment = ParentAlignments.Top | ParentAlignments.InnerV,
                DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding
            };

            chain = new HudList<ListBoxEntry<T>>(display)
            {
                Visible = false,
                AutoResize = true,
                AlignVertical = true,
                ParentAlignment = ParentAlignments.Bottom | ParentAlignments.Right | ParentAlignments.InnerH | ParentAlignments.UsePadding,
            };

            selectionBox = new HighlightBox(chain)
            { Color = new Color(34, 44, 53) };

            highlight = new HighlightBox(chain)
            { Color = new Color(34, 44, 53) };

            Size = new Vector2(200f, 32f);
            IndentSize = 40f;

            Format = GlyphFormat.Blueish;
            display.Name = "NewTreeBox";

            display.MouseInput.OnLeftClick += ToggleList;
        }

        private void ToggleList()
        {
            if (!chain.Visible)
                OpenList();
            else
                CloseList();
        }

        private void OpenList()
        {
            GetFocus();
            chain.Visible = true;
            display.Open = true;
        }

        private void CloseList()
        {
            chain.Visible = false;
            display.Open = false;
        }

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelection(T assocMember)
        {
            ListBoxEntry<T> result = chain.Find(x => assocMember.Equals(x.AssocMember));

            if (result != null)
            {
                Selection = result;
                OnSelectionChanged?.Invoke();
            }
        }

        /// <summary>
        /// Sets the selection to the specified entry.
        /// </summary>
        public void SetSelection(ListBoxEntry<T> member)
        {
            ListBoxEntry<T> result = chain.Find(x => member.Equals(x));

            if (result != null)
            {
                Selection = result;
                OnSelectionChanged?.Invoke();
            }
        }

        /// <summary>
        /// Clears the current selection.
        /// </summary>
        public void ClearSelection()
        {
            Selection = null;
        }

        /// <summary>
        /// Adds a new member to the list box with the given name and associated
        /// object.
        /// </summary>
        public ListBoxEntry<T> Add(string name, T assocMember) =>
            Add(new RichText(name, Format), assocMember);

        /// <summary>
        /// Adds a new member to the list box with the given name and associated
        /// object.
        /// </summary>
        public ListBoxEntry<T> Add(RichString name, T assocMember) =>
            Add(new RichText(name), assocMember);

        /// <summary>
        /// Adds a new member to the list box with the given name and associated
        /// object.
        /// </summary>
        public ListBoxEntry<T> Add(RichText name, T assocMember)
        {
            ListBoxEntry<T> member = chain.AddReserved();

            if (member == null)
            {
                member = new ListBoxEntry<T>(assocMember)
                {
                    Format = Format,
                    Padding = new Vector2(24f, 0f),
                };

                chain.Add(member);
            }

            member.OnMemberSelected += SetSelection;
            member.TextBoard.SetText(name);
            member.Enabled = true;

            return member;
        }

        /// <summary>
        /// Removes the given member from the tree box.
        /// </summary>
        public void Remove(ListBoxEntry<T> member)
        {
            chain.RemoveChild(member);
        }

        /// <summary>
        /// Unparents all HUD elements from list
        /// </summary>
        public void Clear()
        {
            OnSelectionChanged = null;
            Selection = null;
            chain.Clear();
        }

        /// <summary>
        /// Resets the HUD element for later reuse.
        /// </summary>
        public void Reset()
        {
            OnSelectionChanged = null;
            Selection = null;
            chain.Reset();
        }

        protected override void Layout()
        {
            chain.Width = Width - IndentSize;

            for (int n = 0; n < chain.Count; n++)
                chain[n].Height = display.Height;

            if (Selection != null)
            {
                selectionBox.Offset = Selection.Offset;
                selectionBox.Size = Selection.Size;
                selectionBox.Visible = Selection.Visible;
            }
            else
                selectionBox.Visible = false;
        }

        protected override void HandleInput()
        {
            highlight.Visible = false;

            for (int n = 0; n < chain.Count; n++)
            {
                if (chain[n].IsMousedOver)
                {
                    highlight.Visible = true;
                    highlight.Size = chain[n].Size;
                    highlight.Offset = chain[n].Offset;
                }
            }
        }

        protected class HighlightBox : TexturedBox
        {
            public override float Height
            {
                set
                {
                    base.Height = value;
                    tab.Height = value;
                }
            }
            public Color TabColor { get { return tab.Color; } set { tab.Color = value; } }

            private readonly TexturedBox tab;

            public HighlightBox(IHudParent parent = null) : base(parent)
            {
                tab = new TexturedBox(this)
                {
                    Width = 4f,
                    Color = new Color(223, 230, 236),
                    ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH
                };
            }
        }

        private class TreeBoxDisplay : HudElementBase
        {
            public override float Width
            {
                get { return layout.Width; }
                set 
                {
                    if (value > Padding.X)
                        value -= Padding.X;

                    name.Width = value - arrow.Width - divider.Width; 
                }
            }

            public override Vector2 Padding
            {
                get { return layout.Padding; }
                set { layout.Padding = value; }
            }

            public RichText Name { get { return name.Text; } set { name.Text = value; } }

            public GlyphFormat Format { get { return name.Format; } set { name.Format = value; } }

            public Color Color { get { return background.Color; } set { background.Color = value; } }

            public IMouseInput MouseInput => mouseInput;

            public bool Open
            {
                get { return open; }
                set
                {
                    open = value;

                    if (open)
                        arrow.Material = downArrow;
                    else
                        arrow.Material = rightArrow;
                }
            }

            private bool open;

            private readonly Label name;
            private readonly TexturedBox arrow, divider, background;
            private readonly HudChain<HudElementBase> layout;
            private readonly MouseInputElement mouseInput;

            private static readonly Material 
                downArrow = new Material("RichHudDownArrow", new Vector2(64f, 64f)), 
                rightArrow = new Material("RichHudRightArrow", new Vector2(64f, 64f));

            public TreeBoxDisplay(IHudParent parent = null) : base(parent)
            {
                name = new Label()
                {
                    AutoResize = false,
                    Format = GlyphFormat.Blueish.WithSize(1.1f),
                };

                divider = new TexturedBox()
                {
                    DimAlignment = DimAlignments.Height,
                    Padding = new Vector2(4f, 6f),
                    Size = new Vector2(2f, 39f),
                    Color = new Color(104, 113, 120),
                };

                arrow = new TexturedBox()
                {
                    Width = 20f,
                    DimAlignment = DimAlignments.Height,
                    MatAlignment = MaterialAlignment.FitHorizontal,
                    Color = new Color(227, 230, 233),
                    Material = rightArrow,
                };

                background = new TexturedBox(this)
                {
                    Color = new Color(41, 54, 62),
                    DimAlignment = DimAlignments.Both,
                };

                layout = new HudChain<HudElementBase>(this)
                {
                    AlignVertical = false,
                    AutoResize = true,
                    DimAlignment = DimAlignments.Height,
                    ChildContainer = { arrow, divider, name }
                };

                mouseInput = new MouseInputElement(this)
                {
                    DimAlignment = DimAlignments.Both
                };
            }
        }
    }
}