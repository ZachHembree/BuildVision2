using System;
using System.Collections;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI
{
	using Client;
	using Server;

	/// <summary>
	/// Attaches custom control-bind (key/combo) event handling to a UI element.
	/// Allows arbitrary <see cref="IBind"/> definitions to trigger NewPressed/PressedAndHeld/Released events
	/// on a specific UI node, optionally requiring input focus.
	/// </summary>
	public class BindInputElement : HudNodeBase, IBindInput
	{
		/// <summary>
		/// Allows the addition of binds in conjunction with normal property initialization
		/// </summary>
		public IBindInput CollectionInitializer => this;

		/// <summary>
		/// Element that owns this input, used for event callbacks
		/// </summary>
		public IFocusHandler FocusHandler { get; protected set; }

		/// <summary>
		/// Retrieves the event proxy (press/release events) for a specific bind on this UI element
		/// </summary>
		public IBindEventProxy this[IBind bind] => binds[bind];

		/// <summary>
		/// If true, bind events will only fire when the parent element has input focus.
		/// Default = false.
		/// <para>Only applies if the parent/InputOwner implements <see cref="IFocusableElement"/>.</para>
		/// </summary>
		public bool IsFocusRequired { get; set; }

        /// <summary>
        /// If defined, bind events will only fire when the predicate returns true.
        /// </summary>
        public Func<bool> InputPredicate { get; set; }

        /// <summary>
        /// If set, the input groups indicated by the flags will be temporarily blocked 
        /// until the BintInputElement is disabled. 
        /// <para>Uses <see cref="BindManager.RequestTempBlacklist(SeBlacklistModes)"/>.</para>
        /// </summary>
        public SeBlacklistModes InputFilterFlags { get; set; }

		/// <summary>
		/// Internal Bind-EventProxy map
		/// </summary>
		/// <exclude/>
		protected readonly Dictionary<IBind, BindEventProxy> binds;

		public BindInputElement(HudParentBase parent = null) : base(parent)
		{
			FocusHandler = (parent as IFocusableElement)?.FocusHandler;
			IsFocusRequired = false;
			binds = new Dictionary<IBind, BindEventProxy>();
		}

		/// <summary>
		/// Adds a bind (if not already present) and/or subscribes the provided event handlers.
		/// Multiple calls with the same bind are allowed and will stack handlers.
		/// </summary>
		public void Add(IBind bind, EventHandler NewPressed = null, EventHandler PressedAndHeld = null, EventHandler Released = null)
		{
			if (!binds.ContainsKey(bind))
				binds.Add(bind, new BindEventProxy());

			if (NewPressed != null || PressedAndHeld != null | Released != null)
			{
				var proxy = binds[bind];

				if (NewPressed != null)
					proxy.NewPressed += NewPressed;

				if (PressedAndHeld != null)
					proxy.PressedAndHeld += PressedAndHeld;

				if (Released != null)
					proxy.Released += Released;
			}
		}

		/// <summary>
		/// Removes all binds from the element
		/// </summary>
		public void Reset() { binds.Clear(); }

		/// <summary>
		/// Returns true if the given bind is actively used and handled by this element
		/// </summary>
		public bool GetHasBind(IBind bind) =>
			binds.ContainsKey(bind);

		/// <summary>
		/// Polls bind input and fires events
		/// </summary>
		/// <exclude/>
		protected override void HandleInput(Vector2 cursorPos)
		{
			FocusHandler = (Parent as IFocusableElement)?.FocusHandler;

			if (IsFocusRequired && !(FocusHandler?.HasFocus ?? false))
				return;

			if (!InputPredicate?.Invoke() ?? false)
				return;

			if (InputFilterFlags != SeBlacklistModes.None)
				BindManager.RequestTempBlacklist(InputFilterFlags);

			var owner = (object)(FocusHandler?.InputOwner) ?? Parent;

			foreach (KeyValuePair<IBind, BindEventProxy> pair in binds)
			{
				if (pair.Key.IsNewPressed)
					pair.Value.InvokeNewPressed(owner, EventArgs.Empty);

				if (pair.Key.IsPressedAndHeld)
					pair.Value.InvokePressedAndHeld(owner, EventArgs.Empty);

				if (pair.Key.IsReleased)
					pair.Value.InvokeReleased(owner, EventArgs.Empty);
			}
		}

		public IEnumerator<IBindEventProxy> GetEnumerator() =>
			binds.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Provides input events for a specific custom UI binding.
		/// </summary>
		/// <exclude/>
		protected class BindEventProxy : IBindEventProxy
		{
			/// <summary>
			/// Invoked immediately when the bound input is first pressed
			/// </summary>
			public event EventHandler NewPressed;

			/// <summary>
			/// Invoked after the bound input has been held and pressed for at least 500ms
			/// </summary>
			public event EventHandler PressedAndHeld;

			/// <summary>
			/// Invoked immediately after the bound input is released
			/// </summary>
			public event EventHandler Released;

			public void InvokeNewPressed(object sender, EventArgs args) =>
				NewPressed?.Invoke(sender, args);

			public void InvokePressedAndHeld(object sender, EventArgs args) =>
				PressedAndHeld?.Invoke(sender, args);

			public void InvokeReleased(object sender, EventArgs args) =>
				Released?.Invoke(sender, args);

			public void ClearSubscribers()
			{
				NewPressed = null;
				PressedAndHeld = null;
				Released = null;
			}
		}
	}
}