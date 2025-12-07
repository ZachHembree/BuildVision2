namespace RichHudFramework.UI
{
	/// <summary>
	/// A bare HUD element that renders nothing, does no input handling.
	/// <para>
	/// Useful for organizing or grouping UI elements in an area using 
	/// <see cref="HudElementBase.DimAlignment"/> and <see cref="HudElementBase.ParentAlignment"/>
	/// </para>
	/// </summary>
	public class EmptyHudElement : HudElementBase
    {
        public EmptyHudElement(HudParentBase parent) : base(parent)
        { }

        public EmptyHudElement() : this(null)
        { }
    }
}