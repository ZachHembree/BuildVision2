using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// A box with a text field and a colored background.
    /// </summary>
    public class LabelBox : LabelBoxBase
    {
        public RichText Text { get { return textElement.Text; } set { textElement.Text = value; } }
        public GlyphFormat Format { get { return textElement.TextBoard.Format; } set { textElement.TextBoard.Format = value; } }

        public override Vector2 TextPadding { get { return textElement.Padding; } set { textElement.Padding = value; } }
        public override Vector2 TextSize { get { return textElement.Size; } protected set { textElement.Size = value; } }

        /// <summary>
        /// If true, the element will automatically resize to fit the text.
        /// </summary>
        public override bool AutoResize { get { return textElement.AutoResize; } set { textElement.AutoResize = value; } }

        public TextBuilderModes BuilderMode { get { return TextBoard.BuilderMode; } set { TextBoard.BuilderMode = value; } }

        /// <summary>
        /// If true, the text will be vertically centered.
        /// </summary>
        public bool VertCenterText { get { return textElement.VertCenterText; } set { textElement.VertCenterText = value; } }

        public ITextBoard TextBoard => textElement.TextBoard;
        public IHudElement TextElement => textElement;

        protected readonly Label textElement;

        public LabelBox(IHudParent parent = null) : base(parent)
        {
            textElement = new Label(this);
        }
    }
}
