namespace RichHudFramework
{
    namespace UI
    {
        using Client;
        using Server;

        /// <summary>
        /// Low-level mouse input handler providing cursor enter/exit detection and click events.
        /// Events are raised using the owning <see cref="IFocusHandler.InputOwner"/> as the sender.
        /// </summary>
        public interface IMouseInput : IFocusableElement
        {
            /// <summary>
            /// Invoked when the mouse cursor enters the element's interactive area.
            /// </summary>
            event EventHandler CursorEntered;

            /// <summary>
            /// Invoked when the mouse cursor leaves the element's interactive area.
            /// </summary>
            event EventHandler CursorExited;

            /// <summary>
            /// Invoked when the element is clicked with the left mouse button.
            /// </summary>
            event EventHandler LeftClicked;

            /// <summary>
            /// Invoked when the left mouse button is released over the element.
            /// </summary>
            event EventHandler LeftReleased;

            /// <summary>
            /// Invoked when the element is clicked with the right mouse button.
            /// </summary>
            event EventHandler RightClicked;

            /// <summary>
            /// Invoked when the right mouse button is released over the element.
            /// </summary>
            event EventHandler RightReleased;

            /// <summary>
            /// Invoked when the mouse cursor enters the element's interactive area. Event initializer.
            /// </summary>
            EventHandler CursorEnteredCallback { set; }

            /// <summary>
            /// Invoked when the mouse cursor leaves the element's interactive area. Event initializer.
            /// </summary>
            EventHandler CursorExitedCallback { set; }

            /// <summary>
            /// Invoked when the element is clicked with the left mouse button. Event initializer.
            /// </summary>
            EventHandler LeftClickedCallback { set; }

            /// <summary>
            /// Invoked when the left mouse button is released over the element. Event initializer.
            /// </summary>
            EventHandler LeftReleasedCallback { set; }

            /// <summary>
            /// Invoked when the element is clicked with the right mouse button. Event initializer.
            /// </summary>
            EventHandler RightClickedCallback { set; }

            /// <summary>
            /// Invoked when the right mouse button is released over the element. Event initializer.
            /// </summary>
            EventHandler RightReleasedCallback { set; }

            /// <summary>
            /// If true, the input element will temporarily show the cursor while it's enabled.
            /// <para>Uses <see cref="HudMain.EnableCursorTemp"></see>.</para>
            /// </summary>
            bool RequestCursor { get; set; }

            /// <summary>
            /// Optional tooltip text shown when the element is moused over.
            /// </summary>
            ToolTip ToolTip { get; set; }

            /// <summary>
            /// Returns true if the element is currently being held down with the left mouse button.
            /// </summary>
            bool IsLeftClicked { get; }

            /// <summary>
            /// Returns true if the element is currently being held down with the right mouse button.
            /// </summary>
            bool IsRightClicked { get; }

            /// <summary>
            /// Returns true if the element was just clicked with the left mouse button this frame.
            /// </summary>
            bool IsNewLeftClicked { get; }

            /// <summary>
            /// Returns true if the element was just clicked with the right mouse button this frame.
            /// </summary>
            bool IsNewRightClicked { get; }

            /// <summary>
            /// Returns true if the element was just released after being left-clicked this frame.
            /// </summary>
            bool IsLeftReleased { get; }

            /// <summary>
            /// Returns true if the element was just released after being right-clicked this frame.
            /// </summary>
            bool IsRightReleased { get; }

            /// <summary>
            /// Returns true if the mouse cursor is currently over the element.
            /// </summary>
            bool IsMousedOver { get; }
        }

        /// <summary>
        /// Indicates that a UI element supports mouse interaction via an <see cref="IMouseInput"/> instance.
        /// </summary>
        public interface IClickableElement : IFocusableElement
        {
            /// <summary>
            /// Mouse input interface for this clickable element
            /// </summary>
            IMouseInput MouseInput { get; }
        }
    }
}