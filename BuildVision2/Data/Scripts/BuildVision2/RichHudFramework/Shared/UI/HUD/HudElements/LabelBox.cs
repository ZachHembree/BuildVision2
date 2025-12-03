using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
	/// <summary>
	/// A composite UI element that renders text over a textured background.
	/// </summary>
	public class LabelBox : LabelBoxBase, ILabelElement
	{
		/// <summary>
		/// Gets or sets the rich text content displayed by the label.
		/// </summary>
		public RichText Text { get { return TextBoard.GetText(); } set { TextBoard.SetText(value); } }

		/// <summary>
		/// Gets or sets the default glyph formatting (font, style, size, color) applied to text 
		/// that does not have specific formatting defined.
		/// </summary>
		public GlyphFormat Format { get { return TextBoard.Format; } set { TextBoard.SetFormatting(value); } }

		/// <summary>
		/// Gets or sets the padding applied to the text element, offsetting it from the edges of the background box.
		/// </summary>
		public override Vector2 TextPadding { get { return textElement.Padding; } set { textElement.Padding = value; } }

		/// <summary>
		/// Gets or sets the total size of the text element, including the applied <see cref="TextPadding"/>.
		/// </summary>
		public override Vector2 TextSize { get { return textElement.Size; } set { textElement.Size = value; } }

		/// <summary>
		/// If true, the element will automatically resize to fit the text content.
		/// </summary>
		public override bool AutoResize { get { return TextBoard.AutoResize; } set { TextBoard.AutoResize = value; } }

		/// <summary>
		/// Gets or sets the text composition mode, which controls how text is arranged (e.g., single line, wrapped, etc.).
		/// </summary>
		public TextBuilderModes BuilderMode { get { return TextBoard.BuilderMode; } set { TextBoard.BuilderMode = value; } }

		/// <summary>
		/// If true, the text will be vertically centered relative to the text board's bounds.
		/// </summary>
		public bool VertCenterText { get { return TextBoard.VertCenterText; } set { TextBoard.VertCenterText = value; } }

		/// <summary>
		/// Gets the <see cref="ITextBoard"/> backing the internal label element.
		/// </summary>
		public ITextBoard TextBoard { get; }

		/// <summary>
		/// The internal <see cref="Label"/> element responsible for rendering the text.
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