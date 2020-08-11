namespace RichHudFramework.UI
{
    /// <summary>
    /// Basic container class used to associate a scrollbox element with an arbitrary object
    /// of type TData.
    /// </summary>
    public class ScrollBoxEntryTuple<TElement, TData> : ScrollBoxEntry<TElement> where TElement : HudElementBase
    {
        public virtual TData AssocData { get; set; }

        public ScrollBoxEntryTuple()
        { }
    }

    /// <summary>
    /// Basic container class used to associate a scrollbox element with an arbitrary object
    /// of type TData.
    /// </summary>
    public class ScrollBoxEntryTuple<TData> : ScrollBoxEntryTuple<HudElementBase, TData> 
    { }
}