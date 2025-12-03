namespace RichHudFramework.UI.Client
{
	/// <summary>
	/// A boolean checkbox control for a <see cref="ControlTile"/>.
	/// <para>Mimics the appearance of the standard Space Engineers terminal checkbox.</para>
	/// </summary>
	public class TerminalCheckbox : TerminalValue<bool>
	{
		public TerminalCheckbox() : base(MenuControls.Checkbox)
		{ }
	}
}