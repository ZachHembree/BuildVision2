namespace RichHudFramework.UI
{
	/// <summary>
	/// Clickable text element. Text only, no background.
	/// <para>No styling by default; it's just a clickable <see cref="Label"/> with no highlighting.</para>
	/// </summary>
	public class LabelButton : Label, IClickableElement
    {
		/// <summary>
		/// Interface for managing gaining/losing input focus
		/// </summary>
		public IFocusHandler FocusHandler { get; }

		/// <summary>
		/// Handles mouse input for the button.
		/// </summary>
		public IMouseInput MouseInput { get; }

        /// <summary>
        /// Indicates whether or not the cursor is currently positioned over the button.
        /// </summary>
        public override bool IsMousedOver => _mouseInput.IsMousedOver;

		/// <exclude/>
		protected MouseInputElement _mouseInput;

        public LabelButton(HudParentBase parent) : base(parent)
        {
			FocusHandler = new InputFocusHandler(this);
			_mouseInput = new MouseInputElement(this);
            MouseInput = _mouseInput;
        }

        public LabelButton() : this(null)
        { }
    }
}