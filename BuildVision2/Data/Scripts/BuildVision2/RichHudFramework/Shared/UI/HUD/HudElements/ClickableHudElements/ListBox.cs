using System;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework.UI
{
    using CollectionData = MyTuple<Func<int, ApiMemberAccessor>, Func<int>>;
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    internal enum ListBoxAccessors : int
    {
        /// <summary>
        /// CollectionData
        /// </summary>
        ListMembers = 1,

        /// <summary>
        /// MyTuple<RichStringMembers[], T>
        /// </summary>
        Add = 2,

        /// <summary>
        /// ApiMemberAccessor
        /// </summary>
        Selection = 3,
    }

    public class ListBox<T> : HudElementBase, IListBoxEntry
    {
        public event Action OnSelectionChanged;
        public ReadOnlyCollection<ListBoxEntry<T>> Members => scrollBox.List;

        public override float Width
        {
            get { return scrollBox.Width; }
            set { scrollBox.Width = value; }
        }

        public override float Height
        {
            get { return scrollBox.Height; }
            set { scrollBox.Height = value; }
        }

        /// <summary>
        /// Background color
        /// </summary>
        public Color Color { get { return scrollBox.Color; } set { scrollBox.Color = value; } }

        public Color HighlightColor
        {
            get { return selectionBox.TabColor; }
            set
            {
                selectionBox.Color = value;
                highlight.Color = value;
            }
        }

        public Color TabColor
        {
            get { return selectionBox.TabColor; }
            set
            {
                selectionBox.TabColor = value;
                highlight.TabColor = value;
            }
        }

        /// <summary>
        /// Color of the border box
        /// </summary>
        public Color BorderColor { get { return border.Color; } set { border.Color = value; } }

        /// <summary>
        /// Default format for member text;
        /// </summary>
        public GlyphFormat Format { get; set; }

        /// <summary>
        /// Total number of elements in the list
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Minimum number of elements visible in the list at any given time.
        /// </summary>
        public int MinimumVisCount { get { return scrollBox.MinimumVisCount; } set { scrollBox.MinimumVisCount = value; } }

        /// <summary>
        /// Current selection. Null if empty.
        /// </summary>
        public ListBoxEntry<T> Selection { get; private set; }

        /// <summary>
        /// Indicates whether or not the element will appear in the list
        /// </summary>
        public bool Enabled { get; set; }

        private readonly ScrollBox<ListBoxEntry<T>> scrollBox;
        private readonly HighlightBox selectionBox, highlight;
        private readonly BorderBox border;

        public ListBox(IHudParent parent = null) : base(parent)
        {
            scrollBox = new ScrollBox<ListBoxEntry<T>>(this)
            {
                FitToChain = false,
                AlignVertical = true,
                Size = new Vector2(355f, 300f),
                //ParentAlignment = ParentAlignments.Top | ParentAlignments.InnerV
            };

            border = new BorderBox(scrollBox)
            {
                DimAlignment = DimAlignments.Both,
                Color = new Color(58, 68, 77),
                Thickness = 2f,
            };

            selectionBox = new HighlightBox(scrollBox.Members)
            { Color = new Color(34, 44, 53) };

            highlight = new HighlightBox(scrollBox.Members)
            { Color = new Color(34, 44, 53) };

            Size = new Vector2(355f, 223f);
            Enabled = true;
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

            if (Count >= scrollBox.List.Count)
            {
                member = new ListBoxEntry<T>(assocMember)
                {
                    Size = new Vector2(310f, 30f),
                    Padding = new Vector2(10f, 0f)
                };

                member.OnMemberSelected += SetSelection;
                scrollBox.AddToList(member);
            }

            member = scrollBox.List[Count];
            member.TextBoard.SetText(name);
            member.Visible = true;
            Count++;

            return member;
        }

        /// <summary>
        /// Removes the given member from the list box.
        /// </summary>
        public void Remove(ListBoxEntry<T> member)
        {
            scrollBox.RemoveFromList(member);
        }

        /// <summary>
        /// Clears the current contents of the list.
        /// </summary>
        public void Clear()
        {
            for (int n = 0; n < scrollBox.List.Count; n++)
                scrollBox.List[n].Visible = false;

            Count = 0;
        }

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelection(T assocMember)
        {
            ListBoxEntry<T> result = scrollBox.Find(x => assocMember.Equals(x.AssocMember));

            if (result != null)
            {
                Selection = result;
                OnSelectionChanged?.Invoke();
            }
        }

        public void SetSelection(ListBoxEntry<T> member)
        {
            ListBoxEntry<T> result = scrollBox.Find(x => member.Equals(x));

            if (result != null)
            {
                Selection = result;
                OnSelectionChanged?.Invoke();
            }
        }

        protected override void Draw()
        {
            if (Selection != null)
            {
                selectionBox.Offset = Selection.Offset;
                selectionBox.Padding = Selection.Padding;
                selectionBox.Size = Selection.Size;
                selectionBox.Visible = Selection.Visible;
            }
            else
                selectionBox.Visible = false;
        }

        protected override void HandleInput()
        {
            highlight.Visible = false;

            foreach (ListBoxEntry<T> button in scrollBox.List)
            {
                if (button.IsMousedOver)
                {
                    highlight.Visible = true;
                    highlight.Size = button.Size;
                    highlight.Offset = button.Offset;
                }
            }
        }

        public new object GetOrSetMember(object data, int memberEnum)
        {
            var member = (ListBoxAccessors)memberEnum;

            switch (member)
            {
                case ListBoxAccessors.ListMembers:
                    return new CollectionData(x => Members[x].GetOrSetMember, () => Members.Count);
                case ListBoxAccessors.Add:
                    {
                        var entryData = (MyTuple<RichStringMembers[], T>)data;

                        return (ApiMemberAccessor)Add(new RichText(entryData.Item1), entryData.Item2).GetOrSetMember;
                    }
                case ListBoxAccessors.Selection:
                    {
                        if (data == null)
                            return (ApiMemberAccessor)Selection.GetOrSetMember;
                        else
                            SetSelection(data as ListBoxEntry<T>);

                        break;
                    }
            }

            return null;
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
    }

    public interface IListBoxEntry : IHudElement
    {
        /// <summary>
        /// Indicates whether or not the element will appear in the list
        /// </summary>
        bool Enabled { get; }
    }

    internal enum ListBoxEntryAccessors : int
    {
        /// <summary>
        /// RichStringMembers[]
        /// </summary>
        Name = 1,

        /// <summary>
        /// Bool
        /// </summary>
        Enabled = 2,

        /// <summary>
        /// Object
        /// </summary>
        AssocObject = 3,

        /// <summary>
        /// Object
        /// </summary>
        ID = 4,
    }

    /// <summary>
    /// Text button assocated with a given object. Used in conjunction with list boxes. Implements IListBoxMember.
    /// </summary>
    public class ListBoxEntry<T> : LabelButton, IListBoxEntry
    {
        public event Action<ListBoxEntry<T>> OnMemberSelected;
        public bool Enabled { get; set; }
        public T AssocMember { get; set; }

        public ListBoxEntry(T assocMember, IHudParent parent = null) : base(parent)
        {
            this.AssocMember = assocMember;
            AutoResize = false;
            Enabled = true;

            MouseInput.OnLeftClick += SelectMember;
        }

        private void SelectMember()
        {
            OnMemberSelected?.Invoke(this);
        }

        public new object GetOrSetMember(object data, int memberEnum)
        {
            var member = (ListBoxEntryAccessors)memberEnum;

            switch (member)
            {
                case ListBoxEntryAccessors.Name:
                    {
                        if (data == null)
                            TextBoard.SetText(new RichText((RichStringMembers[])data));
                        else
                            return TextBoard.GetText().GetApiData();

                        break;
                    }
                case ListBoxEntryAccessors.Enabled:
                    {
                        if (data == null)
                            Enabled = (bool)data;
                        else
                            return Enabled;

                        break;
                    }
                case ListBoxEntryAccessors.AssocObject:
                    {
                        if (data == null)
                            AssocMember = (T)data;
                        else
                            return AssocMember;

                        break;
                    }
                case ListBoxEntryAccessors.ID:
                        return this;
            }

            return null;
        }
    }
}