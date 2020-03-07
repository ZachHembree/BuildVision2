using System;
using System.Text;
using VRage;
using System.Collections.Generic;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework.UI
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

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
        /// IList<RichStringMembers>
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
    /// Text button assocated with an object of type T. Used in conjunction with list boxes. Implements IListBoxEntry.
    /// </summary>
    public class ListBoxEntry<T> : LabelButton, IListBoxEntry
    {
        /// <summary>
        /// Invoked on left click
        /// </summary>
        public event Action<ListBoxEntry<T>> OnMemberSelected;

        /// <summary>
        /// Determines whether or not the entry will be visible
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Object associated with the entry
        /// </summary>
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
                            TextBoard.SetText(new RichText(data as IList<RichStringMembers>));
                        else
                            return TextBoard.GetText().ApiData;

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