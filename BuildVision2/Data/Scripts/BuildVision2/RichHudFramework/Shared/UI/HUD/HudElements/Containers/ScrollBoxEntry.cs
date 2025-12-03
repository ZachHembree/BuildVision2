namespace RichHudFramework.UI
{
	/// <summary>
	/// Base container class for ScrollBox members. 
	/// <para>Adds an <see cref="Enabled"/> state to the standard <see cref="HudElementContainer{TElement}"/>.</para>
	/// </summary>
	/// <typeparam name="TElement">UI element type used for the entry</typeparam>
	public class ScrollBoxEntry<TElement> : HudElementContainer<TElement>, IScrollBoxEntry<TElement>
        where TElement : HudElementBase
    {
		/// <summary>
		/// Determines if the entry is enabled. Disabled entries are hidden from lists.
		/// </summary>
		public virtual bool Enabled { get; set; }

        public ScrollBoxEntry() { Enabled = true; }
    }

    /// <summary>
    /// Base container class for ScrollBox members using <see cref="HudElementBase"/>.
    /// </summary>
    public class ScrollBoxEntry : ScrollBoxEntry<HudElementBase>
    { }
}