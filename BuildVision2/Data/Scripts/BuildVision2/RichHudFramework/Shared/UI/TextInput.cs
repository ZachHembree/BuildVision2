using Sandbox.ModAPI;
using System;
using System.Collections.Generic;

namespace RichHudFramework.UI
{
	/// <summary>
	/// Provides a simple mechanism for handling text input, integrating with the game's input system
	/// to process characters and backspace events.
	/// </summary>
	public class TextInput
	{
		private readonly Func<char, bool> IsCharAllowedFunc;
		private readonly Action<char> AppendAction;
		private readonly Action BackspaceAction;

		/// <summary>
		/// Initializes a new instance of the <see cref="TextInput"/> class.
		/// </summary>
		/// <param name="AppendAction">The action to be executed for each allowed character typed by the user.</param>
		/// <param name="BackspaceAction">The action to be executed when the backspace key is pressed.</param>
		/// <param name="IsCharAllowedFunc">An optional function used to validate characters before appending. Pass null if all characters are permitted.</param>
		public TextInput(Action<char> AppendAction, Action BackspaceAction, Func<char, bool> IsCharAllowedFunc = null)
		{
			this.AppendAction = AppendAction;
			this.BackspaceAction = BackspaceAction;
			this.IsCharAllowedFunc = IsCharAllowedFunc;
		}

		/// <summary>
		/// Processes the current frame's input and invokes the registered actions for backspace 
		/// and character appending. This method should be called once per game frame.
		/// </summary>
		public void HandleInput()
		{
			IReadOnlyList<char> input = MyAPIGateway.Input.TextInput;

			// Handle Backspace Manually: Checks for both initial press and holding for rapid deletion
			// You never know when you might disagree with however many backspaces happen to be buffered
			if (SharedBinds.Back.IsPressedAndHeld || SharedBinds.Back.IsNewPressed)
				BackspaceAction?.Invoke();

			// Handle Character Append
			for (int n = 0; n < input.Count; n++)
			{
				// Check that the character isn't a backspace and passes the optional filter
				if (input[n] != '\b' && (IsCharAllowedFunc == null || IsCharAllowedFunc(input[n])))
					AppendAction?.Invoke(input[n]);
			}
		}
	}
}