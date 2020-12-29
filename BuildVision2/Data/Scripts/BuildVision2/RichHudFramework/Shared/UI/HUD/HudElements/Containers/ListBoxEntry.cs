using System;
using System.Text;
using VRage;
using System.Collections.Generic;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework.UI
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    public enum ListBoxEntryAccessors : int
    {
        /// <summary>
        /// IList<RichStringMembers>
        /// </summary>
        Name = 1,

        /// <summary>
        /// bool
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
    /// Label button assocated with an object of type T. Used in conjunction with list boxes.
    /// </summary>
    public class ListBoxEntry<T> : ScrollBoxEntryTuple<LabelButton, T>
    {
        private readonly LabelButton button;

        public ListBoxEntry()
        {
            button = new LabelButton() { AutoResize = false };
            Element = button;
            Element.ZOffset = 1;
        }

        public object GetOrSetMember(object data, int memberEnum)
        {
            var member = (ListBoxEntryAccessors)memberEnum;

            switch (member)
            {
                case ListBoxEntryAccessors.Name:
                    {
                        if (data == null)
                            Element.Text = new RichText(data as IList<RichStringMembers>);
                        else
                            return Element.Text.ApiData;

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