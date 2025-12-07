namespace RichHudFramework.UI
{
    /// <summary>
    /// Minimal interface for UI elements controlling a value that invokes an event when changed.
    /// </summary>
    public interface IValueControl
    {
        /// <summary>
        /// Event invoked when the value changes.
        /// </summary>
        event EventHandler ValueChanged;

        /// <summary>
        /// Utility property for registering a value update callback via object initializers.
        /// </summary>
        EventHandler UpdateValueCallback { set; }
    }

    /// <summary>
    /// Minimal interface for UI elements controlling a value that invokes an event when changed, with 
    /// a property for reading that value.
    /// </summary>
    public interface IValueControl<T>
    {
        /// <summary>
        /// Current value of the control.
        /// </summary>
        T Value { get; }
    }
}