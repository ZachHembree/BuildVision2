using System;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Creates a clickable box. Doesn't render any textures or text. Must be used in conjunction with other elements.
    /// </summary>
    public class ClickableElement : HudElementBase, IClickableElement
    {
        /// <summary>
        /// Invoked when the cursor enters the element's bounds
        /// </summary>
        public event Action OnCursorEnter;

        /// <summary>
        /// Invoked when the cursor leaves the element's bounds
        /// </summary>
        public event Action OnCursorExit;

        /// <summary>
        /// Invoked when the element is clicked with the left mouse button
        /// </summary>
        public event Action OnLeftClick;

        /// <summary>
        /// Invoked when the left click is released
        /// </summary>
        public event Action OnLeftRelease;

        /// <summary>
        /// Invoked when the element is clicked with the right mouse button
        /// </summary>
        public event Action OnRightClick;

        /// <summary>
        /// Invoked when the right click is released
        /// </summary>
        public event Action OnRightRelease;

        /// <summary>
        /// Indicates whether or not this element is currently selected.
        /// </summary>
        public bool HasFocus { get { return hasFocus && Visible; } private set { hasFocus = value; } }

        /// <summary>
        /// True if the element is being clicked with the left mouse button
        /// </summary>
        public bool IsLeftClicked { get; private set; }

        /// <summary>
        /// True if the element is being clicked with the right mouse button
        /// </summary>
        public bool IsRightClicked { get; private set; }

        private bool mouseCursorEntered;
        private bool hasFocus;

        public ClickableElement(IHudParent parent = null) : base(parent)
        {
            CaptureCursor = true;
            HasFocus = false;
        }

        protected override void HandleInput()
        {
            if (IsMousedOver)
            {
                if (!mouseCursorEntered)
                {
                    mouseCursorEntered = true;
                    OnCursorEnter?.Invoke();
                }

                if (SharedBinds.LeftButton.IsNewPressed)
                {
                    OnLeftClick?.Invoke();
                    HasFocus = true;
                    IsLeftClicked = true;
                }

                if (SharedBinds.RightButton.IsNewPressed)
                {
                    OnRightClick?.Invoke();
                    HasFocus = true;
                    IsRightClicked = true;
                }                
            }
            else
            {
                if (mouseCursorEntered)
                {
                    mouseCursorEntered = false;
                    OnCursorExit?.Invoke();
                }

                if (HasFocus && (SharedBinds.LeftButton.IsNewPressed || SharedBinds.RightButton.IsNewPressed))
                    HasFocus = false;
            }

            if (!SharedBinds.LeftButton.IsPressed && IsLeftClicked)
            {
                OnLeftRelease?.Invoke();
                IsLeftClicked = false;
            }

            if (!SharedBinds.RightButton.IsPressed && IsRightClicked)
            {
                OnRightRelease?.Invoke();
                IsRightClicked = false;
            }
        }
    }
}