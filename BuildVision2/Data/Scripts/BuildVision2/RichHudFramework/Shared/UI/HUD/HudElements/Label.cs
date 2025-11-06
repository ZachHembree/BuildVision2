using VRageMath;

namespace RichHudFramework.UI
{
	using Rendering;
	using Rendering.Client;
	using Rendering.Server;

	/// <summary>
	/// HUD element used to render text.
	/// </summary>
	public class Label : LabelElementBase
	{
		/// <summary>
		/// Text rendered by the label.
		/// </summary>
		public RichText Text { get { return TextBoard.GetText(); } set { TextBoard.SetText(value); } }

		/// <summary>
		/// TextBoard backing the label element.
		/// </summary>
		public override ITextBoard TextBoard { get; }

		/// <summary>
		/// Default formatting used by the label.
		/// </summary>
		public GlyphFormat Format { get { return TextBoard.Format; } set { TextBoard.SetFormatting(value); } }

		/// <summary>
		/// Line formatting mode used by the label.
		/// </summary>
		public TextBuilderModes BuilderMode { get { return TextBoard.BuilderMode; } set { TextBoard.BuilderMode = value; } }

		/// <summary>
		/// If true, the element will automatically resize to fit the text. True by default.
		/// </summary>
		public bool AutoResize { get { return TextBoard.AutoResize; } set { TextBoard.AutoResize = value; } }

		/// <summary>
		/// If true, the text will be vertically centered. True by default.
		/// </summary>
		public bool VertCenterText { get { return TextBoard.VertCenterText; } set { TextBoard.VertCenterText = value; } }

		/// <summary>
		/// Gets or sets the maximum line width before text will wrap to the next line. Word wrapping must be enabled for
		/// this to apply.
		/// </summary>
		public float LineWrapWidth { get { return TextBoard.LineWrapWidth; } set { TextBoard.LineWrapWidth = value; } }

		public Label(HudParentBase parent) : base(parent)
		{
			TextBoard = new TextBoard();
			TextBoard.SetText("NewLabel", GlyphFormat.White);
		}

		public Label() : this(null)
		{ }

		protected override void UpdateSize()
		{
			if (TextBoard.AutoResize)
				UnpaddedSize = TextBoard.TextSize;
		}

		protected override void Draw()
		{
			Vector2 halfSize = .5f * UnpaddedSize;
			BoundingBox2 box = new BoundingBox2(Position - halfSize, Position + halfSize);

			if (!TextBoard.AutoResize)
				TextBoard.FixedSize = UnpaddedSize;

			if (maskingBox != null)
				TextBoard.Draw(box, maskingBox.Value, HudSpace.PlaneToWorldRef);
			else
				TextBoard.Draw(box, CroppedBox.defaultMask, HudSpace.PlaneToWorldRef);
		}
	}
}
