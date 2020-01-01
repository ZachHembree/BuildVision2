using System;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace RichHudFramework.UI.Client
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;
    using ControlMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember
        object // ID
    >;

    public abstract class TerminalControlBase : ITerminalControl
    {
        public event Action OnControlChanged
        {
            add { GetOrSetMemberFunc(new EventAccessor(true, value), (int)TerminalControlAccessors.OnSettingChanged); }
            remove { GetOrSetMemberFunc(new EventAccessor(false, value), (int)TerminalControlAccessors.OnSettingChanged); }
        }

        public RichText Name
        {
            get { return new RichText((RichStringMembers[])GetOrSetMemberFunc(null, (int)TerminalControlAccessors.Name)); }
            set { GetOrSetMemberFunc(value.GetApiData(), (int)TerminalControlAccessors.Name); }
        }

        public bool Enabled
        {
            get { return (bool)GetOrSetMemberFunc(null, (int)TerminalControlAccessors.Enabled); }
            set { GetOrSetMemberFunc(value, (int)TerminalControlAccessors.Enabled); }
        }

        public object ID { get; }

        protected readonly ApiMemberAccessor GetOrSetMemberFunc;
        internal readonly MenuControls controlEnum;

        internal TerminalControlBase(ControlMembers data)
        {
            GetOrSetMemberFunc = data.Item1;
            ID = data.Item2;
        }

        internal TerminalControlBase(MenuControls controlEnum) : this (ModMenu.GetNewMenuControl(controlEnum))
        { }

        public ControlMembers GetApiData()
        {
            return new ControlMembers()
            {
                Item1 = GetOrSetMemberFunc,
                Item2 = ID
            };
        }

        protected object GetOrSetMember(object data, int memberEnum) =>
            GetOrSetMemberFunc(data, memberEnum);
    }

    public abstract class TerminalValue<T> : TerminalControlBase, ITerminalValue<T>
    {
        public virtual T Value
        {
            get { return (T)GetOrSetMember(null, (int)TerminalControlAccessors.Value); }
            set { GetOrSetMember(value, (int)TerminalControlAccessors.Value); }
        }

        internal TerminalValue(MenuControls controlEnum) : base(controlEnum)
        { }
    }
}