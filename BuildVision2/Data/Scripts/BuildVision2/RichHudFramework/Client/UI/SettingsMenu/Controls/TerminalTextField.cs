using System;

namespace RichHudFramework.UI.Client
{
	/// <summary>
	/// Internal API member accessor indices
	/// </summary>
	/// <exclude/>
	public enum TextFieldAccessors : int
	{
		CharFilterFunc = 16,
	}

	/// <summary>
	/// A single-line text input field with a configurable character filter. For <see cref="ControlTile"/>s.
	/// <para>Mimics the appearance of the text field in the SE terminal.</para>
	/// </summary>
	public class TerminalTextField : TerminalValue<string>
	{
		/// <summary>
		/// A delegate used to validate input characters. 
		/// <para>If the function returns false for a specific character, that character is rejected/ignored.</para>
		/// </summary>
		public Func<char, bool> CharFilterFunc
		{
			get { return GetOrSetMember(null, (int)TextFieldAccessors.CharFilterFunc) as Func<char, bool>; }
			set { GetOrSetMember(value, (int)TextFieldAccessors.CharFilterFunc); }
		}

		public TerminalTextField() : base(MenuControls.TextField)
		{ }
	}
}