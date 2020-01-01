using VRageMath;

namespace RichHudFramework.UI
{
    public class Button : TexturedBox
    {
        public override float Width { get { return base.Width; } set { base.Width = value; mouseInput.Width = value; } }
        public override float Height { get { return base.Height; } set { base.Height = value; mouseInput.Height = value; } }
        public override bool IsMousedOver => mouseInput.IsMousedOver;
        public IClickableElement MouseInput => mouseInput;

        public bool highlightEnabled;
        public Color highlightColor;

        private readonly ClickableElement mouseInput;
        private Color oldColor;

        public Button(IHudParent parent = null) : base(parent)
        {
            mouseInput = new ClickableElement(this);
            highlightColor = new Color(255, 255, 255, 125);
            highlightEnabled = true;

            mouseInput.OnCursorEnter += CursorEntered;
            mouseInput.OnCursorExit += CursorExited;
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