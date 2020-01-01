using VRageMath;

namespace RichHudFramework.UI
{
    public class TextBoxButton : LabelBox
    {
        public override float Width
        {
            get { return TextSize.X + Padding.X; }
            set
            {
                TextSize = new Vector2(value - Padding.X, TextSize.Y);
                background.Width = value;
                mouseInput.Width = value;
            }
        }
        public override float Height
        {
            get { return TextSize.Y + Padding.Y; }
            set
            {
                TextSize = new Vector2(TextSize.X, value - Padding.Y);
                background.Height = value;
                mouseInput.Height = value;
            }
        }

        public virtual Color HighlightColor { get; set; }
        public virtual bool HighlightEnabled { get; set; }
        public override bool IsMousedOver => mouseInput.IsMousedOver;

        public IClickableElement MouseInput => mouseInput;

        protected ClickableElement mouseInput;
        private Color oldColor;

        public TextBoxButton(IHudParent parent = null) : base(parent)
        {
            mouseInput = new ClickableElement(this);
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

        protected override void Draw()
        {
            if (background.Width != Width)
            {
                background.Width = Width;
                mouseInput.Width = Width;
            }

            if (background.Height != Height)
            {
                background.Height = Height;
                mouseInput.Height = Height;
            }
        }
    }
}