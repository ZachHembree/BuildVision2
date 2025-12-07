namespace RichHudFramework.UI
{
    using Rendering;

	/// <summary>
	/// Minimal interface for UI elements rendering formatted <see cref="RichText"/>
	/// </summary>
    public interface IMinLabelElement
    {
		/// <summary>
		/// Gets the <see cref="ITextBoard"/> backing this element, which handles the low-level text 
		/// rendering and layout logic.
		/// </summary>
		ITextBoard TextBoard { get; }
    }

	/// <summary>
	/// Interface for UI elements rendering formatted <see cref="RichText"/>
	/// </summary>
	public interface ILabelElement : IMinLabelElement
    {
		/// <summary>
		/// Gets or sets the rich text content displayed by the label.
		/// </summary>
		RichText Text { get; set; }

		/// <summary>
		/// Gets or sets the default glyph formatting (font, style, size, color) applied to text that 
		/// does not have specific formatting defined.
		/// </summary>
		GlyphFormat Format { get; set; }

		/// <summary>
		/// Gets or sets the text composition mode, which controls how text is arranged 
		/// (e.g., single line, wrapped, etc.).
		/// </summary>
		TextBuilderModes BuilderMode { get; set; }

		/// <summary>
		/// If true, the size of this UI element will automatically adjust to match the size of the text. 
		/// True by default.
		/// </summary>
		bool AutoResize { get; set; }

		/// <summary>
		/// If true, the text will be vertically centered within the bounds of the element. True by default.
		/// </summary>
		bool VertCenterText { get; set; }
    }
}
