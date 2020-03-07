using System;
using System.Text;
using VRage;
using VRageMath;
using System.Collections.Generic;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
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
        /// MyTuple<IList<RichStringMembers>, T>
        /// </summary>
        Add = 2,

        /// <summary>
        /// ApiMemberAccessor
        /// </summary>
        Selection = 3,
    }

    /// <summary>
    /// Scrollable list of text elements. Each list entry is associated with a value of type T.
    /// </summary>
    public class ListBox<T> : HudElementBase, IListBoxEntry
    {
        /// <summary>
        /// Invoked when an entry is selected.
        /// </summary>
        public event Action OnSelectionChanged;

        /// <summary>
        /// Read-only collection of list entries.
        /// </summary>
        public ReadOnlyCollection<ListBoxEntry<T>> List => scrollBox.List;

        /// <summary>
        /// Width of the list box in pixels.
        /// </summary>
        public override float Width { get { return scrollBox.Width; } set { scrollBox.Width = value; } }

        /// <summary>
        /// Height of the list box in pixels.
        /// </summary>
        public override float Height { get { return scrollBox.Height; } set { scrollBox.Height = value; } }

        /// <summary>
        /// Border size. Included in total element size.
        /// </summary>
        public override Vector2 Padding { get { return scrollBox.Padding; } set { scrollBox.Padding = value; } }

        /// <summary>
        /// Background color
        /// </summary>
        public Color Color { get { return scrollBox.Color; } set { scrollBox.Color = value; } }

        /// <summary>
        /// Background color of the highlight box
        /// </summary>
        public Color HighlightColor
        {
            get { return selectionBox.Color; }
            set
            {
                selectionBox.Color = value;
                highlight.Color = value;
            }
        }

        /// <summary>
        /// Color of the highlight box's tab
        /// </summary>
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
        /// Padding applied to list members.
        /// </summary>
        public Vector2 MemberPadding
        {
            get { return _memberPadding; }
            set
            {
                _memberPadding = value;

                for (int n = 0; n < scrollBox.List.Count; n++)
                    scrollBox.List[n].Padding = value;
            }
        }

        /// <summary>
        /// Height of entries in the list.
        /// </summary>
        public float LineHeight 
        { 
            get { return _lineHeight; } 
            set 
            {
                _lineHeight = value;
                
                for (int n = 0; n < scrollBox.List.Count; n++)
                    scrollBox.List[n].Height = value;
            } 
        }

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

        public readonly ScrollBox<ListBoxEntry<T>> scrollBox;
        protected readonly HighlightBox selectionBox, highlight;
        protected readonly BorderBox border;
        private Vector2 _memberPadding;
        private float _lineHeight;

        public ListBox(IHudParent parent = null) : base(parent)
        {
            scrollBox = new ScrollBox<ListBoxEntry<T>>(this)
            {
                SizingMode = ScrollBoxSizingModes.FitMembersToBox,
                AlignVertical = true,
            };

            border = new BorderBox(scrollBox)
            {
                DimAlignment = DimAlignments.Both,
                Color = new Color(58, 68, 77),
                Thickness = 1f,
            };

            selectionBox = new HighlightBox(scrollBox.Chain)
            { Color = new Color(34, 44, 53) };

            highlight = new HighlightBox(scrollBox.Chain)
            { Color = new Color(34, 44, 53) };

            Size = new Vector2(355f, 223f);
            _lineHeight = 30f;

            Enabled = true;
            CaptureCursor = true;
        }

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
                    Format = Format,
                    Height = _lineHeight,
                    Padding = _memberPadding,
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

        /// <summary>
        /// Sets the selection to the specified entry.
        /// </summary>
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

            foreach (ListBoxEntry<T> entry in scrollBox.List)
            {
                if (entry.IsMousedOver)
                {
                    highlight.Visible = true;
                    highlight.Size = entry.Size;
                    highlight.Offset = entry.Offset;
                    highlight.Padding = entry.Padding;
                }
            }
        }

        public new object GetOrSetMember(object data, int memberEnum)
        {
            var member = (ListBoxAccessors)memberEnum;

            switch (member)
            {
                case ListBoxAccessors.ListMembers:
                    return new CollectionData(x => List[x].GetOrSetMember, () => List.Count);
                case ListBoxAccessors.Add:
                    {
                        var entryData = (MyTuple<IList<RichStringMembers>, T>)data;

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
}