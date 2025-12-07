using System;
using VRageMath;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;

namespace RichHudFramework
{
    namespace UI
    {
        using Rendering.Client;
        using Rendering.Server;
        using Rendering;

        /// <summary>
        /// Used to determine text alignment.
        /// </summary>
        public enum TextAlignment : byte
        {
            Left = 0,
            Center = 1,
            Right = 2,
        }

		/// <summary>
		/// Defines the formatting applied to characters in <see cref="RichText"/> and <see cref="ITextBoard"/>.
		/// Includes color, font, style, size, and alignment.
		/// </summary>
		public struct GlyphFormat : IEquatable<GlyphFormat>
        {
            // Predefined common formats
            /// <summary>Black text using default font and regular style.</summary>
            public static readonly GlyphFormat Black = new GlyphFormat();

            /// <summary>White text using default font and regular style.</summary>
            public static readonly GlyphFormat White = new GlyphFormat(color: Color.White);

            /// <summary>Light blue-ish text commonly used for highlighted or neutral UI elements.</summary>
            public static readonly GlyphFormat Blueish = new GlyphFormat(color: new Color(220, 235, 242));

			/// <summary>Empty/invalid format. All fields are default/zero.</summary>
			public static readonly GlyphFormat Empty = new GlyphFormat(default(GlyphFormatMembers));

			/// <summary>
			/// Text alignment for this format (Left, Center, or Right).
			/// </summary>
			public TextAlignment Alignment => (TextAlignment)Data.Item1;

			/// <summary>
			/// Scale multiplier for text size. 1.0f ~= 12pts
			/// </summary>
			public float TextSize => Data.Item2;

			/// <summary>
			/// The font used by this format, retrieved from the global <see cref="FontManager"/>.
			/// </summary>
			public IFontMin Font => FontManager.GetFont(Data.Item3.X);

			/// <summary>
			/// The specific style (Regular, Bold, Italic, etc.) applied to the font.
			/// </summary>
			public FontStyles FontStyle => (FontStyles)Data.Item3.Y;

			/// <summary>
			/// Combined font index and style index as a <see cref="Vector2I"/> (X = font index, Y = style index).
			/// </summary>
			public Vector2I StyleIndex => Data.Item3;

			/// <summary>
			/// The color of the text.
			/// </summary>
			public Color Color => Data.Item4;

			/// <summary>
			/// Internal backing storage for the format data. Exposed for API compatibility.
			/// </summary>
			/// <exclude/>
			public GlyphFormatMembers Data { get; set; }

			// Constructors

			/// <summary>
			/// Creates a new <see cref="GlyphFormat"/> using a font name string.
			/// </summary>
			/// <param name="color">Text color. Defaults to black if unspecified.</param>
			/// <param name="alignment">Text alignment.</param>
			/// <param name="textSize">Text size normalized to 1.0f ~= 12pts.</param>
			/// <param name="fontName">Name of the font registered in <see cref="FontManager"/>.</param>
			/// <param name="style">Font style (Regular, Bold, Italic, etc.).</param>
			public GlyphFormat(Color color, TextAlignment alignment, float textSize,
				string fontName, FontStyles style = FontStyles.Regular) :
				this(color, alignment, textSize, style, FontManager.GetFont(fontName))
			{ }

			/// <summary>
			/// Creates a new <see cref="GlyphFormat"/> using raw font/style indices.
			/// </summary>
			public GlyphFormat(Color color, TextAlignment alignment, float textSize, Vector2I fontStyle)
			{
				if (color == default(Color))
					color = Color.Black;

				Data = new GlyphFormatMembers((byte)alignment, textSize, fontStyle, color);
			}

			/// <summary>
			/// Creates a new <see cref="GlyphFormat"/> using an <see cref="IFontMin"/> instance and style.
			/// </summary>
			/// <param name="color">Text color. Defaults to black.</param>
			/// <param name="alignment">Text alignment. Defaults to Left.</param>
			/// <param name="textSize">Text size. Defaults to 1.0f ~= 12pts.</param>
			/// <param name="style">Font style. Defaults to Regular.</param>
			/// <param name="font">Font instance. Defaults to the system default font if null.</param>
			public GlyphFormat(Color color = default(Color), TextAlignment alignment = TextAlignment.Left,
				float textSize = 1f, FontStyles style = FontStyles.Regular, IFontMin font = null)
			{
				if (color == default(Color))
					color = Color.Black;
				if (font == null)
					font = FontManager.GetFont(FontManager.Default.X);

				Data = new GlyphFormatMembers((byte)alignment, textSize, font.GetStyleIndex(style), color);
			}

			/// <summary>
			/// Wraps raw API format data in a new <see cref="GlyphFormat"/> instance.
			/// </summary>
			/// <exclude/>
			public GlyphFormat(GlyphFormatMembers data) { this.Data = data; }

			/// <summary>
			/// Copy constructor. Creates a new independent copy of the given format.
			/// </summary>
			public GlyphFormat(GlyphFormat original) { Data = original.Data; }

			// Fluent modification methods

			/// <summary>
			/// Returns a new <see cref="GlyphFormat"/> with the specified color.
			/// </summary>
			public GlyphFormat WithColor(Color color) =>
				new GlyphFormat(color, Alignment, TextSize, StyleIndex);

			/// <summary>
			/// Returns a new <see cref="GlyphFormat"/> with the specified alignment.
			/// </summary>
			public GlyphFormat WithAlignment(TextAlignment textAlignment) =>
				new GlyphFormat(Color, textAlignment, TextSize, StyleIndex);

			/// <summary>
			/// Returns a new <see cref="GlyphFormat"/> using the font at the given index (style unchanged).
			/// </summary>
			public GlyphFormat WithFont(int font) =>
				new GlyphFormat(Color, Alignment, TextSize, new Vector2I(font, StyleIndex.Y));

			/// <summary>
			/// Returns a new <see cref="GlyphFormat"/> using the specified font and style.
			/// </summary>
			public GlyphFormat WithFont(IFontMin font, FontStyles style = FontStyles.Regular) =>
				new GlyphFormat(Color, Alignment, TextSize, style, font);

			/// <summary>
			/// Returns a new <see cref="GlyphFormat"/> using the font with the specified name and style.
			/// </summary>
			public GlyphFormat WithFont(string fontName, FontStyles style = FontStyles.Regular) =>
				new GlyphFormat(Color, Alignment, TextSize, style, FontManager.GetFont(fontName));

			/// <summary>
			/// Returns a new <see cref="GlyphFormat"/> using the specified font/style index pair.
			/// </summary>
			public GlyphFormat WithFont(Vector2I fontStyle) =>
				new GlyphFormat(Color, Alignment, TextSize, fontStyle);

			/// <summary>
			/// Returns a new <see cref="GlyphFormat"/> with the specified style applied, if the current font supports it.
			/// Otherwise returns the original format unchanged.
			/// </summary>
			public GlyphFormat WithStyle(FontStyles style)
			{
				if (FontManager.GetFont(StyleIndex.X).IsStyleDefined(style))
					return new GlyphFormat(Color, Alignment, TextSize, new Vector2I(StyleIndex.X, (int)style));
				else
					return this;
			}

			/// <summary>
			/// Returns a new <see cref="GlyphFormat"/> with the specified numeric style index, if supported.
			/// Otherwise returns the original format unchanged.
			/// </summary>
			public GlyphFormat WithStyle(int style)
			{
				if (FontManager.GetFont(StyleIndex.X).IsStyleDefined(style))
					return new GlyphFormat(Color, Alignment, TextSize, new Vector2I(StyleIndex.X, style));
				else
					return this;
			}

			/// <summary>
			/// Returns a new <see cref="GlyphFormat"/> with the specified text size.
			/// </summary>
			public GlyphFormat WithSize(float size) =>
				new GlyphFormat(Color, Alignment, size, StyleIndex);

			// Equality & hashing

			/// <summary>
			/// Determines whether this format is equal to another object.
			/// </summary>
			public override bool Equals(object obj)
            {
                if (obj == null || !(obj is GlyphFormat))
                    return false;

                return Equals((GlyphFormat)obj);
            }

			/// <summary>
			/// Determines whether this format is equal to another <see cref="GlyphFormat"/>.
			/// Two formats are equal if all fields match exactly.
			/// </summary>
			public bool Equals(GlyphFormat format)
            {
                return Data.Item1 == format.Data.Item1
                    && Data.Item2 == format.Data.Item2
                    && Data.Item3 == format.Data.Item3
                    && Data.Item4 == format.Data.Item4;
            }

            /// <summary>
            /// Returns true if the given API format tuple is equivalent to this format object
            /// </summary>
            /// <exclude/>
            public bool DataEqual(GlyphFormatMembers data)
            {
                return Data.Item1 == data.Item1
                    && Data.Item2 == data.Item2
                    && Data.Item3 == data.Item3
                    && Data.Item4 == data.Item4;
            }

			/// <summary>
			/// Returns a hash code for this format.
			/// </summary>
			public override int GetHashCode() =>
                Data.GetHashCode();
        }
    }
}