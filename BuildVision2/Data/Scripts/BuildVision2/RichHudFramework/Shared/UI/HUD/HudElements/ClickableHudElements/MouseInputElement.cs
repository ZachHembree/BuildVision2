using System;

namespace RichHudFramework.UI
{
    /// <summary>
    /// A clickable box. Doesn't render any textures or text. Must be used in conjunction with other elements.
    /// Events return the parent object.
    /// </summary>
    public class MouseInputElement : HudElementBase, IMouseInput
    {
        /// <summary>
        /// Invoked when the cursor enters the element's bounds
        /// </summary>
        public event EventHandler OnCursorEnter;

        /// <summary>
        /// Invoked when the cursor leaves the element's bounds
        /// </summary>
        public event EventHandler OnCursorExit;

        /// <summary>
        /// Invoked when the element is clicked with the left mouse button
        /// </summary>
        public event EventHandler OnLeftClick;

        /// <summary>
        /// Invoked when the left click is released
        /// </summary>
        public event EventHandler OnLeftRelease;

        /// <summary>
        /// Invoked when the element is clicked with the right mouse button
        /// </summary>
        public event EventHandler OnRightClick;

        /// <summary>
        /// Invoked when the right click is released
        /// </summary>
        public event EventHandler OnRightRelease;

        /// <summary>
        /// Indicates whether or not the cursor is currently over this element.
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

        public MouseInputElement(HudParentBase parent = null) : base(parent)
        {
            UseCursor = true;
            HasFocus = false;
            DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding;
        }

        /// <summary>
        /// Clears all subscribers to mouse input events.
        /// </summary>
        public void ClearSubscribers()
        {
            OnCursorEnter = null;
            OnCursorExit = null;
            OnLeftClick = null;
            OnLeftRelease = null;
            OnRightClick = null;
            OnRightRelease = null;
        }

        protected override void HandleInput()
        {
            if (IsMousedOver)
            {
                if (!mouseCursorEntered)
                {
                    mouseCursorEntered = true;
                    OnCursorEnter?.Invoke(_parent, EventArgs.Empty);
                }

                if (SharedBinds.LeftButton.IsNewPressed)
                {
                    OnLeftClick?.Invoke(_parent, EventArgs.Empty);
                    HasFocus = true;
                    IsLeftClicked = true;
                }

                if (SharedBinds.RightButton.IsNewPressed)
                {
                    OnRightClick?.Invoke(_parent, EventArgs.Empty);
                    HasFocus = true;
                    IsRightClicked = true;
                }                
            }
            else
            {
                if (mouseCursorEntered)
                {
                    mouseCursorEntered = false;
                    OnCursorExit?.Invoke(_parent, EventArgs.Empty);
                }

                if (HasFocus && (SharedBinds.LeftButton.IsNewPressed || SharedBinds.RightButton.IsNewPressed))
                    HasFocus = false;
            }

            if (!SharedBinds.LeftButton.IsPressed && IsLeftClicked)
            {
                OnLeftRelease?.Invoke(_parent, EventArgs.Empty);
                IsLeftClicked = false;
            }

            if (!SharedBinds.RightButton.IsPressed && IsRightClicked)
            {
                OnRightRelease?.Invoke(_parent, EventArgs.Empty);
                IsRightClicked = false;
            }
        }
    }
}