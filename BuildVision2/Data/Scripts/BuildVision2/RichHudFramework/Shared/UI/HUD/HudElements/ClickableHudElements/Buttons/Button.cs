using VRageMath;
using System;

namespace RichHudFramework.UI
{
	/// <summary>
	/// Clickable button with a textured background.
	/// <para>No styling by default; it's just a clickable <see cref="TexturedBox"/> with highlighting.</para>
	/// </summary>
	public class Button : TexturedBox, IClickableElement
    {
        /// <summary>
        /// Indicates whether or not the cursor is currently positioned over the button.
        /// </summary>
        public override bool IsMousedOver => MouseInput.IsMousedOver;

		/// <summary>
		/// Interface used to manage the element's input focus state.
		/// </summary>
		public IFocusHandler FocusHandler { get; }

		/// <summary>
		/// Handles mouse input for the button.
		/// </summary>
		public IMouseInput MouseInput { get; }

        /// <summary>
        /// Determines whether or not the button will highlight when moused over.
        /// </summary>
        public bool HighlightEnabled { get; set; }

        /// <summary>
        /// Color of the background when moused over.
        /// </summary>
        public Color HighlightColor { get; set; }

        /// <exclude/>
        protected readonly MouseInputElement _mouseInput;

		/// <summary>
		/// Last background color used before highlighting
		/// </summary>
		/// <exclude/>
		protected Color lastBackgroundColor;

        public Button(HudParentBase parent) : base(parent)
        {
            FocusHandler = new InputFocusHandler(this);
            _mouseInput = new MouseInputElement(this);
            MouseInput = _mouseInput;

            HighlightColor = new Color(125, 125, 125, 255);
            HighlightEnabled = true;

			MouseInput.CursorEntered += CursorEnter;
			MouseInput.CursorExited += CursorExit;
        }

        public Button() : this(null)
        { }

		/// <summary>
		/// Handles highlighting when the cursor enters the button
		/// </summary>
		/// <exclude/>
		protected virtual void CursorEnter(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                lastBackgroundColor = Color;
                Color = HighlightColor;
            }
        }

		/// <summary>
		/// Resets highlighting when the cursor leaves the button
		/// </summary>
		/// <exclude/>
		protected virtual void CursorExit(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                Color = lastBackgroundColor;
            }
        }
    }
}