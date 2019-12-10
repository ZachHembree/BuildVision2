using System;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.UI
{
    using Rendering;

    public class Dropdown : HudElementBase, IEnumerable<RichText>, IScrollBoxMember
    {
        public event Action<int> OnSelectionChanged;
        public new Dropdown ChildContainer => this;
        public override float Width
        {
            get { return display.Width; }
            set
            {
                display.Width = value;
                list.Width = value;

                foreach (DropdownMember member in list.ChainElements)
                    member.Size = display.Size;
            }
        }
        public override float Height
        {
            get { return display.Height; }
            set
            {
                display.Height = value;

                foreach (DropdownMember member in list.ChainElements)
                    member.Size = display.Size;
            }
        }
        public int SelectionID => selection != null ? selection.ID : -1;
        public bool Enabled => true;

        private readonly Label name;
        private DropdownDisplay display;
        private readonly TexturedBox highlight;
        private readonly ScrollBox<DropdownMember> list;
        private DropdownMember selection;

        public Dropdown(IHudParent parent = null) : base(parent)
        {
            CaptureCursor = true;

            name = new Label("NewDropdown", GlyphFormat.White, this)
            { ParentAlignment = ParentAlignment.Top | ParentAlignment.Left | ParentAlignment.InnerH };

            display = new DropdownDisplay(this)
            {
                Padding = new Vector2(15f, 0f),
            };

            list = new ScrollBox<DropdownMember>(this)
            {
                AutoResize = true,
                ParentAlignment = ParentAlignment.Bottom,
                Visible = false,
                ChildContainer =
                {
                    new BorderBox()
                    {
                        Color = new Color(58, 68, 77),
                        Thickness = 2f,
                        MatchParentSize = true,
                        Offset = new Vector2(0f, -1f)
                    }
                }
            };

            highlight = new TexturedBox(list)
            {
                Padding = new Vector2(10f, 8f),
                Color = new Color(34, 44, 53),
                Visible = false
            };

            Size = new Vector2(331f, 43f);
            display.Text.SetText("Empty");
            display.MouseInput.OnLeftClick += ToggleList;
        }

        protected override void HandleInput()
        {
            highlight.Visible = false;

            foreach (DropdownMember button in list.ChainElements)
            {
                if (button.Visible && button.IsMousedOver)
                {
                    highlight.Visible = true;
                    highlight.Size = button.Size;
                    highlight.Offset = button.Offset;
                }
            }
        }

        private void ToggleList()
        {
            if (!list.Visible)
                OpenList();
            else
                CloseList();
        }

        private void OpenList()
        {
            GetFocus();
            list.Visible = true;
        }

        private void CloseList()
        {
            list.Visible = false;
        }

        public void Add(RichString name, int ID) =>
            Add(new RichText(name), ID);

        public void Add(RichText name, int ID)
        {
            DropdownMember newMember = new DropdownMember(list.ChainElements.Count, ID, name)
            {
                Size = Size - display.Padding,
                Padding = display.Padding,
                Format = display.Format
            };

            newMember.OnMemberSelected += SetSelection;
            list.AddToList(newMember);
        }

        public void SetSelection(int index)
        {
            selection = list.ChainElements[index];
            display.Text.SetText(selection.Text.GetText());

            OnSelectionChanged?.Invoke(selection.ID);
            CloseList();
        }

        public void Remove(int ID)
        {
            list.RemoveFromList(x => x.ID == ID);
        }

        IEnumerator<RichText> IEnumerable<RichText>.GetEnumerator() =>
            new CollectionDataEnumerator<RichText>(x => list.ChainElements[x].Text.GetText(), () => list.ChainElements.Count);

        private class DropdownDisplay : TextBoxButton
        {
            public override float Width
            {
                get { return TextSize.X + arrow.Width + Padding.X; }
                set
                {
                    TextSize = new Vector2(value - Padding.X - arrow.Width, TextSize.Y);
                    background.Width = value;
                    mouseInput.Width = value;
                }
            }

            public override float Height
            {
                get { return TextSize.Y + Padding.Y; }
                set
                {
                    TextSize = new Vector2(TextSize.X, value - Padding.Y);
                    background.Height = value;
                    mouseInput.Height = value;
                    arrow.Height = value;
                    verticalBar.Height = value;
                }
            }

            public override Vector2 Padding
            {
                set
                {
                    base.Padding = value;
                    textElement.Offset = new Vector2(Padding.X, 0f);
                }
            }

            private readonly TexturedBox arrow, verticalBar;
            private readonly BorderBox border;

            public DropdownDisplay(IHudParent parent = null) : base(parent)
            {
                AutoResize = false;
                Format = GlyphFormat.White;
                Color = new Color(41, 54, 62);
                textElement.ParentAlignment = ParentAlignment.Left | ParentAlignment.InnerH;

                arrow = new TexturedBox(textElement)
                {
                    Width = 39f,
                    Color = new Color(227, 230, 233),
                    ParentAlignment = ParentAlignment.Right,
                    MatAlignment = MaterialAlignment.FitHorizontal,
                    Material = new Material("HudLibDownArrow", new Vector2(64f, 64f)),
                };

                verticalBar = new TexturedBox(arrow)
                {
                    Padding = new Vector2(0f, 17f),
                    Size = new Vector2(2f, 39f),
                    Color = new Color(104, 113, 120),
                    ParentAlignment = ParentAlignment.Left
                };

                border = new BorderBox(this)
                {
                    Color = new Color(94, 103, 110),
                    Thickness = 2f,
                    MatchParentSize = true
                };
            }
        }

        private class DropdownMember : LabelButton, IScrollBoxMember
        {
            public event Action<int> OnMemberSelected;
            public bool Enabled { get; set; }
            public readonly int index, ID;
            
            public DropdownMember(int index, int ID, RichText name, IHudParent parent = null) : base(name, parent)
            {
                this.index = index;
                this.ID = ID;
                AutoResize = false;
                Enabled = true;

                MouseInput.OnLeftClick += SelectMember;
            }

            private void SelectMember()
            {
                OnMemberSelected?.Invoke(index);
            }
        }
    }
}