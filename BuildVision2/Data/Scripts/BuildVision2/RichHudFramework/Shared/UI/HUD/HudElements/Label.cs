using VRageMath;

namespace RichHudFramework.UI
{
	using Rendering;
	using Rendering.Client;
	using Rendering.Server;

	/// <summary>
	/// A HUD element dedicated to rendering formatted <see cref="RichText"/>. 
	/// This element acts as a wrapper for the underlying <see cref="TextBoard"/>.
	/// </summary>
	public class Label : LabelElementBase
	{
		/// <summary>
		/// Gets or sets the rich text content displayed by the label.
		/// </summary>
		public RichText Text { get { return TextBoard.GetText(); } set { TextBoard.SetText(value); } }

		/// <summary>
		/// Gets the <see cref="ITextBoard"/> backing this element, which handles the low-level text 
		/// rendering and layout logic.
		/// </summary>
		public override ITextBoard TextBoard { get; }

		/// <summary>
		/// Gets or sets the default glyph formatting (font, style, size, color) applied to text that 
		/// does not have specific formatting defined.
		/// </summary>
		public GlyphFormat Format { get { return TextBoard.Format; } set { TextBoard.SetFormatting(value); } }

		/// <summary>
		/// Gets or sets the text composition mode, which controls how text is arranged 
		/// (e.g., single line, wrapped, etc.).
		/// </summary>
		public TextBuilderModes BuilderMode { get { return TextBoard.BuilderMode; } set { TextBoard.BuilderMode = value; } }

		/// <summary>
		/// If true, the size of this UI element will automatically adjust to match the size of the text. 
		/// True by default.
		/// </summary>
		public bool AutoResize { get { return TextBoard.AutoResize; } set { TextBoard.AutoResize = value; } }

		/// <summary>
		/// If true, the text will be vertically centered within the bounds of the element. True by default.
		/// </summary>
		public bool VertCenterText { get { return TextBoard.VertCenterText; } set { TextBoard.VertCenterText = value; } }

		/// <summary>
		/// Gets or sets the maximum width of a line before the text wraps to the next line. 
		/// <para>
		/// Note: The <see cref="BuilderMode"/> must be set to <see cref="TextBuilderModes.Wrapped"/> for this to take effect.
		/// </para>
		/// </summary>
		public float LineWrapWidth { get { return TextBoard.LineWrapWidth; } set { TextBoard.LineWrapWidth = value; } }

		public Label(HudParentBase parent) : base(parent)
		{
			TextBoard = new TextBoard();
			TextBoard.SetText("NewLabel", GlyphFormat.White);
			UnpaddedSize = new Vector2(50f);
		}

		public Label() : this(null)
		{ }

		/// <summary>
		/// Updates the size of the UI element to match the text dimensions if <see cref="AutoResize"/> is enabled.
		/// </summary>
		/// <exclude/>
		protected override void Measure()
		{
			if (TextBoard.AutoResize)
				UnpaddedSize = TextBoard.TextSize;
		}

		/// <summary>
		/// Draws the text and handles masking logic.
		/// </summary>
		/// <exclude/>
		protected override void Draw()
		{
			Vector2 halfSize = .5f * UnpaddedSize;
			BoundingBox2 box = new BoundingBox2(Position - halfSize, Position + halfSize);

			if (!TextBoard.AutoResize)
				TextBoard.FixedSize = UnpaddedSize;

			if (MaskingBox != null)
				TextBoard.Draw(box, MaskingBox.Value, HudSpace.PlaneToWorldRef);
			else
				TextBoard.Draw(box, CroppedBox.defaultMask, HudSpace.PlaneToWorldRef);
		}
	}
}