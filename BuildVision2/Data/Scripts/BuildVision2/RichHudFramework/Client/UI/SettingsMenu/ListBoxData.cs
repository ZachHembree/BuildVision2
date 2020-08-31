using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework.UI
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;
    using CollectionData = MyTuple<Func<int, ApiMemberAccessor>, Func<int>>;

    public class ListBoxData<T> : ReadOnlyApiCollection<EntryData<T>>
    {
        public EntryData<T> Selection 
        {
            get 
            {
                object id = GetOrSetMemberFunc(null, (int)ListBoxAccessors.Selection);

                if (id != null)
                {
                    for (int n = 0; n < Count; n++)
                    {
                        if (this[n].ID == id)
                            return this[n];
                    }
                }

                return null;
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

        public EntryData<T> Add(RichText text, T assocObject)
        {
            var data = new MyTuple<IList<RichStringMembers>, object>()
            {
                Item1 = text.ApiData,
                Item2 = assocObject
            };

            return new EntryData<T>((ApiMemberAccessor)GetOrSetMemberFunc(data, (int)ListBoxAccessors.Add));
        }

        public void SetSelection(EntryData<T> entry) =>
            GetOrSetMemberFunc(entry.ID, (int)ListBoxAccessors.Selection);
    }

    public class EntryData<T>
    {
        /// <summary>
        /// Indicates whether or not the element will appear in the list
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetOrSetMemberFunc(null, (int)ListBoxEntryAccessors.Enabled); }
            set { GetOrSetMemberFunc(value, (int)ListBoxEntryAccessors.Enabled); }
        }
        
        /// <summary>
        /// Object paired with the entry
        /// </summary>
        public T AssocObject
        {
            get { return (T)GetOrSetMemberFunc(null, (int)ListBoxEntryAccessors.AssocObject); }
            set { GetOrSetMemberFunc(value, (int)ListBoxEntryAccessors.AssocObject); }
        }

        /// <summary>
        /// Unique identifier
        /// </summary>
        public object ID => GetOrSetMemberFunc(null, (int)ListBoxEntryAccessors.ID);

        private readonly ApiMemberAccessor GetOrSetMemberFunc;

        public EntryData(ApiMemberAccessor GetOrSetMemberFunc)
        {
            this.GetOrSetMemberFunc = GetOrSetMemberFunc;
        }
    }
}