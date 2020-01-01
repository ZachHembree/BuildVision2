using System;
using System.Text;
using VRage;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;

namespace RichHudFramework.UI.Client
{
    using CollectionData = MyTuple<Func<int, ApiMemberAccessor>, Func<int>>;

    internal enum ListControlAccessors : int
    {
        ListAccessors = 16,
    }

    public class ListControl<T> : TerminalValue<EntryData<T>>
    {
        public override EntryData<T> Value
        {
            get { return List.Selection; }
            set { List.SetSelection(value); }
        }

        public ListBoxData<T> List { get; }

        public ListControl() : base(MenuControls.ListControl)
        {
            var listData = (ApiMemberAccessor)GetOrSetMemberFunc(null, (int)ListControlAccessors.ListAccessors);

            List = new ListBoxData<T>(GetOrSetMemberFunc);
        }
    }
}