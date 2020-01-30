using System;
using VRage;

namespace RichHudFramework
{
    namespace UI
    {
        public interface IClickableElement : IHudElement
        {
            /// <summary>
            /// Invoked when the cursor enters the element's bounds
            /// </summary>
            event Action OnCursorEnter;

            /// <summary>
            /// Invoked when the cursor leaves the element's bounds
            /// </summary>
            event Action OnCursorExit;

            /// <summary>
            /// Invoked when the element is clicked with the left mouse button
            /// </summary>
            event Action OnLeftClick;

            /// <summary>
            /// Invoked when the left click is released
            /// </summary>
            event Action OnLeftRelease;

            /// <summary>
            /// Invoked when the element is clicked with the right mouse button
            /// </summary>
            event Action OnRightClick;

            /// <summary>
            /// Invoked when the right click is released
            /// </summary>
            event Action OnRightRelease;

            /// <summary>
            /// True if the element is being clicked with the left mouse button
            /// </summary>
            bool IsLeftClicked { get; }

            /// <summary>
            /// True if the element is being clicked with the right mouse button
            /// </summary>
            bool IsRightClicked { get; }
        }
    }
}