namespace RichHudFramework.UI
{
	/// <summary>
	/// Container class used to associate a <see cref="HudChain"/> entry with an arbitrary data object.
	/// </summary>
	/// <typeparam name="TElement">UI element type used for the entry</typeparam>
	/// <typeparam name="TData">Data type associated with the entry</typeparam>
	public class HudElementTuple<TElement, TData> : HudElementContainer<TElement> where TElement : HudElementBase
	{
		/// <summary>
		/// The arbitrary data associated with this element.
		/// </summary>
		public virtual TData AssocData { get; set; }

		public HudElementTuple()
		{ }
	}

	/// <summary>
	/// Container class used to associate a base <see cref="HudElementBase"/> with an arbitrary data object.
	/// </summary>
	/// <typeparam name="TData">Data type associated with the entry</typeparam>
	public class HudElementTuple<TData> : HudElementTuple<HudElementBase, TData>
	{ }
}
