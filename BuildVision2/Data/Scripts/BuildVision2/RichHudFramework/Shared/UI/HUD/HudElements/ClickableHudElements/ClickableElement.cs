using System;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Creates a clickable box. Doesn't render any textures or text. Must be used in conjunction with other elements.
    /// </summary>
    public class ClickableElement : HudElementBase, IClickableElement
    {
        public event Action OnLeftClick, OnRightClick, OnLeftRelease, OnRightRelease, OnCursorEnter, OnCursorExit;

        /// <summary>
        /// Indicates whether or not this element is currently selected.
        /// </summary>
        public bool HasFocus { get { return hasFocus && Visible; } private set { hasFocus = value; } }

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
                }
                else if (SharedBinds.RightButton.IsNewPressed)
                {
                    OnRightClick?.Invoke();
                    HasFocus = true;
                }
                else if (SharedBinds.LeftButton.IsReleased)
                    OnLeftRelease?.Invoke();
                else if (SharedBinds.RightButton.IsReleased)
                    OnRightRelease?.Invoke();
            }
            else
            {
                if (mouseCursorEntered)
                {
                    mouseCursorEntered = false;
                    OnCursorExit?.Invoke();

                    if (SharedBinds.LeftButton.IsPressed)
                        OnLeftRelease?.Invoke();
                    else if (SharedBinds.RightButton.IsPressed)
                        OnRightRelease?.Invoke();
                }

                if (HasFocus && (SharedBinds.LeftButton.IsNewPressed || SharedBinds.RightButton.IsNewPressed))
                {
                    HasFocus = false;
                }
            }
        }
    }
}