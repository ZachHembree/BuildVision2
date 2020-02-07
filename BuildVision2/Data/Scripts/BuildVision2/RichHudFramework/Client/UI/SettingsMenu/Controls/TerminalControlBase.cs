using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework.UI.Client
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;
    using ControlMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember
        object // ID
    >;

    /// <summary>
    /// Abstract base for all controls in the settings menu accessible via the Framework API.
    /// </summary>
    public abstract class TerminalControlBase : ITerminalControl
    {
        /// <summary>
        /// Invoked whenver a change occurs to a control that requires a response, like a change
        /// to a value.
        /// </summary>
        public event Action OnControlChanged
        {
            add { GetOrSetMemberFunc(new EventAccessor(true, value), (int)TerminalControlAccessors.OnSettingChanged); }
            remove { GetOrSetMemberFunc(new EventAccessor(false, value), (int)TerminalControlAccessors.OnSettingChanged); }
        }

        /// <summary>
        /// The name of the control as rendred in the terminal.
        /// </summary>
        public RichText Name
        {
            get { return new RichText(GetOrSetMemberFunc(null, (int)TerminalControlAccessors.Name) as IList<RichStringMembers>); }
            set { GetOrSetMemberFunc(value.ApiData, (int)TerminalControlAccessors.Name); }
        }

        /// <summary>
        /// Determines whether or not the control should be visible in the terminal.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetOrSetMemberFunc(null, (int)TerminalControlAccessors.Enabled); }
            set { GetOrSetMemberFunc(value, (int)TerminalControlAccessors.Enabled); }
        }

        public object ID { get; }

        protected readonly ApiMemberAccessor GetOrSetMemberFunc;

        internal TerminalControlBase(MenuControls controlEnum) : this(RichHudTerminal.GetNewMenuControl(controlEnum))
        { }

        internal TerminalControlBase(ControlMembers data)
        {
            GetOrSetMemberFunc = data.Item1;
            ID = data.Item2;
        }

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

    public abstract class TerminalControlBase<T> : TerminalControlBase, ITerminalControl<T> where T : TerminalControlBase<T>
    {
        /// <summary>
        /// Delegate invoked by OnControlChanged. Passes in a reference of type calling.
        /// </summary>
        public Action<T> ControlChangedAction { get; set; }

        internal TerminalControlBase(MenuControls controlEnum) : this(RichHudTerminal.GetNewMenuControl(controlEnum))
        { }

        internal TerminalControlBase(ControlMembers data) : base(data)
        {
            OnControlChanged += UpdateControl;
        }

        protected virtual void UpdateControl()
        {
            ControlChangedAction?.Invoke(this as T);
        }
    }

    /// <summary>
    /// Abstract base for all settings menu controls associated with a given type of value.
    /// </summary>
    public abstract class TerminalValue<TValue, TCon> : TerminalControlBase<TCon>, ITerminalValue<TValue, TCon> where TCon : TerminalControlBase<TCon>
    {
        /// <summary>
        /// Value associated with the control.
        /// </summary>
        public virtual TValue Value
        {
            get { return (TValue)GetOrSetMember(null, (int)TerminalControlAccessors.Value); }
            set { GetOrSetMember(value, (int)TerminalControlAccessors.Value); }
        }

        public Func<TValue> CustomValueGetter
        {
            get { return GetOrSetMember(null, (int)TerminalControlAccessors.ValueGetter) as Func<TValue>; }
            set { GetOrSetMember(value, (int)TerminalControlAccessors.ValueGetter); }
        }

        public Action<TValue> CustomValueSetter
        {
            get { return GetOrSetMember(null, (int)TerminalControlAccessors.ValueSetter) as Action<TValue>; }
            set { GetOrSetMember(value, (int)TerminalControlAccessors.ValueSetter); }
        }

        internal TerminalValue(MenuControls controlEnum) : base(controlEnum)
        { }
    }
}