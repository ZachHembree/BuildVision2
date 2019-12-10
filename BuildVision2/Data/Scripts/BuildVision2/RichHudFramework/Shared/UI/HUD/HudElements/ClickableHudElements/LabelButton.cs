namespace DarkHelmet.UI
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

        public LabelButton(RichText text, IHudParent parent = null, bool wordWrapping = false) : base(text, parent, wordWrapping)
        {
            mouseInput = new ClickableElement(this);
        }

        public LabelButton(RichString text, IHudParent parent = null, bool wordWrapping = false) : base(text, parent, wordWrapping)
        {
            mouseInput = new ClickableElement(this);
        }

        public LabelButton(string text, GlyphFormat format, IHudParent parent = null, bool wordWrapping = false) : base(text, format, parent, wordWrapping)
        {
            mouseInput = new ClickableElement(this);
        }
    }
}