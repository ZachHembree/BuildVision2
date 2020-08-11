using VRageMath;
using System;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Clickable label box. 
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
        /// Mouse input for the button.
        /// </summary>
        public IMouseInput MouseInput => _mouseInput;

        protected MouseInputElement _mouseInput;
        private Color oldColor;

        public LabelBoxButton(HudParentBase parent = null) : base(parent)
        {
            _mouseInput = new MouseInputElement(this) { DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding };
            _mouseInput.OnCursorEnter += CursorEntered;
            _mouseInput.OnCursorExit += CursorExited;
        }

        protected virtual void CursorEntered(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                oldColor = Color;
                Color = HighlightColor;
            }
        }

        protected virtual void CursorExited(object sender, EventArgs args)
        {
            if (HighlightEnabled)
            {
                Color = oldColor;
            }
        }
    }
}