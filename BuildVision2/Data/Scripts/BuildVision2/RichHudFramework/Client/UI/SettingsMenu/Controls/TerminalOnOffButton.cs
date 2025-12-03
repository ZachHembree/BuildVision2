namespace RichHudFramework.UI.Client
{
	/// <summary>
	/// An On/Off toggle button used for <see cref="ControlTile"/>s.
	/// <para>Mimics the appearance of the "On/Off" button in the SE Terminal.</para>
	/// </summary>
	public class TerminalOnOffButton : TerminalValue<bool>
	{
		public TerminalOnOffButton() : base(MenuControls.OnOffButton)
		{ }
	}
}