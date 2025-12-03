using VRageMath;

namespace RichHudFramework.UI.Client
{
	/// <summary>
	/// An RGB color picker using three sliders (Red, Green, Blue) for <see cref="ControlTile"/>s.
	/// <para>Designed to mimic the appearance of the color picker in the SE terminal.</para>
	/// </summary>
	public class TerminalColorPicker : TerminalValue<Color>
	{
		public TerminalColorPicker() : base(MenuControls.ColorPicker)
		{ }
	}
}