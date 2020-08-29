using System;
using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI.Server
{
    /// <summary>
    /// Unlined clickable textbox with a background and border designed to look like text fields in the SE
    /// terminal.
    /// </summary>
    public class TextField : LabelBoxBase, IClickableElement
    {
        /// <summary>
        /// Invoked whenever a change is made to the text. Invokes once every 500ms, at most.
        /// </summary>
        public event EventHandler OnTextChanged;

        /// <summary>
        /// Text rendered by the text field.
        /// </summary>
        public RichText Text { get { return textBox.TextBoard.GetText(); } set { textBox.TextBoard.SetText(value); } }

        /// <summary>
        /// TextBoard backing the text field.
        /// </summary>
        public ITextBoard TextBoard => textBox.TextBoard;

        /// <summary>
        /// Default formatting used by the text field.
        /// </summary>
        public GlyphFormat Format { get { return textBox.Format; } set { textBox.Format = value; } }

        public override Vector2 TextSize { get { return textBox.Size; } set { textBox.Size = value; } }

        public override Vector2 TextPadding { get { return textBox.Padding; } set { textBox.Padding = value; } }

        public override bool AutoResize { get { return textBox.AutoResize; } set { textBox.AutoResize = value; } }

        /// <summary>
        /// Determines whether or not the textbox will allow the user to edit its contents
        /// </summary>
        public bool EnableEditing { get { return textBox.EnableEditing; } set { textBox.EnableEditing = value; } }

        /// <summary>
        /// Determines whether the user will be allowed to highlight text
        /// </summary>
        public bool EnableHighlighting { get { return textBox.EnableHighlighting; } set { textBox.EnableHighlighting = value; } }

        /// <summary>
        /// Indicates whether or not the text field will accept input
        /// </summary>
        public bool InputOpen { get { return textBox.EnableEditing; } }

        /// <summary>
        /// Used to restrict the range of characters allowed for input.
        /// </summary>
        public Func<char, bool> CharFilterFunc { get { return textBox.CharFilterFunc; } set { textBox.CharFilterFunc = value; } }

        /// <summary>
        /// Index of the first character in the selected range.
        /// </summary>
        public Vector2I SelectionStart => textBox.SelectionStart;

        /// <summary>
        /// Index of the last character in the selected range.
        /// </summary>
        public Vector2I SelectionEnd => textBox.SelectionEnd;

        /// <summary>
        /// If true, then text box currently has a range of characters selected.
        /// </summary>
        public bool SelectionEmpty => textBox.SelectionEmpty;

        /// <summary>
        /// Gets/sets the background color of the text field
        /// </summary>
        public Color BackgroundColor { get { return background.Color; } set { background.Color = value; } }

        /// <summary>
        /// Color of the thin border surrounding the text field
        /// </summary>
        public Color BorderColor { get { return border.Color; } set { border.Color = value; } }

        /// <summary>
        /// Thickness of the border around the text field
        /// </summary>
        public float BorderThickness { get { return border.Thickness; } set { border.Thickness = value; } }

        public IMouseInput MouseInput => textBox.MouseInput;

        public override bool IsMousedOver => textBox.IsMousedOver;

        private readonly TextBox textBox;
        private readonly TexturedBox highlight;
        private readonly BorderBox border;

        public TextField(HudParentBase parent = null) : base(parent)
        {
            background.Color = new Color(42, 55, 63);

            border = new BorderBox(background)
            {
                Color = TerminalFormatting.BorderColor,
                Thickness = 1f,
                DimAlignment = DimAlignments.Both,
            };

            textBox = new TextBox(background)
            {
                Format = TerminalFormatting.ControlFormat,
                AutoResize = false,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                Padding = new Vector2(24f, 0f),
            };

            highlight = new TexturedBox(background)
            {
                Color = TerminalFormatting.HighlightOverlayColor,
                DimAlignment = DimAlignments.Both,
                Visible = false,
            };

            Size = new Vector2(319f, 40);

            textBox.TextBoard.OnTextChanged += TextChanged;
        }

        private void TextChanged()
        {
            OnTextChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (textBox.IsMousedOver)
            {
                highlight.Visible = true;
            }
            else
            {
                highlight.Visible = false;
            }
        }
    }
}