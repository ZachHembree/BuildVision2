using System;
using VRageMath;

namespace DarkHelmet.UI
{
    public class ListBox : PaddedElementBase, IScrollBoxMember
    {
        public event Action<int> OnSelectionChanged;
        public override float Width
        {
            get { return list.Width; }
            set
            {
                //name.Width = value;
                list.Width = value;
            }
        }
        public override float Height
        {
            get { return list.Height; }
            set
            {
                list.Height = value;
            }
        }
        public Color Color { get { return list.Color; } set { list.Color = value; } }
        public int Count { get; private set; }
        public int Selection => selection != null ? selection.index : -1;
        public bool Enabled => true;

        //private readonly Label name;
        private readonly ScrollBox<ListBoxMember> list;
        private readonly HighlightBox selectionBox, highlight;

        private ListBoxMember selection;

        public ListBox(IHudParent parent = null) : base(parent)
        {
            //name = new Label("NewListBox", GlyphFormat.White, this)
            //{ AutoResize = false, Height = 24f, ParentAlignment = ParentAlignment.Top | ParentAlignment.InnerV };

            list = new ScrollBox<ListBoxMember>(this)
            { ParentAlignment = ParentAlignment.Bottom };

            selectionBox = new HighlightBox(list)
            {
                Width = 302f,
                //Padding = new Vector2(10f, 0f),
                Color = new Color(34, 44, 53),
            };

            highlight = new HighlightBox(list)
            {
                Width = 302f,
                Color = new Color(34, 44, 53),
            };

            Size = new Vector2(355f, 223f);
        }

        public void Add(RichString name) =>
            Add(new RichText(name));

        public void Add(RichText name)
        {
            if (Count >= list.ChainElements.Count)
            {
                ListBoxMember newMember = new ListBoxMember(list.ChainElements.Count)
                {
                    Size = new Vector2(310f, 30f),
                    Padding = new Vector2(10f, 0f)
                };

                newMember.OnMemberSelected += SetSelection;
                list.AddToList(newMember);
            }

            list.ChainElements[Count].Text.SetText(name);
            list.ChainElements[Count].Visible = true;
            Count++;         
        }

        public void Remove(int index)
        {
            //list.RemoveAt(index);
        }

        public void Clear()
        {
            for (int n = 0; n < list.ChainElements.Count; n++)
                list.ChainElements[n].Visible = false;

            Count = 0;
        }

        public void SetSelection(int index)
        {
            selection = list.ChainElements[index];
            OnSelectionChanged?.Invoke(index);
        }

        protected override void Draw()
        {
            if (selection != null)
            {
                selectionBox.Offset = selection.Offset;
                selectionBox.Size = selection.Size;
                selectionBox.Visible = selection.Visible;
            }
            else
                selectionBox.Visible = false;
        }

        protected override void HandleInput()
        {
            highlight.Visible = false;

            foreach (ListBoxMember button in list.ChainElements)
            {
                if (button.IsMousedOver)
                {
                    highlight.Visible = true;
                    highlight.Size = button.Size;
                    highlight.Offset = button.Offset;
                }
            }
        }

        private class HighlightBox : TexturedBox
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
                    ParentAlignment = ParentAlignment.Left | ParentAlignment.InnerH
                };
            }
        }

        private class ListBoxMember : LabelButton, IScrollBoxMember
        {
            public event Action<int> OnMemberSelected;
            public bool Enabled { get; set; }
            public readonly int index;

            public ListBoxMember(int index, IHudParent parent = null) : base(parent)
            {
                this.index = index;
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