using VRageMath;
using System;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Clickable button with text on top of a textured background with highlighting. 
    /// </summary>
    public class LabelBoxButton : LabelBox, IClickableElement
    {
        /// <summary>
        /// Color of the background when moused over.
        /// </summary>
        public virtual Color HighlightColor { get; set; }

        /// <summary>
        /// Determines whether or not the button will highlight when moused over.
        /// </summary>
        public virtual bool HighlightEnabled { get; set; }

        /// <summary>
        /// Indicates whether or not the cursor is currently over the element.
        /// </summary>
        public override bool IsMousedOver => _mouseInput.IsMousedOver;

		/// <summary>
		/// Interface used to manage the element's input focus state
		/// </summary>
		public IFocusHandler FocusHandler { get; }

		/// <summary>
		/// Mouse input for the button.
		/// </summary>
		public IMouseInput MouseInput { get; }

        /// <exclude/>
        protected MouseInputElement _mouseInput;

        /// <summary>
        /// Last background color set before highlighting
        /// </summary>
        /// <exclude/>
        protected Color oldColor;

        public LabelBoxButton(HudParentBase parent) : base(parent)
        {
			FocusHandler = new InputFocusHandler(this);
            _mouseInput = new MouseInputElement(this)
            { 
                CursorEnteredCallback = CursorEnter,
                CursorExitedCallback = CursorExit
            };

            MouseInput = _mouseInput;
            Color = Color.DarkGray;
            HighlightColor = Color.Gray;
            HighlightEnabled = true;
        }

        public LabelBoxButton() : this(null)
        { }

        /// <summary>
        /// Sets highlighting when the cursor enters the button
        /// </summary>
        /// <exclude/>
        protected virtual void CursorEnter(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                oldColor = Color;
                Color = HighlightColor;
            }
        }

        /// <summary>
        /// Clears highlighting when the cursor leaves the button
        /// </summary>
        /// <exclude/>
        protected virtual void CursorExit(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                Color = oldColor;
            }
        }
    }
}