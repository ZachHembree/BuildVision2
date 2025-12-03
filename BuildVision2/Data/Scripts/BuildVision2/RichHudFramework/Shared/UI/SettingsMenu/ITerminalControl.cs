using System;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
	using ControlMembers = MyTuple<
		ApiMemberAccessor, // GetOrSetMember
		object // ID
	>;

	namespace UI
	{
		/// <summary>
		/// Internal member accessor enums for terminal controls
		/// </summary>
		/// <exclude/>
		public enum TerminalControlAccessors : int
		{
			/// <summary>
			/// Action
			/// </summary>
			GetOrSetControlCallback = 1,

			/// <summary>
			/// string
			/// </summary>
			Name = 2,

			/// <summary>
			/// bool
			/// </summary>
			Enabled = 3,

			/// <summary>
			/// in: Func{ToolTipMembers}
			/// </summary>
			ToolTip = 4,

			/// <summary>
			/// T
			/// </summary>
			Value = 8,

			/// <summary>
			/// Func{T}
			/// </summary>
			ValueGetter = 9,
		}

		/// <summary>
		/// Internal interface for clickable terminal controls implemented in the client and master modules.
		/// </summary>
		/// <exclude/>
		public interface ITerminalControl
		{
			/// <summary>
			/// Raised whenever the control's value is changed.
			/// </summary>
			event EventHandler ControlChanged;

			/// <summary>
			/// Control change callback initializer property
			/// </summary>
			EventHandler ControlChangedHandler { set; }

			/// <summary>
			/// Name of the control as it appears in the menu.
			/// </summary>
			string Name { get; set; }

			/// <summary>
			/// Determines whether or not the control will be visible in the menu.
			/// </summary>
			bool Enabled { get; set; }

			/// <summary>
			/// Optional tooltip for the control
			/// </summary>
			ToolTip ToolTip { get; set; }

			/// <summary>
			/// Unique identifer.
			/// </summary>
			object ID { get; }

			/// <summary>
			/// Retrieves data used by the Framework API
			/// </summary>
			ControlMembers GetApiData();
		}

		/// <summary>
		/// Internal interface for terminal controls associated with a value of a given type.
		/// Implemented by shared and master modules.
		/// </summary>
		/// <exclude/>
		public interface ITerminalValue<TValue> : ITerminalControl
		{
			/// <summary>
			/// Value associated with the control.
			/// </summary>
			TValue Value { get; set; }

			/// <summary>
			/// Delegate used to periodically refresh the control's value. Optional.
			/// </summary>
			Func<TValue> CustomValueGetter { get; set; }
		}
	}
}