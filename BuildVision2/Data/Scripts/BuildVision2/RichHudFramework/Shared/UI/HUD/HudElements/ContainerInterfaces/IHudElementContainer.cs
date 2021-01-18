namespace RichHudFramework.UI
{
    /// <summary>
    /// Interface for objects acting as containers of UI elements
    /// </summary>
    public interface IHudElementContainer<TElement> where TElement : HudElementBase
    {
        TElement Element { get; set; }
    }
}
