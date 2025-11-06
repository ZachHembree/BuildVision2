using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// A text element with a textured background.
    /// </summary>
    public class LabelBox : LabelBoxBase, ILabelElement
    {
        /// <summary>
        /// Text rendered by the label.
        /// </summary>
        public RichText Text { get { return TextBoard.GetText(); } set { TextBoard.SetText(value); } }

        /// <summary>
        /// Default formatting used by the label.
        /// </summary>
        public GlyphFormat Format { get { return TextBoard.Format; } set { TextBoard.SetFormatting(value); } }

        /// <summary>
        /// Padding applied to the text element.
        /// </summary>
        public override Vector2 TextPadding { get { return textElement.Padding; } set { textElement.Padding = value; } }

        /// <summary>
        /// Size of the text element including TextPadding.
        /// </summary>
        public override Vector2 TextSize { get { return textElement.Size; } set { textElement.Size = value; } }

        /// <summary>
        /// If true, the element will automatically resize to fit the text.
        /// </summary>
        public override bool AutoResize { get { return TextBoard.AutoResize; } set { TextBoard.AutoResize = value; } }

        /// <summary>
        /// Line formatting mode used by the label.
        /// </summary>
        public TextBuilderModes BuilderMode { get { return TextBoard.BuilderMode; } set { TextBoard.BuilderMode = value; } }

        /// <summary>
        /// If true, the text will be vertically centered.
        /// </summary>
        public bool VertCenterText { get { return TextBoard.VertCenterText; } set { TextBoard.VertCenterText = value; } }

        /// <summary>
        /// TextBoard backing the label element.
        /// </summary>
        public ITextBoard TextBoard { get; }

        /// <summary>
        /// Text element contained by the label box.
        /// </summary>
        public readonly Label textElement;

        public LabelBox(HudParentBase parent) : base(parent)
        {
            textElement = new Label(this);
            TextBoard = textElement.TextBoard;
        }

        public LabelBox() : this(null)
        { }
    }
}
