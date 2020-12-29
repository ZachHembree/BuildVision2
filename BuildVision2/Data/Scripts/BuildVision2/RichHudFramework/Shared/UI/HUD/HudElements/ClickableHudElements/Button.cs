using VRageMath;
using System;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Clickable button with a textured background.
    /// </summary>
    public class Button : TexturedBox, IClickableElement
    {
        /// <summary>
        /// Indicates whether or not the cursor is currently positioned over the button.
        /// </summary>
        public override bool IsMousedOver => _mouseInput.IsMousedOver;

        /// <summary>
        /// Handles mouse input for the button.
        /// </summary>
        public IMouseInput MouseInput => _mouseInput;

        /// <summary>
        /// Determines whether or not the button will highlight when moused over.
        /// </summary>
        public bool highlightEnabled;

        /// <summary>
        /// Color of the background when moused over.
        /// </summary>
        public Color highlightColor;

        private readonly MouseInputElement _mouseInput;
        private Color oldColor;

        public Button(HudParentBase parent) : base(parent)
        {
            _mouseInput = new MouseInputElement(this);
            highlightColor = new Color(255, 255, 255, 125);
            highlightEnabled = true;

            _mouseInput.OnCursorEnter += CursorEntered;
            _mouseInput.OnCursorExit += CursorExited;
        }

        public Button() : this(null)
        { }

        protected void CursorEntered(object sender, EventArgs args)
        {
            if (highlightEnabled)
            {
                oldColor = Color;
                Color = highlightColor;
            }
        }

        protected void CursorExited(object sender, EventArgs args)
        {
            if (highlightEnabled)
            {
                Color = oldColor;
            }
        }
    }
}