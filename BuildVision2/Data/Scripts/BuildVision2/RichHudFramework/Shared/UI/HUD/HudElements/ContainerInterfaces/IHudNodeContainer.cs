namespace RichHudFramework.UI
{
	/// <summary>
	/// Interface for objects acting as decorator wrappers for UI elements.
	/// <para>Separates the UI element from metadata.</para>
	/// </summary>
	public interface IHudNodeContainer<TElement> where TElement : HudNodeBase
	{
		/// <summary>
		/// The HUD Element associated with this container.
		/// </summary>
		TElement Element { get; }

		/// <summary>
		/// Sets the element associated with the container. 
		/// <para>This method should only allow a single assignment to ensure immutability of the link.</para>
		/// </summary>
		void SetElement(TElement Element);
	}

	/// <summary>
	/// Interface for objects containing UI elements compatible with <see cref="HudChain{TElementContainer, TElement}"/>.
	/// <para>Provides layout weighting information required for proportional sizing.</para>
	/// </summary>
	public interface IChainElementContainer<TElement> : IHudNodeContainer<TElement>
        where TElement : HudElementBase
    {
        /// <summary>
        /// Determines how excess space in the <see cref="HudChain"/> is distributed along the alignment axis.
        /// <para>0f = The element retains its fixed size (default behavior).</para>
        /// <para>> 0f = The element scales proportionally relative to the sum of all weighted elements to fill remaining space.</para>
        /// <para>Note: This value is ignored if the Chain is using <see cref="HudChainSizingModes.FitMembersAlignAxis"/>.</para>
        /// </summary>
        float AlignAxisScale { get; set; }
    }
}
