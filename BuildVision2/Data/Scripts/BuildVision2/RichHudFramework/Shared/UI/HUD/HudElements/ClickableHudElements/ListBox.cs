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
    using CollectionData = MyTuple<Func<int, ApiMemberAccessor>, Func<int>>;
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    public enum ListBoxAccessors : int
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
    public class ListBox<T> : HudElementBase, IEntryBox<T>
    {
        /// <summary>
        /// Invoked when an entry is selected.
        /// </summary>
        public event EventHandler OnSelectionChanged;

        /// <summary>
        /// Used to allow the addition of list entries using collection-initializer syntax in
        /// conjunction with normal initializers.
        /// </summary>
        public ListBox<T> ListContainer => this;

        /// <summary>
        /// Read-only collection of list entries.
        /// </summary>
        public IReadOnlyList<ListBoxEntry<T>> ListEntries => scrollBox.ChainEntries;

        /// <summary>
        /// Background color
        /// </summary>
        public Color Color { get { return scrollBox.Color; } set { scrollBox.Color = value; } }

        /// <summary>
        /// Color of the slider bar
        /// </summary>
        public Color BarColor { get { return scrollBox.BarColor; } set { scrollBox.BarColor = value; } }

        /// <summary>
        /// Bar color when moused over
        /// </summary>
        public Color BarHighlight { get { return scrollBox.BarHighlight; } set { scrollBox.BarHighlight = value; } }

        /// <summary>
        /// Color of the slider box when not moused over
        /// </summary>
        public Color SliderColor { get { return scrollBox.SliderColor; } set { scrollBox.SliderColor = value; } }

        /// <summary>
        /// Color of the slider button when moused over
        /// </summary>
        public Color SliderHighlight { get { return scrollBox.SliderHighlight; } set { scrollBox.SliderHighlight = value; } }

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

                for (int n = 0; n < scrollBox.ChainEntries.Count; n++)
                    scrollBox.ChainEntries[n].Element.Padding = value;
            }
        }

        /// <summary>
        /// Padding applied to the highlight box.
        /// </summary>
        public Vector2 HighlightPadding { get; set; }

        /// <summary>
        /// Height of entries in the list.
        /// </summary>
        public float LineHeight 
        { 
            get { return scrollBox.MemberMaxSize.Y; } 
            set { scrollBox.MemberMaxSize = new Vector2(scrollBox.MemberMaxSize.X, value); } 
        }

        /// <summary>
        /// Default format for member text;
        /// </summary>
        public GlyphFormat Format { get; set; }

        /// <summary>
        /// Minimum number of elements visible in the list at any given time.
        /// </summary>
        public int MinVisibleCount { get { return scrollBox.MinVisibleCount; } set { scrollBox.MinVisibleCount = value; } }

        /// <summary>
        /// Current selection. Null if empty.
        /// </summary>
        public ListBoxEntry<T> Selection { get; private set; }

        public readonly ScrollBox<ListBoxEntry<T>, LabelButton> scrollBox;

        protected readonly HighlightBox selectionBox, highlight;
        protected readonly BorderBox border;
        protected Vector2 _memberPadding;
        protected readonly ObjectPool<ListBoxEntry<T>> entryPool;

        public ListBox(HudParentBase parent) : base(parent)
        {
            entryPool = new ObjectPool<ListBoxEntry<T>>(GetNewEntry, ResetEntry);

            scrollBox = new ScrollBox<ListBoxEntry<T>, LabelButton>(true, this)
            {
                SizingMode = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.FitChainOffAxis,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
            };

            selectionBox = new HighlightBox(this);
            highlight = new HighlightBox(this);

            border = new BorderBox(scrollBox)
            {
                DimAlignment = DimAlignments.Both,
                Color = new Color(58, 68, 77),
                Thickness = 1f,
            };

            Format = GlyphFormat.White;
            Size = new Vector2(335f, 203f);

            HighlightPadding = new Vector2(12f, 6f);
            MemberPadding = new Vector2(20f, 6f);
            LineHeight = 30f;
        }

        public ListBox() : this(null)
        { }

        public IEnumerator<ListBoxEntry<T>> GetEnumerator() =>
            scrollBox.ChainEntries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        /// <summary>
        /// Adds a new member to the list box with the given name and associated
        /// object.
        /// </summary>
        public ListBoxEntry<T> Add(RichText name, T assocMember, bool enabled = true)
        {
            ListBoxEntry<T> entry = entryPool.Get();

            entry.Element.Text = name;
            entry.AssocMember = assocMember;
            entry.Enabled = enabled;
            scrollBox.Add(entry);

            return entry;
        }

        /// <summary>
        /// Adds the given range of entries to the list box.
        /// </summary>
        public void AddRange(IReadOnlyList<MyTuple<RichText, T, bool>> entries)
        {
            for (int n = 0; n < entries.Count; n++)
            {
                ListBoxEntry<T> entry = entryPool.Get();

                entry.Element.Text = entries[n].Item1;
                entry.AssocMember = entries[n].Item2;
                entry.Enabled = entries[n].Item3;
                scrollBox.Add(entry);
            }
        }

        /// <summary>
        /// Inserts an entry at the given index.
        /// </summary>
        public void Insert(int index, RichText name, T assocMember, bool enabled = true)
        {
            ListBoxEntry<T> entry = entryPool.Get();

            entry.Element.Text = name;
            entry.AssocMember = assocMember;
            entry.Enabled = enabled;
            scrollBox.Insert(index, entry);
        }

        /// <summary>
        /// Removes the member at the given index from the list box.
        /// </summary>
        public void RemoveAt(int index)
        {
            ListBoxEntry<T> entry = scrollBox.ChainEntries[index];
            scrollBox.RemoveAt(index);
            entryPool.Return(entry);
        }

        /// <summary>
        /// Removes the member at the given index from the list box.
        /// </summary>
        public bool Remove(ListBoxEntry<T> entry)
        {
            if (scrollBox.Remove(entry))
            {
                entryPool.Return(entry);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Removes the specified range of indices from the list box.
        /// </summary>
        public void RemoveRange(int index, int count)
        {
            for (int n = index; n < index + count; n++)
                entryPool.Return(scrollBox.ChainEntries[n]);

            scrollBox.RemoveRange(index, count);
        }

        /// <summary>
        /// Removes all entries from the list box.
        /// </summary>
        public void ClearEntries()
        {
            for (int n = 0; n < scrollBox.ChainEntries.Count; n++)
                entryPool.Return(scrollBox.ChainEntries[n]);

            scrollBox.Clear();
        }

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelection(int index)
        {
            if (index > 0 && index < scrollBox.ChainEntries.Count)
            {
                Selection = scrollBox.ChainEntries[index];
                Selection.Enabled = true;
                OnSelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelection(T assocMember)
        {
            int index = scrollBox.FindIndex(x => assocMember.Equals(x.AssocMember));

            if (index != -1)
            {
                Selection = scrollBox.ChainEntries[index];
                Selection.Enabled = true;
                OnSelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Sets the selection to the specified entry.
        /// </summary>
        public void SetSelection(ListBoxEntry<T> member)
        {
            int index = scrollBox.FindIndex(x => member.Equals(x));

            if (index != -1)
            {
                Selection = scrollBox.ChainEntries[index];
                Selection.Enabled = true;
                OnSelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private ListBoxEntry<T> GetNewEntry()
        {
            var entry = new ListBoxEntry<T>();
            entry.Element.Format = Format;
            entry.Element.Padding = _memberPadding;
            entry.Enabled = true;

            return entry;
        }

        private void ResetEntry(ListBoxEntry<T> entry)
        {
            if (Selection == entry)
                Selection = null;

            entry.Element.TextBoard.Clear();
            entry.Element.MouseInput.ClearSubscribers();
            entry.AssocMember = default(T);
            entry.Enabled = true;
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            // Make sure the selection box highlights the current selection
            if (Selection != null && Selection.Element.Visible)
            {
                selectionBox.Offset = Selection.Element.Position - selectionBox.Origin;
                selectionBox.Size = Selection.Element.Size;
                selectionBox.Visible = Selection.Element.Visible;
            }
            else
                selectionBox.Visible = false;

            highlight.Visible = false;

            for (int n = 0; n < scrollBox.ChainEntries.Count; n++)
            {
                ListBoxEntry<T> entry = scrollBox.ChainEntries[n];

                if (entry.Element.IsMousedOver)
                {
                    highlight.Visible = true;
                    highlight.Size = entry.Element.Size;
                    highlight.Offset = entry.Element.Position - highlight.Origin;

                    if (SharedBinds.LeftButton.IsNewPressed)
                    {
                        Selection = entry;
                        OnSelectionChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public object GetOrSetMember(object data, int memberEnum)
        {
            var member = (ListBoxAccessors)memberEnum;

            switch (member)
            {
                case ListBoxAccessors.ListMembers:
                    return new CollectionData
                    (
                        x => scrollBox.ChainEntries[x].GetOrSetMember, 
                        () => scrollBox.ChainEntries.Count
                     );
                case ListBoxAccessors.Add:
                    {
                        var entryData = (MyTuple<IList<RichStringMembers>, T>)data;

                        return (ApiMemberAccessor)Add(new RichText(entryData.Item1), entryData.Item2).GetOrSetMember;
                    }
                case ListBoxAccessors.Selection:
                    {
                        if (data == null)
                            return Selection;
                        else
                            SetSelection(data as ListBoxEntry<T>);

                        break;
                    }
            }

            return null;
        }

        /// <summary>
        /// A textured box with a white tab positioned on the left hand side.
        /// </summary>
        protected class HighlightBox : TexturedBox
        {
            public Color TabColor { get { return tabBoard.Color; } set { tabBoard.Color = value; } }

            private readonly MatBoard tabBoard;

            public HighlightBox(HudParentBase parent = null) : base(parent)
            {
                tabBoard = new MatBoard() { Color = new Color(223, 230, 236) };
                Color = new Color(34, 44, 53);
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
                
                if (tabBoard.Color.A > 0)
                    tabBoard.Draw(tabPos, ref ptw);
            }
        }
    }
}