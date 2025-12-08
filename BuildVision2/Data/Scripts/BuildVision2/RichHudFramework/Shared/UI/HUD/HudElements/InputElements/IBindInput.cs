using System;
using System.Collections.Generic;

namespace RichHudFramework.UI
{
    using Client;
    using Server;

    /// <summary>
    /// Provides per-bind event proxies (NewPressed / PressedAndHeld / Released) for custom control bindings
    /// attached to a UI element.
    /// </summary>
    public interface IBindEventProxy
	{
		/// <summary>
		/// Invoked immediately when the bound input is first pressed
		/// </summary>
		event EventHandler NewPressed;

		/// <summary>
		/// Invoked after the bound input has been held and pressed for at least 500ms
		/// </summary>
		event EventHandler PressedAndHeld;

		/// <summary>
		/// Invoked immediately after the bound input is released
		/// </summary>
		event EventHandler Released;
	}

	/// <summary>
	/// Allows a UI element to respond to arbitrary custom control binds (<see cref="IBind"/>).
	/// </summary>
	public interface IBindInput : IFocusableElement, IEnumerable<IBindEventProxy>
	{
		/// <summary>
		/// Retrieves the event proxy (press/release events) for a specific bind on this UI element
		/// </summary>
		IBindEventProxy this[IBind bind] { get; }

		/// <summary>
		/// Adds a new bind to the input element if it hasn't been added before, and/or 
		/// registers the given event handlers to it.
		/// </summary>
		void Add(IBind bind, EventHandler NewPressed = null, EventHandler PressedAndHeld = null, EventHandler Released = null);

		/// <summary>
		/// Returns true if the given bind is actively used and handled by this element
		/// </summary>
		bool GetHasBind(IBind bind);

        /// <summary>
        /// If true, bind events will only fire when the parent element has input focus.
        /// Default = false.
        /// <para>Only applies if the parent/InputOwner implements <see cref="IFocusableElement"/>.</para>
        /// </summary>
        bool IsFocusRequired { get; set; }

        /// <summary>
        /// If set, the input groups indicated by the flags will be temporarily blocked 
        /// until the BintInputElement is disabled. 
        /// <para>Uses <see cref="BindManager.RequestTempBlacklist(SeBlacklistModes)"/>.</para>
        /// </summary>
        SeBlacklistModes InputFilter { get; set; }
    }

	/// <summary>
	/// Marks a UI element as supporting custom bind input via an <see cref="IBindInput"/> instance.
	/// </summary>
	public interface IBindInputElement : IFocusableElement
	{
		/// <summary>
		/// Custom bind input interface for this element
		/// </summary>
		IBindInput BindInput { get; }
	}
}