using System;
using VRage;

namespace RichHudFramework
{
    public delegate void EventHandler(object sender, EventArgs e);

    namespace UI
    {
        /// <summary>
        /// Interface for mouse input of a UI element.
        /// </summary>
        public interface IMouseInput
        {
            /// <summary>
            /// Invoked when the cursor enters the element's bounds
            /// </summary>
            event EventHandler OnCursorEnter;

            /// <summary>
            /// Invoked when the cursor leaves the element's bounds
            /// </summary>
            event EventHandler OnCursorExit;

            /// <summary>
            /// Invoked when the element is clicked with the left mouse button
            /// </summary>
            event EventHandler OnLeftClick;

            /// <summary>
            /// Invoked when the left click is released
            /// </summary>
            event EventHandler OnLeftRelease;

            /// <summary>
            /// Invoked when the element is clicked with the right mouse button
            /// </summary>
            event EventHandler OnRightClick;

            /// <summary>
            /// Invoked when the right click is released
            /// </summary>
            event EventHandler OnRightRelease;

            /// <summary>
            /// True if the element is being clicked with the left mouse button
            /// </summary>
            bool IsLeftClicked { get; }

            /// <summary>
            /// True if the element is being clicked with the right mouse button
            /// </summary>
            bool IsRightClicked { get; }

            /// <summary>
            /// Indicates whether or not the cursor is currently over this element.
            /// </summary>
            bool HasFocus { get; }

            /// <summary>
            /// Clears all subscribers to mouse input events.
            /// </summary>
            void ClearSubscribers();
        }

        public interface IClickableElement : IReadOnlyHudElement
        {
            IMouseInput MouseInput { get; }
        }
    }
}