using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI
{
	using CollectionData = MyTuple<Func<int, ApiMemberAccessor>, Func<int>>;
	using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

	/// <summary>
	/// Manages the underlying data model for ListBox and Dropdown controls.
	/// <para>This class handles the list entries and selection state, communicating with the master module via API.</para>
	/// </summary>
	/// <exclude/>
	public class ListBoxData<T> : ReadOnlyApiCollection<EntryData<T>>
	{
		/// <summary>
		/// The currently selected entry. Returns null if the list is empty or no selection is made.
		/// </summary>
		public EntryData<T> Selection
		{
			get
			{
				var index = (int)GetOrSetMemberFunc(null, (int)ListBoxAccessors.SelectionIndex);
				return (index != -1) ? this[index] : null;
			}
		}

		/// <summary>
		/// The index of the current selection. Returns -1 if empty or no selection.
		/// </summary>
		public int SelectionIndex
		{
			get
			{
				return (int)GetOrSetMemberFunc(null, (int)ListBoxAccessors.SelectionIndex);
			}
		}

		private readonly ApiMemberAccessor GetOrSetMemberFunc;

		public ListBoxData(ApiMemberAccessor GetOrSetMemberFunc) : base(GetListData(GetOrSetMemberFunc))
		{
			this.GetOrSetMemberFunc = GetOrSetMemberFunc;
		}

		private static MyTuple<Func<int, EntryData<T>>, Func<int>> GetListData(ApiMemberAccessor GetOrSetMemberFunc)
		{
			var listData = (CollectionData)GetOrSetMemberFunc(null, (int)ListBoxAccessors.ListMembers);
			Func<int, EntryData<T>> GetEntryFunc = x => new EntryData<T>(listData.Item1(x));

			return new MyTuple<Func<int, EntryData<T>>, Func<int>>()
			{
				Item1 = GetEntryFunc,
				Item2 = listData.Item2
			};
		}

		/// <summary>
		/// Adds a new member to the list box with the given display text and associated data object.
		/// </summary>
		public void Add(RichText text, T assocObject)
		{
			var data = new MyTuple<List<RichStringMembers>, object>()
			{
				Item1 = text.apiData,
				Item2 = assocObject
			};

			GetOrSetMemberFunc(data, (int)ListBoxAccessors.Add);
		}

		/// <summary>
		/// Inserts a new entry at the specified index.
		/// </summary>
		public void Insert(int index, RichText text, T assocObject)
		{
			var data = new MyTuple<int, List<RichStringMembers>, object>()
			{
				Item1 = index,
				Item2 = text.apiData,
				Item3 = assocObject
			};

			GetOrSetMemberFunc(data, (int)ListBoxAccessors.Insert);
		}

		/// <summary>
		/// Removes the specified entry from the list.
		/// </summary>
		public bool Remove(EntryData<T> entry) =>
			(bool)GetOrSetMemberFunc(entry.ID, (int)ListBoxAccessors.Remove);

		/// <summary>
		/// Removes the entry at the specified index.
		/// </summary>
		public void RemoveAt(int index) =>
			GetOrSetMemberFunc(index, (int)ListBoxAccessors.RemoveAt);

		/// <summary>
		/// Clears all entries from the list.
		/// </summary>
		public void Clear() =>
			GetOrSetMemberFunc(null, (int)ListBoxAccessors.ClearEntries);

		/// <summary>
		/// Sets the selection to the specified entry object.
		/// </summary>
		public void SetSelection(EntryData<T> entry) =>
			GetOrSetMemberFunc(entry.ID, (int)ListBoxAccessors.Selection);

		/// <summary>
		/// Sets the selection to the first entry associated with the given data object.
		/// </summary>
		public void SetSelection(T assocMember) =>
			GetOrSetMemberFunc(assocMember, (int)ListBoxAccessors.SetSelectionAtData);

		/// <summary>
		/// Sets the selection to the entry at the specified index.
		/// </summary>
		public void SetSelection(int index) =>
			GetOrSetMemberFunc(index, (int)ListBoxAccessors.SelectionIndex);
	}

	/// <summary>
	/// Represents a single entry in a ListBox or Dropdown.
	/// </summary>
	public class EntryData<T>
	{
		/// <summary>
		/// The display name/text of the entry in the UI.
		/// </summary>
		public RichText Text
		{
			get { return new RichText(GetOrSetMemberFunc(null, (int)ListBoxEntryAccessors.Name) as List<RichStringMembers>); }
			set { GetOrSetMemberFunc(value.apiData, (int)ListBoxEntryAccessors.Name); }
		}

		/// <summary>
		/// Indicates whether or not the element is visible in the list.
		/// </summary>
		public bool Enabled
		{
			get { return (bool)GetOrSetMemberFunc(null, (int)ListBoxEntryAccessors.Enabled); }
			set { GetOrSetMemberFunc(value, (int)ListBoxEntryAccessors.Enabled); }
		}

		/// <summary>
		/// The generic data object associated with this entry.
		/// </summary>
		public T AssocObject
		{
			get { return (T)GetOrSetMemberFunc(null, (int)ListBoxEntryAccessors.AssocObject); }
			set { GetOrSetMemberFunc(value, (int)ListBoxEntryAccessors.AssocObject); }
		}

		/// <summary>
		/// Unique identifier.
		/// </summary>
		/// <exclude/>
		public object ID => GetOrSetMemberFunc(null, (int)ListBoxEntryAccessors.ID);

		private readonly ApiMemberAccessor GetOrSetMemberFunc;

		public EntryData(ApiMemberAccessor GetOrSetMemberFunc)
		{
			this.GetOrSetMemberFunc = GetOrSetMemberFunc;
		}
	}
}