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

    /// <summary>
    /// Generic SelectionBox using HudChain
    /// </summary>
    public class ChainSelectionBox<TContainer, TElement, TValue>
        : SelectionBox<HudChain<TContainer, TElement>, TContainer, TElement, TValue>
        where TContainer : class, IListBoxEntry<TElement, TValue>, new()
        where TElement : HudElementBase, IMinLabelElement
    { }

    /// <summary>
    /// Generic SelectionBox using ScrollBox
    /// </summary>
    public class ScrollSelectionBox<TContainer, TElement, TValue>
        : SelectionBox<ScrollBox<TContainer, TElement>, TContainer, TElement, TValue>
        where TContainer : class, IListBoxEntry<TElement, TValue>, new()
        where TElement : HudElementBase, IMinLabelElement
    { }

    /// <summary>
    /// Generic list of pooled, selectable entries of fixed size.
    /// </summary>
    public class SelectionBox<TChain, TContainer, TElement, TValue> 
        : SelectionBoxBase<TChain, TContainer, TElement>
        where TChain : HudChain<TContainer, TElement>, new()
        where TContainer : class, IListBoxEntry<TElement, TValue>, new()
        where TElement : HudElementBase, IMinLabelElement
    {
        /// <summary>
        /// Padding applied to list members.
        /// </summary>
        public Vector2 MemberPadding { get; set; }

        /// <summary>
        /// Height of entries in the list.
        /// </summary>
        public float LineHeight
        {
            get { return hudChain.MemberMaxSize.Y; }
            set { hudChain.MemberMaxSize = new Vector2(hudChain.MemberMaxSize.X, value); }
        }

        public readonly BorderBox border;
        protected readonly ObjectPool<TContainer> entryPool;

        public SelectionBox(HudParentBase parent) : base(parent)
        {
            entryPool = new ObjectPool<TContainer>(GetNewEntry, ResetEntry);
            hudChain.SizingMode = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.ClampChainOffAxis;

            border = new BorderBox(hudChain)
            {
                DimAlignment = DimAlignments.Both,
                Color = new Color(58, 68, 77),
                Thickness = 1f,
            };

            LineHeight = 28f;
            MemberPadding = new Vector2(20f, 6f);
        }

        public SelectionBox() : this(null)
        { }

        /// <summary>
        /// Adds a new member to the list box with the given name and associated
        /// object.
        /// </summary>
        public TContainer Add(RichText name, TValue assocMember, bool enabled = true)
        {
            TContainer entry = entryPool.Get();

            entry.Element.TextBoard.SetText(name);
            entry.AssocMember = assocMember;
            entry.Enabled = enabled;
            hudChain.Add(entry);

            return entry;
        }

        /// <summary>
        /// Adds the given range of entries to the list box.
        /// </summary>
        public void AddRange(IReadOnlyList<MyTuple<RichText, TValue, bool>> entries)
        {
            for (int n = 0; n < entries.Count; n++)
            {
                TContainer entry = entryPool.Get();

                entry.Element.TextBoard.SetText(entries[n].Item1);
                entry.AssocMember = entries[n].Item2;
                entry.Enabled = entries[n].Item3;
                hudChain.Add(entry);
            }
        }

        /// <summary>
        /// Inserts an entry at the given index.
        /// </summary>
        public void Insert(int index, RichText name, TValue assocMember, bool enabled = true)
        {
            TContainer entry = entryPool.Get();

            entry.Element.TextBoard.SetText(name);
            entry.AssocMember = assocMember;
            entry.Enabled = enabled;
            hudChain.Insert(index, entry);
        }

        /// <summary>
        /// Removes the member at the given index from the list box.
        /// </summary>
        public void RemoveAt(int index)
        {
            TContainer entry = hudChain.Collection[index];
            hudChain.RemoveAt(index);
            entryPool.Return(entry);
        }

        /// <summary>
        /// Removes the member at the given index from the list box.
        /// </summary>
        public bool Remove(TContainer entry)
        {
            if (hudChain.Remove(entry))
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
                entryPool.Return(hudChain.Collection[n]);

            hudChain.RemoveRange(index, count);
        }

        /// <summary>
        /// Removes all entries from the list box.
        /// </summary>
        public void ClearEntries()
        {
            for (int n = 0; n < hudChain.Collection.Count; n++)
                entryPool.Return(hudChain.Collection[n]);

            hudChain.Clear();
        }

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelection(TValue assocMember)
        {
            int index = hudChain.FindIndex(x => assocMember.Equals(x.AssocMember));

            if (index != -1)
            {
                listInput.SetSelectionAt(index);
            }
        }

        protected override void Layout()
        {
            for (int n = 0; n < hudChain.Collection.Count; n++)
                hudChain.Collection[n].Element.Padding = MemberPadding;
        }

        protected virtual TContainer GetNewEntry()
        {
            var entry = new TContainer();
            entry.Element.TextBoard.Format = Format;
            entry.Element.Padding = MemberPadding;
            entry.Element.ZOffset = 1;
            entry.Enabled = true;

            return entry;
        }

        protected virtual void ResetEntry(TContainer entry)
        {
            if (Selection == entry)
                listInput.ClearSelection();

            entry.Element.TextBoard.Clear();
            entry.AssocMember = default(TValue);
            entry.Enabled = true;
        }

        public virtual object GetOrSetMember(object data, int memberEnum)
        {
            var member = (ListBoxAccessors)memberEnum;

            switch (member)
            {
                case ListBoxAccessors.ListMembers:
                    return new CollectionData
                    (
                        x => hudChain.Collection[x].GetOrSetMember,
                        () => hudChain.Collection.Count
                     );
                case ListBoxAccessors.Add:
                    {
                        if (data is MyTuple<List<RichStringMembers>, TValue>)
                        {
                            var entryData = (MyTuple<List<RichStringMembers>, TValue>)data;
                            return (ApiMemberAccessor)Add(new RichText(entryData.Item1), entryData.Item2).GetOrSetMember;
                        }
                        else
                        {
                            var entryData = (MyTuple<IList<RichStringMembers>, TValue>)data;
                            var stringList = entryData.Item1 as List<RichStringMembers>;
                            return (ApiMemberAccessor)Add(new RichText(stringList), entryData.Item2).GetOrSetMember;
                        }
                    }
                case ListBoxAccessors.Selection:
                    {
                        if (data == null)
                            return Selection;
                        else
                            SetSelection(data as TContainer);

                        break;
                    }
                case ListBoxAccessors.SelectionIndex:
                    {
                        if (data == null)
                            return SelectionIndex;
                        else
                            SetSelectionAt((int)data); break;
                    }
                case ListBoxAccessors.SetSelectionAtData:
                    SetSelection((TValue)data); break;
                case ListBoxAccessors.Insert:
                    {
                        var entryData = (MyTuple<int, List<RichStringMembers>, TValue>)data;
                        Insert(entryData.Item1, new RichText(entryData.Item2), entryData.Item3);
                        break;
                    }
                case ListBoxAccessors.Remove:
                    return Remove(data as TContainer);
                case ListBoxAccessors.RemoveAt:
                    RemoveAt((int)data); break;
                case ListBoxAccessors.ClearEntries:
                    ClearEntries(); break;
            }

            return null;
        }
    }
}