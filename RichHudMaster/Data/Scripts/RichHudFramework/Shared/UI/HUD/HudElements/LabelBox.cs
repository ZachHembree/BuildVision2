using DarkHelmet.UI.Rendering;
using VRageMath;

namespace DarkHelmet.UI
{
    /// <summary>
    /// A box with a text field and a colored background.
    /// </summary>
    public class LabelBox : LabelBoxBase
    {
        public GlyphFormat Format { get { return textElement.Text.Format; } set { textElement.Text.Format = value; } }
        public override Vector2 TextSize { get { return textElement.Size; } protected set { textElement.Size = value; } }

        /// <summary>
        /// If true, the element will automatically resize to fit the text.
        /// </summary>
        public override bool AutoResize { get { return textElement.AutoResize; } set { textElement.AutoResize = value; } }

        /// <summary>
        /// If true, the text will be vertically centered.
        /// </summary>
        public bool VertCenterText { get { return textElement.VertCenterText; } set { textElement.VertCenterText = value; } }

        public ITextBoard Text => textElement.Text;
        public IHudElement TextElement => textElement;

        protected readonly Label textElement;

        public LabelBox(RichText text, IHudParent parent = null, bool wordWrapping = false) : this(parent, wordWrapping)
        {
            Text.Append(text);
        }

        public LabelBox(RichString text, IHudParent parent = null, bool wordWrapping = false) : this(parent, wordWrapping)
        {
            Text.Append(text);
        }

        public LabelBox(string text, GlyphFormat format, IHudParent parent = null, bool wordWrapping = false) : this(parent, wordWrapping)
        {
            if (format != null)
                Text.Format = format;

            Text.Append(text);
        }

        public LabelBox(IHudParent parent = null, bool wordWrapping = false) : base(parent)
        {
            textElement = new Label(this, wordWrapping);
        }
    }
}
