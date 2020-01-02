namespace RichHudFramework.UI
{
    public class LabelButton : Label
    {
        public override float Width
        {
            set
            {
                base.Width = value;
                mouseInput.Width = value;
            }
        }

        public override float Height
        {
            set
            {
                base.Height = value;
                mouseInput.Height = value;
            }
        }

        public IClickableElement MouseInput => mouseInput;
        public override bool IsMousedOver => mouseInput.IsMousedOver;

        private ClickableElement mouseInput;

        public LabelButton(IHudParent parent = null) : base(parent)
        {
            mouseInput = new ClickableElement(this);
        }
    }
}