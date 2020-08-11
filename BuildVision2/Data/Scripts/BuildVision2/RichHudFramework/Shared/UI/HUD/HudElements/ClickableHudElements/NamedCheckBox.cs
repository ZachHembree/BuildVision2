using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI.Server
{
    /// <summary>
    /// Named checkbox designed to mimic the appearance of checkboxes used in the SE terminal.
    /// </summary>
    public class NamedCheckBox : HudElementBase, IClickableElement
    {
        /// <summary>
        /// Text rendered by the label.
        /// </summary>
        public RichText Name { get { return name.TextBoard.GetText(); } set { name.TextBoard.SetText(value); } }

        /// <summary>
        /// Default formatting used by the label.
        /// </summary>
        public GlyphFormat Format { get { return name.TextBoard.Format; } set { name.TextBoard.Format = value; } }

        /// <summary>
        /// Size of the text element sans padding.
        /// </summary>
        public Vector2 TextSize { get { return name.Size; } set { name.Size = value; } }

        /// <summary>
        /// Padding applied to the text element.
        /// </summary>
        public Vector2 TextPadding { get { return name.Padding; } set { name.Padding = value; } }

        /// <summary>
        /// If true, the element will automatically resize to fit the text.
        /// </summary>
        public bool AutoResize { get { return name.AutoResize; } set { name.AutoResize = value; } }

        /// <summary>
        /// Line formatting mode used by the label.
        /// </summary>
        public TextBuilderModes BuilderMode { get { return name.BuilderMode; } set { name.BuilderMode = value; } }

        /// <summary>
        /// If true, the text will be vertically centered.
        /// </summary>
        public bool VertCenterText { get { return name.VertCenterText; } set { name.VertCenterText = value; } }

        /// <summary>
        /// If true, then the background will resize to match the size of the text plus padding. Otherwise,
        /// size will be clamped such that the element will not be smaller than the text element.
        /// </summary>
        public bool FitToTextElement { get; set; }

        public override float Width
        {
            get { return FitToTextElement ? chain.Width + Padding.X : base.Width; }
            set 
            {
                if (!FitToTextElement)
                    value = MathHelper.Max(chain.Width, value);

                if (value > Padding.X)
                    value -= Padding.X;

                if (FitToTextElement)
                    name.Width = value - checkbox.Width - chain.Spacing;
                else
                    base.Width = value;
            }
        }

        public override float Height 
        { 
            get { return FitToTextElement ? chain.Height + Padding.Y : base.Height; } 
            set 
            {
                if (!FitToTextElement)
                    value = MathHelper.Max(chain.Height, value);

                if (value > Padding.Y)
                    value -= Padding.Y;

                if (FitToTextElement)
                    chain.Height = value;
                else
                    base.Height = value;

                checkbox.Height = value;
            } 
        }

        /// <summary>
        /// TextBoard backing the label element.
        /// </summary>
        public ITextBoard TextBoard => name.TextBoard;

        /// <summary>
        /// Checkbox mouse input
        /// </summary>
        public IMouseInput MouseInput => checkbox.MouseInput;

        /// <summary>
        /// Indicates whether or not the box is checked.
        /// </summary>
        public bool BoxChecked { get { return checkbox.BoxChecked; } set { checkbox.BoxChecked = value; } }

        private readonly Label name;
        private readonly BorderedCheckBox checkbox;
        private readonly HudChain chain;

        public NamedCheckBox(HudParentBase parent = null) : base(parent)
        {
            name = new Label()
            {
                Format = TerminalFormatting.ControlFormat.WithAlignment(TextAlignment.Right),
                Text = "NewCheckbox",
                AutoResize = false
            };

            checkbox = new BorderedCheckBox();

            chain = new HudChain(false, this)
            {
                SizingMode = HudChainSizingModes.FitChainOffAxis | HudChainSizingModes.FitChainBoth,
                Spacing = 17f,
                ChainContainer =
                {
                    name,
                    checkbox,
                }
            };

            Height = 36f;
        }

        protected override void Layout()
        {
            // The element may not be smaller than the text
            if (!FitToTextElement)
            {
                _absoluteWidth = MathHelper.Max(TextSize.Y, _absoluteWidth * Scale) / Scale;
                _absoluteHeight = MathHelper.Max(TextSize.X, _absoluteHeight * Scale) / Scale;
            }
        }
    }
}