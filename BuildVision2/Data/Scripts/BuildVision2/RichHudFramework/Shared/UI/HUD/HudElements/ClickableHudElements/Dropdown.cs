using System;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI
{
    using Rendering;

    public class Dropdown<T> : HudElementBase, IListBoxEntry
    {
        public event Action OnSelectionChanged { add { list.OnSelectionChanged += value; } remove { list.OnSelectionChanged -= value; } }
        public ReadOnlyCollection<ListBoxEntry<T>> Members => list.Members;

        public override float Width
        {
            get { return display.Width; }
            set
            {
                display.Width = value;
                list.Width = value;

                foreach (ListBoxEntry<T> member in list.Members)
                    member.Width = value;
            }
        }

        public override float Height
        {
            get { return display.Height; }
            set
            {
                display.Height = value;

                foreach (ListBoxEntry<T> member in list.Members)
                    member.Height = value;
            }
        }

        /// <summary>
        /// Default format for member text;
        /// </summary>
        public GlyphFormat Format { get { return list.Format; } set { list.Format = value; } }

        /// <summary>
        /// Current selection. Null if empty.
        /// </summary>
        public ListBoxEntry<T> Selection => list.Selection;

        /// <summary>
        /// Indicates whether or not the element will appear in the list
        /// </summary>
        public bool Enabled { get { return list.Enabled; } set { list.Enabled = value; } }

        public IClickableElement MouseInput => display.MouseInput;

        private readonly DropdownDisplay display;
        private readonly ListBox<T> list;

        public Dropdown(IHudParent parent = null) : base(parent)
        {
            display = new DropdownDisplay(this)
            {
                Padding = new Vector2(16f, 0f),
            };

            list = new ListBox<T>(display)
            {
                TabColor = new Color(0, 0, 0, 0),
                MinimumVisCount = 4,
                Offset = new Vector2(0f, -2f),
                ParentAlignment = ParentAlignments.Bottom,
                Visible = false,
            };

            Size = new Vector2(331f, 43f);
            display.TextBoard.SetText("Empty");
            display.MouseInput.OnLeftClick += ToggleList;
            OnSelectionChanged += UpdateDisplay;
        }

        private void UpdateDisplay()
        {
            if (Selection != null)
                display.TextBoard.SetText(Selection.TextBoard.GetText());
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

        /// <summary>
        /// Adds a new member to the list box with the given name and associated
        /// object.
        /// </summary>
        public ListBoxEntry<T> Add(string name, T assocMember) =>
            list.Add(name, assocMember);

        /// <summary>
        /// Adds a new member to the list box with the given name and associated
        /// object.
        /// </summary>
        public ListBoxEntry<T> Add(RichString name, T assocMember) =>
            list.Add(name, assocMember);

        /// <summary>
        /// Adds a new member to the list box with the given name and associated
        /// object.
        /// </summary>
        public ListBoxEntry<T> Add(RichText name, T assocMember) =>
            list.Add(name, assocMember);

        /// <summary>
        /// Removes the given member from the list box.
        /// </summary>
        public void Remove(ListBoxEntry<T> member) =>
            list.Remove(member);

        /// <summary>
        /// Clears the current contents of the list.
        /// </summary>
        public void Clear() =>
            list.Clear();

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelection(T assocMember) =>
            list.SetSelection(assocMember);

        public void SetSelection(ListBoxEntry<T> member) =>
            list.SetSelection(member);

        public new object GetOrSetMember(object data, int memberEnum) =>
            list.GetOrSetMember(data, memberEnum);

        private class DropdownDisplay : TextBoxButton
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

            public override Vector2 Padding
            {
                set
                {
                    base.Padding = value;
                    textElement.Offset = new Vector2(value.X, 0f);
                }
            }

            private readonly TexturedBox arrow, verticalBar;
            private readonly BorderBox border;

            public DropdownDisplay(IHudParent parent = null) : base(parent)
            {
                AutoResize = false;
                Format = GlyphFormat.White;
                Color = new Color(41, 54, 62);
                textElement.ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH;

                arrow = new TexturedBox(textElement)
                {
                    Width = 39f,
                    Color = new Color(227, 230, 233),
                    ParentAlignment = ParentAlignments.Right,
                    MatAlignment = MaterialAlignment.FitHorizontal,
                    Material = new Material("RichHudDownArrow", new Vector2(64f, 64f)),
                };

                verticalBar = new TexturedBox(arrow)
                {
                    Padding = new Vector2(0f, 17f),
                    Size = new Vector2(2f, 39f),
                    Color = new Color(104, 113, 120),
                    ParentAlignment = ParentAlignments.Left
                };

                border = new BorderBox(this)
                {
                    Color = new Color(94, 103, 110),
                    Thickness = 2f,
                    DimAlignment = DimAlignments.Both,
                };
            }
        }
    }
}