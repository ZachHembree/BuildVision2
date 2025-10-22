namespace RichHudFramework.UI
{
    /// <summary>
    /// Base container class for <see cref="HudChain"/> members. Can be extended to associate data with chain
    /// elements.
    /// </summary>
    public class HudNodeContainer<TElement> : IHudElementContainer<TElement> where TElement : HudNodeBase
    {
        public virtual TElement Element { get; protected set; }

        public HudNodeContainer()
        { }

        public virtual void SetElement(TElement element)
        {
            if (Element == null)
                Element = element;
            else
                throw new System.Exception("Only one element can ever be associated with a container object.");
        }
    }

    /// <summary>
    /// Base container class for <see cref="HudChain"/> members. Can be extended to associate data with chain
    /// elements.
    /// </summary>
    public class HudElementContainer<TElement> : IChainElementContainer<TElement> where TElement : HudElementBase
    {
        public virtual TElement Element { get; protected set; }

        public float AlignAxisScale { get; set; }

        public HudElementContainer()
        {
            AlignAxisScale = 0f;
        }

        public virtual void SetElement(TElement element)
        {
            if (Element == null)
                Element = element;
            else
                throw new System.Exception("Only one element can ever be associated with a container object.");
        }
    }

    /// <summary>
    /// Base container class for <see cref="HudChain"/> members. Can be extended to associate data with chain
    /// elements.
    /// </summary>
    public class HudElementContainer : HudElementContainer<HudElementBase>
    { }
}
