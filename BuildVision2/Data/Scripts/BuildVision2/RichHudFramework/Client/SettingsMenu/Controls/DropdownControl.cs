using System;
using System.Text;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace RichHudFramework.UI.Client
{
    using CollectionData = MyTuple<Func<int, ApiMemberAccessor>, Func<int>>;

    public class DropdownControl<T> : TerminalValue<EntryData<T>, DropdownControl<T>>
    {
        public override EntryData<T> Value
        {
            get { return List.Selection; }
            set { List.SetSelection(value); }
        }

        public ListBoxData<T> List { get; }

        public DropdownControl() : base(MenuControls.DropdownControl)
        {
            var listData = (ApiMemberAccessor)GetOrSetMemberFunc(null, (int)ListControlAccessors.ListAccessors);
            
            List = new ListBoxData<T>(GetOrSetMemberFunc);
        }
    }
}