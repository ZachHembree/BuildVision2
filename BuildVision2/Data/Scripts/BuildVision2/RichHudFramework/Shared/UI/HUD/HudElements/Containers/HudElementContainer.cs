namespace RichHudFramework.UI
{
    /// <summary>
    /// Base container class for <see cref="HudChain"/> members. Can be extended to associate data with chain
    /// elements.
    /// </summary>
    public class HudElementContainer<TElement> : IHudElementContainer<TElement> where TElement : HudElementBase
    {
        public virtual TElement Element { get; set; }

        public HudElementContainer()
        { }
    }

    /// <summary>
    /// Base container class for <see cref="HudChain"/> members. Can be extended to associate data with chain
    /// elements.
    /// </summary>
    public class HudElementContainer : HudElementContainer<HudElementBase>
    { }
}
