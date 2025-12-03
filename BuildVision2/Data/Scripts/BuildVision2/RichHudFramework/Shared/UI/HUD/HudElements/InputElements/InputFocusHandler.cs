using System;

namespace RichHudFramework
{
	namespace UI
	{
		using Client;
		using Server;

		/// <summary>
		/// Default implementation of <see cref="IFocusHandler"/>. Handles acquiring and releasing global input focus
		/// through <see cref="HudMain"/> and raises the appropriate events.
		/// </summary>
		public class InputFocusHandler : IFocusHandler
		{
			/// <summary>
			/// The UI element that owns this handler. Used as the sender for all focus-related events.
			/// </summary>
			public IFocusableElement InputOwner { get; set; }

			/// <summary>
			/// True if this element currently holds global input focus.
			/// </summary>
			public bool HasFocus { get; private set; }

			/// <summary>
			/// Invoked when taking focus
			/// </summary>
			public event EventHandler GainedInputFocus;

			/// <summary>
			/// Invoked when focus is lost
			/// </summary>
			public event EventHandler LostInputFocus;

			/// <summary>
			/// Invoked when this UI element gains input focus. Event initializer.
			/// </summary>
			public EventHandler GainedInputFocusCallback { set { GainedInputFocus += value; } }

			/// <summary>
			/// Invoked when this UI element loses input focus. Event initializer.
			/// </summary>
			public EventHandler LostInputFocusCallback { set { LostInputFocus += value; } }

			public InputFocusHandler(IFocusableElement inputOwner)
			{
				InputOwner = inputOwner;
			}

			/// <summary>
			/// Requests global input focus for the owning element. Automatically called on mouse click for most elements.
			/// Does nothing if already focused.
			/// </summary>
			public virtual void GetInputFocus()
			{
				if (!HasFocus)
				{
					HudMain.GetInputFocus(this);
					HasFocus = true;
					GainedInputFocus?.Invoke(InputOwner, EventArgs.Empty);
				}
			}

			/// <summary>
			/// Releases global input focus from this element. Called automatically when clicking outside a focused element.
			/// </summary>
			public virtual void ReleaseFocus()
			{
				if (HasFocus)
				{
					HasFocus = false;
					LostInputFocus?.Invoke(InputOwner, EventArgs.Empty);
				}
			}
		}
	}
}