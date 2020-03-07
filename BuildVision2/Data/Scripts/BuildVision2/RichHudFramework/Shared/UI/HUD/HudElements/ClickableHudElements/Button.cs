using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Clickable button with a textured background.
    /// </summary>
    public class Button : TexturedBox, IClickableElement
    {
        /// <summary>
        /// Width of the button in pixels.
        /// </summary>
        public override float Width { get { return base.Width; } set { base.Width = value; _mouseInput.Width = value; } }

        /// <summary>
        /// Height of the button in pixels.
        /// </summary>
        public override float Height { get { return base.Height; } set { base.Height = value; _mouseInput.Height = value; } }

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

        public Button(IHudParent parent = null) : base(parent)
        {
            _mouseInput = new MouseInputElement(this);
            highlightColor = new Color(255, 255, 255, 125);
            highlightEnabled = true;

            _mouseInput.OnCursorEnter += CursorEntered;
            _mouseInput.OnCursorExit += CursorExited;
        }

        protected void CursorEntered()
        {
            if (highlightEnabled)
            {
                oldColor = Color;
                Color = highlightColor;
            }
        }

        protected void CursorExited()
        {
            if (highlightEnabled)
            {
                Color = oldColor;
            }
        }
    }
}