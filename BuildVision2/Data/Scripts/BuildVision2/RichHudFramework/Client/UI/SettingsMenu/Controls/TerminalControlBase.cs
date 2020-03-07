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
    /// Base type for all controls in the Rich Hud Terminal.
    /// </summary>
    public abstract class TerminalControlBase : ITerminalControl
    {
        /// <summary>
        /// Invoked whenver a change occurs to a control that requires a response, like a change
        /// to a value.
        /// </summary>
        public event Action OnControlChanged
        {
            add { GetOrSetMember(new EventAccessor(true, value), (int)TerminalControlAccessors.OnSettingChanged); }
            remove { GetOrSetMember(new EventAccessor(false, value), (int)TerminalControlAccessors.OnSettingChanged); }
        }

        /// <summary>
        /// The name of the control as it appears in the terminal.
        /// </summary>
        public string Name
        {
            get { return GetOrSetMember(null, (int)TerminalControlAccessors.Name) as string; }
            set { GetOrSetMember(value, (int)TerminalControlAccessors.Name); }
        }

        /// <summary>
        /// Determines whether or not the control should be visible in the terminal.
        /// </summary>
        public bool Enabled
        {
            get { return (bool)GetOrSetMember(null, (int)TerminalControlAccessors.Enabled); }
            set { GetOrSetMember(value, (int)TerminalControlAccessors.Enabled); }
        }

        /// <summary>
        /// Unique identifier
        /// </summary>
        public object ID { get; }

        protected readonly ApiMemberAccessor GetOrSetMember;

        internal TerminalControlBase(MenuControls controlEnum) : this(RichHudTerminal.GetNewMenuControl(controlEnum))
        { }

        internal TerminalControlBase(ControlMembers data)
        {
            GetOrSetMember = data.Item1;
            ID = data.Item2;
        }

        public ControlMembers GetApiData()
        {
            return new ControlMembers()
            {
                Item1 = GetOrSetMember,
                Item2 = ID
            };
        }
    }

    /// <summary>
    /// Clickable control used in conjunction by the settings menu.
    /// </summary>
    public abstract class TerminalControlBase<T> : TerminalControlBase, ITerminalControl<T> where T : TerminalControlBase<T>
    {
        /// <summary>
        /// Delegate invoked by OnControlChanged. Passes in a reference to the control.
        /// </summary>
        public Action<T> ControlChangedAction { get; set; }

        internal TerminalControlBase(MenuControls controlEnum) : this(RichHudTerminal.GetNewMenuControl(controlEnum))
        { }

        internal TerminalControlBase(ControlMembers data) : base(data)
        {
            OnControlChanged += UpdateControl;
        }

        private void UpdateControl()
        {
            ControlChangedAction?.Invoke(this as T);
        }
    }

    /// <summary>
    /// Base type for settings menu controls associated with a value of a given type.
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

        /// <summary>
        /// Used to periodically update the value associated with the control. Optional.
        /// </summary>
        public Func<TValue> CustomValueGetter
        {
            get { return GetOrSetMember(null, (int)TerminalControlAccessors.ValueGetter) as Func<TValue>; }
            set { GetOrSetMember(value, (int)TerminalControlAccessors.ValueGetter); }
        }

        internal TerminalValue(MenuControls controlEnum) : base(controlEnum)
        { }
    }
}