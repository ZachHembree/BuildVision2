using System;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework.UI.Client
{
	using ControlMembers = MyTuple<
		ApiMemberAccessor, // GetOrSetMember
		object // ID
	>;

	/// <summary>
	/// The abstract base class for all controls in the <see cref="RichHudTerminal"/>
	/// </summary>
	public abstract class TerminalControlBase : ITerminalControl
	{
		/// <summary>
		/// Invoked whenever a change occurs to the control that requires a response (e.g., value change, user interaction).
		/// </summary>
		public event EventHandler ControlChanged;

		/// <summary>
		/// Control change callback initializer property
		/// </summary>
		public EventHandler ControlChangedHandler { set { ControlChanged += value; } }

		/// <summary>
		/// The name or label of the control as it appears in the terminal UI.
		/// </summary>
		public string Name
		{
			get { return GetOrSetMember(null, (int)TerminalControlAccessors.Name) as string; }
			set { GetOrSetMember(value, (int)TerminalControlAccessors.Name); }
		}

		/// <summary>
		/// Determines whether or not the control is visible and interactive in the terminal.
		/// </summary>
		public bool Enabled
		{
			get { return (bool)GetOrSetMember(null, (int)TerminalControlAccessors.Enabled); }
			set { GetOrSetMember(value, (int)TerminalControlAccessors.Enabled); }
		}

		/// <summary>
		/// Optional tooltip text displayed when hovering over the control.
		/// </summary>
		public ToolTip ToolTip
		{
			get { return _toolTip; }
			set { _toolTip = value; GetOrSetMember(value.GetToolTipFunc, (int)TerminalControlAccessors.ToolTip); }
		}

		/// <summary>
		/// Unique identifier used by the Framework API.
		/// </summary>
		/// <exclude/>
		public object ID { get; }

		/// <summary>
		/// Internal API member accessor delegate.
		/// </summary>
		/// <exclude/>
		protected readonly ApiMemberAccessor GetOrSetMember;

		/// <summary>
		/// Internal tooltip cache.
		/// </summary>
		/// <exclude/>
		protected ToolTip _toolTip;

		/// <summary>
		/// Initializes a new RHF terminal control corresponding to the given enum type.
		/// </summary>
		/// <exclude/>
		public TerminalControlBase(MenuControls controlEnum) : this(RichHudTerminal.Instance.GetNewMenuControl(controlEnum))
		{
			// Register event callback
			GetOrSetMember(new Action(ControlChangedCallback), (int)TerminalControlAccessors.GetOrSetControlCallback);
		}

		/// <summary>
		/// Internal callback wrapper for safe event invocation.
		/// </summary>
		/// <exclude/>
		protected virtual void ControlChangedCallback()
		{
			if (ControlChanged == null)
				return;

			Internal.ExceptionHandler.Run(() =>
			{
				ControlChanged.Invoke(this, EventArgs.Empty);
			});
		}

		/// <summary>
		/// Initializes control data from internal RHF API data.
		/// </summary>
		/// <exclude/>
		public TerminalControlBase(ControlMembers data)
		{
			GetOrSetMember = data.Item1;
			ID = data.Item2;
		}

		/// <summary>
		/// Returns the internal API data tuple.
		/// </summary>
		/// <exclude/>
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
	/// Abstract base class for terminal controls that are associated with a specific data value 
	/// (e.g., sliders, checkboxes) in the <see cref="RichHudTerminal"/>.
	/// </summary>
	/// <typeparam name="TValue">The type of the value associated with this control.</typeparam>
	public abstract class TerminalValue<TValue> : TerminalControlBase, ITerminalValue<TValue>
	{
		/// <summary>
		/// The current value associated with the control.
		/// </summary>
		public virtual TValue Value
		{
			get { return (TValue)GetOrSetMember(null, (int)TerminalControlAccessors.Value); }
			set { GetOrSetMember(value, (int)TerminalControlAccessors.Value); }
		}

		/// <summary>
		/// An optional delegate used to periodically retrieve the value from an external source, keeping the control in sync.
		/// </summary>
		public Func<TValue> CustomValueGetter
		{
			get { return GetOrSetMember(null, (int)TerminalControlAccessors.ValueGetter) as Func<TValue>; }
			set { GetOrSetMember(value, (int)TerminalControlAccessors.ValueGetter); }
		}

		public TerminalValue(MenuControls controlEnum) : base(controlEnum)
		{ }
	}
}