using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI
{
	using CollectionData = MyTuple<Func<int, ApiMemberAccessor>, Func<int>>;
	using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

	/// <summary>
	/// A non-scrolling list of arbitrary selectable UI elements.
	/// Specialization of SelectionBox using HudChain
	/// </summary>
	/// <typeparam name="TContainer">Container element type wrapping the UI element</typeparam>
	/// <typeparam name="TElement">UI element in the list</typeparam>
	/// <typeparam name="TValue">Data type stored associated with each entry</typeparam>
	public class ChainSelectionBox<TContainer, TElement, TValue>
		: SelectionBox<HudChain<TContainer, TElement>, TContainer, TElement, TValue>
		where TContainer : class, IListBoxEntry<TElement, TValue>, new()
		where TElement : HudElementBase, IMinLabelElement
	{
		public ChainSelectionBox(HudParentBase parent) : base(parent)
		{ }

		public ChainSelectionBox() : base(null)
		{ }
	}

	/// <summary>
	/// A scrollable list of arbitrary selectable UI elements.
	/// Generic SelectionBox using ScrollBox
	/// </summary>
	/// <typeparam name="TContainer">Container element type wrapping the UI element</typeparam>
	/// <typeparam name="TElement">UI element in the list</typeparam>
	/// <typeparam name="TValue">Data type stored associated with each entry</typeparam>
	public class ScrollSelectionBox<TContainer, TElement, TValue>
		: SelectionBox<ScrollBox<TContainer, TElement>, TContainer, TElement, TValue>
		where TContainer : class, IListBoxEntry<TElement, TValue>, new()
		where TElement : HudElementBase, IMinLabelElement
	{
		/// <summary>
		/// Background color
		/// </summary>
		public Color Color { get { return EntryChain.Color; } set { EntryChain.Color = value; } }

		/// <summary>
		/// If enabled scrolling using the scrollbar and mousewheel will be allowed
		/// </summary>
		public virtual bool EnableScrolling { get { return EntryChain.EnableScrolling; } set { EntryChain.EnableScrolling = value; } }

		/// <summary>
		/// Enable/disable smooth scrolling and range clipping
		/// </summary>
		public virtual bool UseSmoothScrolling { get { return EntryChain.UseSmoothScrolling; } set { EntryChain.UseSmoothScrolling = value; } }

		/// <summary>
		/// Minimum number of visible elements allowed. Supercedes maximum length. If the number of elements that
		/// can fit within the maximum length is less than this value, then this element will expand beyond its maximum
		/// size.
		/// </summary>
		public virtual int MinVisibleCount { get { return EntryChain.MinVisibleCount; } set { EntryChain.MinVisibleCount = value; } }

		/// <summary>
		/// Minimum total length (on the align axis) of visible members allowed in the scrollbox.
		/// </summary>
		public virtual float MinLength { get { return EntryChain.MinLength; } set { EntryChain.MinLength = value; } }

		/// <summary>
		/// Entry highlight selection box width
		/// </summary>
		/// <exclude/>
		protected override float HighlightWidth =>
			EntryChain.Size.X - Padding.X - EntryChain.ScrollBar.Width - EntryChain.Padding.X - HighlightPadding.X;

		public ScrollSelectionBox(HudParentBase parent) : base(parent)
		{ }

		public ScrollSelectionBox() : base(null)
		{ }

		/// <summary>
		/// Updates visible entry range to track input scrolling
		/// </summary>
		/// <exclude/>
		protected override void HandleInput(Vector2 cursorPos)
		{
			if (listInput.KeyboardScroll)
			{
				if (listInput.HighlightIndex > EntryChain.End)
				{
					EntryChain.End = listInput.HighlightIndex;
				}
				else if (listInput.HighlightIndex < EntryChain.Start)
				{
					EntryChain.Start = listInput.HighlightIndex;
				}
			}
		}
	}

	/// <summary>
	/// Generic list of pooled, selectable entries of uniform size.
	/// </summary>
	/// <typeparam name="TChain">Linear stacking element containing entries. May or may not be scrollable.</typeparam>
	/// <typeparam name="TContainer">Container element type wrapping the UI element</typeparam>
	/// <typeparam name="TElement">UI element in the list</typeparam>
	/// <typeparam name="TValue">Data type stored associated with each entry</typeparam>
	public class SelectionBox<TChain, TContainer, TElement, TValue>
		: SelectionBoxBase<TChain, TContainer, TElement>
		where TChain : HudChain<TContainer, TElement>, new()
		where TContainer : class, IListBoxEntry<TElement, TValue>, new()
		where TElement : HudElementBase, IMinLabelElement
	{
		/// <summary>
		/// Enables collection-initializer syntax (e.g., new SelectionBox { ListContainer = { entry1, entry2 } })
		/// </summary>
		public new SelectionBox<TChain, TContainer, TElement, TValue> ListContainer => this;

		/// <summary>
		/// Padding applied to list members.
		/// </summary>
		public Vector2 MemberPadding { get; set; }

		/// <summary>
		/// Sets padding for the list independent of selection box padding.
		/// </summary>
		public virtual Vector2 ListPadding { get { return EntryChain.Padding; } set { EntryChain.Padding = value; } }

		/// <summary>
		/// Uniform height applied to list entries
		/// </summary>
		public float LineHeight { get; set; }

		/// <summary>
		/// Tintable border surrounding the selection box
		/// </summary>
		public readonly BorderBox border;

		/// <summary>
		/// Pool of reusable entry containers
		/// </summary>
		/// <exclude/>
		protected readonly ObjectPool<TContainer> entryPool;

		public SelectionBox(HudParentBase parent) : base(parent)
		{
			entryPool = new ObjectPool<TContainer>(GetNewEntry, ResetEntry);
			EntryChain.SizingMode = HudChainSizingModes.FitMembersOffAxis;

			border = new BorderBox(EntryChain)
			{
				DimAlignment = DimAlignments.Size,
				Color = new Color(58, 68, 77),
				Thickness = 1f,
			};

			LineHeight = 28f;
			MemberPadding = new Vector2(20f, 6f);
		}

		public SelectionBox() : this(null)
		{ }

		/// <summary>
		/// Adds a new pooled container entry to the list in its default state and returns it.
		/// </summary>
		public TContainer AddNew()
		{
			TContainer entry = entryPool.Get();
			EntryChain.Add(entry);
			return entry;
		}

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
			EntryChain.Add(entry);

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
				EntryChain.Add(entry);
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
			EntryChain.Insert(index, entry);
		}

		/// <summary>
		/// Removes the member at the given index from the list box.
		/// </summary>
		public void RemoveAt(int index)
		{
			TContainer entry = EntryChain.Collection[index];
			EntryChain.RemoveAt(index);
			entryPool.Return(entry);
		}

		/// <summary>
		/// Removes the member at the given index from the list box.
		/// </summary>
		public bool Remove(TContainer entry)
		{
			if (EntryChain.Remove(entry))
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
			entryPool.ReturnRange(EntryChain.Collection, index, count - index);
			EntryChain.RemoveRange(index, count);
		}

		/// <summary>
		/// Removes all entries from the list box.
		/// </summary>
		public void ClearEntries()
		{
			ClearSelection();
			entryPool.ReturnRange(EntryChain.Collection);
			EntryChain.Clear();
		}

		/// <summary>
		/// Sets the selection to the member associated with the given object.
		/// </summary>
		public void SetSelection(TValue assocMember)
		{
			int index = EntryChain.FindIndex(x => assocMember.Equals(x.AssocMember));

			if (index != -1)
			{
				listInput.SetSelectionAt(index);
			}
		}

		/// <summary>
		/// Returns an empty entry with formatting set/reset to match the selection box
		/// </summary>
		/// <exclude/>
		protected virtual TContainer GetNewEntry()
		{
			var entry = new TContainer();
			entry.Element.TextBoard.Format = Format;
			entry.Element.Padding = MemberPadding;
			entry.Element.Height = LineHeight;
			entry.Element.ZOffset = 1;
			entry.Enabled = true;

			return entry;
		}

		/// <summary>
		/// Clears and returns an entry to the internal pool
		/// </summary>
		/// <exclude/>
		protected virtual void ResetEntry(TContainer entry)
		{
			if (Value == entry)
				listInput.ClearSelection();

			entry.Reset();
		}

		/// <summary>
		/// Internal API interop method
		/// </summary>
		/// <exclude/>
		public virtual object GetOrSetMember(object data, int memberEnum)
		{
			var member = (ListBoxAccessors)memberEnum;

			switch (member)
			{
				case ListBoxAccessors.ListMembers:
					return new CollectionData
					(
						x => EntryChain.Collection[x].GetOrSetMember,
						() => EntryChain.Collection.Count
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
							return Value;
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