namespace RichHudFramework.UI
{
    /// <summary>
    /// Interface for objects acting as containers of UI elements
    /// </summary>
    public interface IHudElementContainer<TElement> where TElement : HudNodeBase
    {
        /// <summary>
        /// HUD Element associated with the container
        /// </summary>
        TElement Element { get; }

        /// <summary>
        /// Sets the element associated with the container. Should only
        /// allow one assignment.
        /// </summary>
        void SetElement(TElement Element);
    }

    /// <summary>
    /// Interface for objects containing UI elements compatible with HudChain
    /// </summary>
    public interface IChainElementContainer<TElement> : IHudElementContainer<TElement> 
        where TElement : HudElementBase
    {
		/// <summary>
		/// Scale of the UI element relative to the chain. Normalized to sum of all members
		/// scales. Can be overridden by Fit/ClampMember sizing modes.
        /// 
		/// 0f = constant (no scaling); 1f = auto
		/// </summary>
		float AlignAxisScale { get; set; }
    }
}
