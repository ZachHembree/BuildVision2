using VRageMath;

namespace RichHudFramework.UI
{
    public class TextBoxButton : LabelBox
    {
        public virtual Color HighlightColor { get; set; }
        public virtual bool HighlightEnabled { get; set; }
        public override bool IsMousedOver => mouseInput.IsMousedOver;

        public IClickableElement MouseInput => mouseInput;

        protected ClickableElement mouseInput;
        private Color oldColor;

        public TextBoxButton(IHudParent parent = null) : base(parent)
        {
            mouseInput = new ClickableElement(this) { DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding };
            mouseInput.OnCursorEnter += CursorEntered;
            mouseInput.OnCursorExit += CursorExited;
        }

        protected virtual void CursorEntered()
        {
            if (HighlightEnabled)
            {
                oldColor = Color;
                Color = HighlightColor;
            }
        }

        protected virtual void CursorExited()
        {
            if (HighlightEnabled)
            {
                Color = oldColor;
            }
        }
    }
}