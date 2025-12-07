namespace RichHudFramework.UI
{
    /// <summary>
    /// Base container class for generic <see cref="HudCollection{TElementContainer, TElement}"/> members. 
    /// <para>Can be extended to associate data with arbitrary HUD nodes.</para>
    /// </summary>
    /// <typeparam name="TElement">UI element type used for the entry</typeparam>
    public class HudNodeContainer<TElement> : IHudNodeContainer<TElement> where TElement : HudNodeBase
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
    /// Standard container class for <see cref="HudCollection{TElementContainer, TElement}"/> members 
	/// using the base <see cref="HudNodeBase"/>.
    /// </summary>
    public class HudNodeContainer : HudNodeContainer<HudNodeBase>
    { }

    /// <summary>
    /// Standard container class for <see cref="HudChain{TElementContainer, TElement}"/>  members. 
    /// </summary>
    /// <typeparam name="TElement">UI element type used for the entry</typeparam>
    public class HudElementContainer<TElement> : IChainElementContainer<TElement> where TElement : HudElementBase
	{
		public virtual TElement Element { get; protected set; }

		/// <inheritdoc/>
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
	/// Standard container class for <see cref="HudChain{TElementContainer, TElement}"/> 
	/// members using the base <see cref="HudElementBase"/>.
	/// </summary>
	public class HudElementContainer : HudElementContainer<HudElementBase>
	{ }
}
