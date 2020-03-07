namespace RichHudFramework.UI
{
    /// <summary>
    /// Clickable text element. Text only, no background.
    /// </summary>
    public class LabelButton : Label, IClickableElement
    {
        /// <summary>
        /// Width of the hud element in pixels.
        /// </summary>
        public override float Width
        {
            set
            {
                base.Width = value;
                _mouseInput.Width = value;
            }
        }

        /// <summary>
        /// Height of the hud element in pixels.
        /// </summary>
        public override float Height
        {
            set
            {
                base.Height = value;
                _mouseInput.Height = value;
            }
        }

        /// <summary>
        /// Handles mouse input for the button.
        /// </summary>
        public IMouseInput MouseInput => _mouseInput;

        /// <summary>
        /// Indicates whether or not the cursor is currently positioned over the button.
        /// </summary>
        public override bool IsMousedOver => _mouseInput.IsMousedOver;

        private MouseInputElement _mouseInput;

        public LabelButton(IHudParent parent = null) : base(parent)
        {
            _mouseInput = new MouseInputElement(this);
        }
    }
}