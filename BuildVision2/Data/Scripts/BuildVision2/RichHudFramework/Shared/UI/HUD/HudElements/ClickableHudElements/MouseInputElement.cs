using System;
using VRage;
using VRageMath;
using HudSpaceDelegate = System.Func<VRage.MyTuple<bool, float, VRageMath.MatrixD>>;

namespace RichHudFramework.UI
{
    using Client;
    using Server;

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

        /// <summary>
        /// True if the element was just with the left mouse button
        /// </summary>
        public bool IsNewLeftClicked { get; private set; }

        /// <summary>
        /// True if the element was just with the right mouse button
        /// </summary>
        public bool IsNewRightClicked { get; private set; }

        private bool mouseCursorEntered;
        private bool hasFocus;

        public MouseInputElement(HudParentBase parent) : base(parent)
        {
            UseCursor = true;
            ShareCursor = true;
            HasFocus = false;
            DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding;
        }

        public MouseInputElement() : this(null)
        { }

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

        protected override void InputDepth()
        {
            if (Visible)
            {
                if (UseCursor)
                {
                    Vector3 cursorPos = HudSpace.CursorPos;
                    Vector2 offset = Vector2.Max(cachedSize, new Vector2(minMouseBounds)) / 2f;
                    BoundingBox2 box = new BoundingBox2(cachedPosition - offset, cachedPosition + offset);
                    mouseInBounds = box.Contains(new Vector2(cursorPos.X, cursorPos.Y)) == ContainmentType.Contains
                        || (IsLeftClicked || IsRightClicked);

                    if (mouseInBounds)
                        HudMain.Cursor.TryCaptureHudSpace(cursorPos.Z, HudSpace.GetHudSpaceFunc);
                }
            }
        }

        protected override void BeginInput()
        {
            if (Visible)
            {
                Vector3 cursorPos = HudSpace.CursorPos;

                if (UseCursor && mouseInBounds && !HudMain.Cursor.IsCaptured && HudMain.Cursor.IsCapturingSpace(HudSpace.GetHudSpaceFunc))
                {
                    _isMousedOver = mouseInBounds;

                    HandleInput(new Vector2(cursorPos.X, cursorPos.Y));

                    if (!ShareCursor)
                        HudMain.Cursor.Capture(GetOrSetMemberFunc);
                }
                else
                {
                    _isMousedOver = false;
                    HandleInput(new Vector2(cursorPos.X, cursorPos.Y));
                }
            }
            else
                _isMousedOver = false;
        }

        protected override void HandleInput(Vector2 cursorPos)
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
                    IsNewLeftClicked = true;
                }
                else
                    IsNewLeftClicked = false;

                if (SharedBinds.RightButton.IsNewPressed)
                {
                    OnRightClick?.Invoke(_parent, EventArgs.Empty);
                    HasFocus = true;
                    IsRightClicked = true;
                    IsNewRightClicked = true;
                }
                else
                    IsNewRightClicked = false;
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

                IsNewLeftClicked = false;
                IsNewRightClicked = false;
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