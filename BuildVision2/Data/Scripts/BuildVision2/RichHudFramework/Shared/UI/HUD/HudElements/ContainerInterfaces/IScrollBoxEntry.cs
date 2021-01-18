namespace RichHudFramework.UI
{
    /// <summary>
    /// Interface for scrollbox entry containers.
    /// </summary>
    public interface IScrollBoxEntry<TElement> : IHudElementContainer<TElement> where TElement : HudElementBase
    {
        bool Enabled { get; set; }
    }
}