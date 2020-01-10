using System;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI
{
    using Rendering;

    public class TreeBox<T> : HudElementBase, IListBoxEntry
    {
        public event Action OnSelectionChanged;
        public ReadOnlyCollection<ListBoxEntry<T>> Members => chain.List;

        public override float Width
        {
            get { return display.Width; }
            set
            {
                display.Width = value;
                chain.Width = value - IndentSize;
            }
        }

        public override float Height
        {
            get
            {
                if (!chain.Visible)
                    return display.Height;
                else
                    return display.Height + chain.Height;
            }
            set
            {
                if (!chain.Visible)
                {
                    display.Height = value;                   
                }
            }
        }

        /// <summary>
        /// Name of the element as rendered on the display
        /// </summary>
        public RichText Name { get { return display.Text; } set { display.Text = value; } }

        /// <summary>
        /// Default format for member text;
        /// </summary>
        public GlyphFormat Format { get { return display.Format; } set { display.Format = value; } }

        /// <summary>
        /// Current selection. Null if empty.
        /// </summary>
        public ListBoxEntry<T> Selection { get; private set; }

        public int Count { get; private set; }

        public float IndentSize { get; set; }

        /// <summary>
        /// Indicates whether or not the element will appear in the list
        /// </summary>
        public bool Enabled { get; set; }

        public IClickableElement MouseInput => display.MouseInput;

        private readonly TreeBoxDisplay display;
        private readonly HighlightBox highlight, selectionBox;
        private readonly HudChain<ListBoxEntry<T>> chain;

        public TreeBox(IHudParent parent = null) : base(parent)
        {
            display = new TreeBoxDisplay(this)
            {
                Padding = new Vector2(10f, 0f),
                ParentAlignment = ParentAlignments.Top | ParentAlignments.InnerV,
            };

            chain = new HudChain<ListBoxEntry<T>>(display)
            {
                Visible = false,
                AutoResize = true,
                AlignVertical = true,
                ParentAlignment = ParentAlignments.Bottom | ParentAlignments.Right | ParentAlignments.InnerH,
            };

            selectionBox = new HighlightBox(chain)
            { Color = new Color(34, 44, 53) };

            highlight = new HighlightBox(chain)
            { Color = new Color(34, 44, 53) };

            Size = new Vector2(200f, 32f);
            IndentSize = 40f;

            Format = GlyphFormat.Blueish;
            display.Text = "NewTreeBox";

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
            ListBoxEntry<T> member;

            if (Count >= Members.Count)
            {
                member = new ListBoxEntry<T>(assocMember)
                {
                    Format = Format,
                    Padding = new Vector2(24f, 0f),
                };

                member.OnMemberSelected += SetSelection;
                chain.Add(member);
            }

            member = Members[Count];
            member.TextBoard.SetText(name);
            member.Visible = true;
            Count++;

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
        /// Clears the current contents of the list.
        /// </summary>
        public void Clear()
        {
            for (int n = 0; n < Members.Count; n++)
                Members[n].Visible = false;

            Count = 0;
        }

        protected override void Draw()
        {
            foreach (ListBoxEntry<T> member in chain.List)
                member.Height = display.Height;

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

            foreach (ListBoxEntry<T> button in Members)
            {
                if (button.IsMousedOver)
                {
                    highlight.Visible = true;
                    highlight.Size = button.Size;
                    highlight.Offset = button.Offset;
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

        private class TreeBoxDisplay : TextBoxButton
        {
            public override float Width
            {
                get { return base.Width + arrow.Width; }
                set { base.Width = value - arrow.Width; }
            }

            public override float Height
            {
                set
                {
                    base.Height = value;
                    arrow.Height = value;
                    verticalBar.Height = value;
                }
            }

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
            private readonly TexturedBox arrow, verticalBar;
            private static readonly Material 
                downArrow = new Material("RichHudDownArrow", new Vector2(64f, 64f)), 
                rightArrow = new Material("RichHudRightArrow", new Vector2(64f, 64f));

            public TreeBoxDisplay(IHudParent parent = null) : base(parent)
            {
                AutoResize = false;
                Format = GlyphFormat.Blueish.WithSize(1.1f);
                Color = new Color(41, 54, 62);

                textElement.ParentAlignment = ParentAlignments.Right | ParentAlignments.InnerH;

                arrow = new TexturedBox(this)
                {
                    Width = 20f,
                    Offset = new Vector2(1f, 0f),
                    ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH,
                    MatAlignment = MaterialAlignment.FitHorizontal,
                    Color = new Color(227, 230, 233),
                    Material = rightArrow,
                };

                verticalBar = new TexturedBox(arrow)
                {
                    Padding = new Vector2(0f, 6f),
                    Size = new Vector2(2f, 39f),
                    Color = new Color(104, 113, 120),
                    ParentAlignment = ParentAlignments.Right
                };
            }
        }
    }
}