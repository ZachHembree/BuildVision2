namespace RichHudFramework
{
	namespace UI
	{
		/// <summary>
		/// Manages keyboard/input focus for a UI element. Only one element in the entire UI can have focus at a time.
		/// Input-related events use <see cref="InputOwner"/> as the sender object.
		/// </summary>
		public interface IFocusHandler
		{
			/// <summary>
			/// UI element that owns this focus handler, used in event callbacks
			/// </summary>
			IFocusableElement InputOwner { get; set; }

			/// <summary>
			/// Invoked when this UI element gains input focus
			/// </summary>
			event EventHandler GainedInputFocus;

			/// <summary>
			/// Invoked when this UI element loses input focus
			/// </summary>
			event EventHandler LostInputFocus;

			/// <summary>
			/// Invoked when this UI element gains input focus. Event initializer.
			/// </summary>
			EventHandler GainedInputFocusCallback { set; }

			/// <summary>
			/// Invoked when this UI element loses input focus. Event initializer.
			/// </summary>
			EventHandler LostInputFocusCallback { set; }

			/// <summary>
			/// Returns true if the UI element currently has input focus
			/// </summary>
			bool HasFocus { get; }

			/// <summary>
			/// Manually requests and takes input focus for this element
			/// </summary>
			void GetInputFocus();

			/// <summary>
			/// Releases the input focus from this element
			/// </summary>
			void ReleaseFocus();
		}

		/// <summary>
		/// Represents a UI element that can receive keyboard/input focus.
		/// </summary>
		public interface IFocusableElement
		{
			/// <summary>
			/// Interface used to manage the element's input focus state
			/// </summary>
			IFocusHandler FocusHandler { get; }
		}
	}
}