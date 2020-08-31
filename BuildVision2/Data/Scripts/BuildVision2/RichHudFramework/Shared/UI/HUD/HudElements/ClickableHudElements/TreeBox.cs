﻿using System;
using System.Collections.Generic;
using VRageMath;
using VRage;

namespace RichHudFramework.UI
{
    using Rendering;
    using System.Collections;

    /// <summary>
    /// Indented, collapsable list. Designed to fit in with SE UI elements.
    /// </summary>
    public class TreeBox<T> : HudElementBase, IEntryBox<T>, IClickableElement
    {
        /// <summary>
        /// Invoked when a list member is selected.
        /// </summary>
        public event EventHandler OnSelectionChanged;

        /// <summary>
        /// List of entries in the treebox.
        /// </summary>
        public IReadOnlyList<ListBoxEntry<T>> ListEntries => entryChain.ChainEntries;

        /// <summary>
        /// Used to allow the addition of list entries using collection-initializer syntax in
        /// conjunction with normal initializers.
        /// </summary>
        public TreeBox<T> ListContainer => this;

        /// <summary>
        /// If true, then the dropdown list will be open
        /// </summary>
        public bool ListOpen { get; set; }

        /// <summary>
        /// Height of the treebox in pixels.
        /// </summary>
        public override float Height
        {
            get
            {
                if (!ListOpen)
                    return display.Height + Padding.Y;
                else
                    return display.Height + entryChain.Height + Padding.Y;
            }
            set
            {
                if (Padding.Y < value)
                    value -= Padding.Y;

                if (!ListOpen)
                {
                    display.Height = value;
                    entryChain.MemberMaxSize = new Vector2(entryChain.MemberMaxSize.X, value);
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
        public int Count => entryChain.ChainEntries.Count;

        /// <summary>
        /// Determines how far to the right list members should be offset from the position of the header.
        /// </summary>
        public float IndentSize 
        { 
            get { return entryChain.Padding.X; } 
            set 
            {
                entryChain.Padding = new Vector2(value, entryChain.Padding.Y);
                entryChain.Offset = entryChain.Padding / 2f;
            } 
         }

        /// <summary>
        /// Handles mouse input for the header.
        /// </summary>
        public IMouseInput MouseInput => display.MouseInput;

        public HudElementBase Display => display;

        public readonly HudChain<ListBoxEntry<T>, LabelButton> entryChain;

        protected readonly TreeBoxDisplay display;
        protected readonly HighlightBox highlight, selectionBox;
        private readonly ObjectPool<ListBoxEntry<T>> entryPool;

        public TreeBox(HudParentBase parent = null) : base(parent)
        {
            entryPool = new ObjectPool<ListBoxEntry<T>>(GetNewEntry, ResetEntry);

            display = new TreeBoxDisplay(this)
            {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.InnerV | ParentAlignments.UsePadding,
                DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding
            };

            selectionBox = new HighlightBox(display)
            { Color = new Color(34, 44, 53) };

            highlight = new HighlightBox(display)
            { Color = new Color(34, 44, 53) };

            entryChain = new HudChain<ListBoxEntry<T>, LabelButton>(true, display)
            {
                Visible = false,
                DimAlignment = DimAlignments.Width,
                SizingMode = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.FitChainBoth,
                ParentAlignment = ParentAlignments.Bottom | ParentAlignments.Right | ParentAlignments.InnerH | ParentAlignments.UsePadding,
            };

            Size = new Vector2(200f, 34f);
            IndentSize = 40f;

            Format = GlyphFormat.Blueish;
            display.Name = "NewTreeBox";

            display.MouseInput.OnLeftClick += ToggleList;
        }

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelection(T assocMember)
        {
            ListBoxEntry<T> result = entryChain.Find(x => assocMember.Equals(x.AssocMember));

            if (result != null)
            {
                Selection = result;
                OnSelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Sets the selection to the specified entry.
        /// </summary>
        public void SetSelection(ListBoxEntry<T> member)
        {
            ListBoxEntry<T> result = entryChain.Find(x => member.Equals(x));

            if (result != null)
            {
                Selection = result;
                OnSelectionChanged?.Invoke(this, EventArgs.Empty);
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
        /// Adds a new member to the tree box with the given name and associated
        /// object.
        /// </summary>
        public ListBoxEntry<T> Add(RichText name, T assocMember, bool enabled = true)
        {
            ListBoxEntry<T> entry = entryPool.Get();

            entry.Element.Text = name;
            entry.AssocMember = assocMember;
            entry.Enabled = enabled;
            entryChain.Add(entry);

            return entry;
        }

        /// <summary>
        /// Adds the given range of entries to the tree box.
        /// </summary>
        public void AddRange(IReadOnlyList<MyTuple<RichText, T, bool>> entries)
        {
            for (int n = 0; n < entries.Count; n++)
            {
                ListBoxEntry<T> entry = entryPool.Get();

                entry.Element.Text = entries[n].Item1;
                entry.AssocMember = entries[n].Item2;
                entry.Enabled = entries[n].Item3;
                entryChain.Add(entry);
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
            entryChain.Insert(index, entry);
        }

        /// <summary>
        /// Removes the member at the given index from the tree box.
        /// </summary>
        public void RemoveAt(int index)
        {
            ListBoxEntry<T> entry = entryChain.ChainEntries[index];
            entryChain.RemoveAt(index);
            entryPool.Return(entry);
        }

        /// <summary>
        /// Removes the specified range of indices from the tree box.
        /// </summary>
        public void RemoveRange(int index, int count)
        {
            for (int n = index; n < index + count; n++)
                entryPool.Return(entryChain.ChainEntries[n]);

            entryChain.RemoveRange(index, count);
        }

        /// <summary>
        /// Removes all entries from the tree box.
        /// </summary>
        public void ClearEntries()
        {
            for (int n = 0; n < entryChain.ChainEntries.Count; n++)
                entryPool.Return(entryChain.ChainEntries[n]);

            entryChain.Clear();
        }

        private ListBoxEntry<T> GetNewEntry()
        {
            var entry = new ListBoxEntry<T>();
            entry.Element.Format = Format;
            entry.Element.Padding = new Vector2(24f, 0f);
            entry.Enabled = true;

            return entry;
        }

        private void ResetEntry(ListBoxEntry<T> entry)
        {
            entry.Element.TextBoard.Clear();
            entry.Element.MouseInput.ClearSubscribers();
            entry.AssocMember = default(T);
            entry.Enabled = true;
        }

        private void ToggleList(object sender, EventArgs args)
        {
            if (!ListOpen)
                OpenList();
            else
                CloseList();
        }

        private void OpenList()
        {
            entryChain.Visible = true;
            display.Open = true;
            ListOpen = true;
        }

        private void CloseList()
        {
            entryChain.Visible = false;
            display.Open = false;
            ListOpen = false;
        }

        protected override void Layout()
        {
            if (Selection != null)
            {
                selectionBox.Offset = Selection.Element.Position - selectionBox.Origin;
                selectionBox.Size = Selection.Element.Size;
                selectionBox.Visible = Selection.Element.Visible;
            }
            else
                selectionBox.Visible = false;

            for (int n = 0; n < entryChain.ChainEntries.Count; n++)
            {
                ListBoxEntry<T> entry = entryChain.ChainEntries[n];
                entry.Element.Visible = entry.Enabled;
            }
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            highlight.Visible = false;

            for (int n = 0; n < entryChain.ChainEntries.Count; n++)
            {
                ListBoxEntry<T> entry = entryChain.ChainEntries[n];

                if (entry.Element.IsMousedOver)
                {
                    highlight.Visible = true;
                    highlight.Size = entry.Element.Size;
                    highlight.Offset = entry.Element.Position - highlight.Origin;

                    if (entry.Element.MouseInput.IsNewLeftClicked)
                    {
                        Selection = entry;
                        OnSelectionChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public IEnumerator<ListBoxEntry<T>> GetEnumerator() =>
            entryChain.ChainEntries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            entryChain.ChainEntries.GetEnumerator();

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
                Color = Color = new Color(34, 44, 53);
            }

            protected override void Layout()
            {
                hudBoard.Size = cachedSize - cachedPadding;
                tabBoard.Size = new Vector2(4f * Scale, cachedSize.Y - cachedPadding.Y);
            }

            protected override void Draw(object matrix)
            {
                var ptw = (MatrixD)matrix;

                if (hudBoard.Color.A > 0)
                    hudBoard.Draw(cachedPosition, ref ptw);

                // Left align the tab
                Vector2 tabPos = cachedPosition;
                tabPos.X += (-hudBoard.Size.X + tabBoard.Size.X) / 2f;

                if (tabBoard.Color.A > 0)
                    tabBoard.Draw(tabPos, ref ptw);
            }
        }

        /// <summary>
        /// Modified dropdown header with a rotating arrow on the left side indicating
        /// whether the list is open.
        /// </summary>
        protected class TreeBoxDisplay : HudElementBase
        {
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
            private readonly HudChain layout;
            private readonly MouseInputElement mouseInput;

            private static readonly Material 
                downArrow = new Material("RichHudDownArrow", new Vector2(64f, 64f)), 
                rightArrow = new Material("RichHudRightArrow", new Vector2(64f, 64f));

            public TreeBoxDisplay(HudParentBase parent = null) : base(parent)
            {
                background = new TexturedBox(this)
                {
                    Color = new Color(41, 54, 62),
                    DimAlignment = DimAlignments.Both,
                };

                name = new Label()
                {
                    AutoResize = false,
                    Padding = new Vector2(10f, 0f),
                    Format = GlyphFormat.Blueish.WithSize(1.1f),
                };

                divider = new TexturedBox()
                {
                    Padding = new Vector2(2f, 6f),
                    Size = new Vector2(2f, 39f),
                    Color = new Color(104, 113, 120),
                };

                arrow = new TexturedBox()
                {
                    Width = 20f,
                    Padding = new Vector2(8f, 0f),
                    MatAlignment = MaterialAlignment.FitHorizontal,
                    Color = new Color(227, 230, 233),
                    Material = rightArrow,
                };

                layout = new HudChain(false, this)
                {
                    SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.FitChainBoth,
                    DimAlignment = DimAlignments.Height | DimAlignments.IgnorePadding,
                    ChainContainer = { arrow, divider, name }
                };

                mouseInput = new MouseInputElement(this)
                {
                    DimAlignment = DimAlignments.Both
                };
            }

            protected override void Layout()
            {
                name.Width = (Width - Padding.X) - divider.Width - arrow.Width;
            }
        }
    }
}