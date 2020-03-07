using System.Text;
using VRage;
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
        /// Defines the formatting of the characters in rich text types.
        /// </summary>
        public class GlyphFormat
        {
            public static readonly GlyphFormat 
                Black = new GlyphFormat(),
                White = new GlyphFormat(color: Color.White), 
                Blueish = new GlyphFormat(color: new Color(220, 235, 242)),
                Empty = new GlyphFormat(default(GlyphFormatMembers));

            /// <summary>
            /// Determines the alignment (left, center, right) of a given piece of RichText.
            /// </summary>
            public TextAlignment Alignment => (TextAlignment)data.Item1;

            /// <summary>
            /// Text size
            /// </summary>
            public float TextSize => data.Item2;

            /// <summary>
            /// Font specified by the format.
            /// </summary>
            public IFontMin Font => FontManager.GetFont(data.Item3.X);

            /// <summary>
            /// The font style specifed by the format.
            /// </summary>
            public FontStyles FontStyle => (FontStyles)data.Item3.Y;

            /// <summary>
            /// The font and style used by the format represented as a pair of integers.
            /// </summary>
            public Vector2I StyleIndex => data.Item3;

            /// <summary>
            /// Text color
            /// </summary>
            public Color Color => data.Item4;

            public readonly GlyphFormatMembers data;

            public GlyphFormat(Color color = default(Color), TextAlignment alignment = TextAlignment.Left, float textSize = 1f, Vector2I fontStyle = default(Vector2I))
            {
                if (color == default(Color))
                    color = Color.Black;

                data = new GlyphFormatMembers((byte)alignment, textSize, fontStyle, color);
            }

            public GlyphFormat(GlyphFormatMembers data)
            {
                this.data = data;
            }

            public GlyphFormat(GlyphFormat original)
            {
                data = original.data;
            }

            /// <summary>
            /// Returns a copy of the <see cref="GlyphFormat"/> using the specified <see cref="VRageMath.Color"/>.
            /// </summary>
            public GlyphFormat WithColor(Color color) =>
                new GlyphFormat(color, Alignment, TextSize, StyleIndex);

            /// <summary>
            /// Returns a copy of the <see cref="GlyphFormat"/> using the specified <see cref="TextAlignment"/>.
            /// </summary>
            public GlyphFormat WithAlignment(TextAlignment textAlignment) =>
                new GlyphFormat(Color, textAlignment, TextSize, StyleIndex);

            /// <summary>
            /// Returns a copy of the <see cref="GlyphFormat"/> using the font associated with the given index.
            /// </summary>
            public GlyphFormat WithFont(int font) =>
                new GlyphFormat(Color, Alignment, TextSize, new Vector2I(font, 0));

            /// <summary>
            /// Returns a copy of the <see cref="GlyphFormat"/> using the font style associated with the given index.
            /// </summary>
            public GlyphFormat WithFont(Vector2I fontStyle) =>
                new GlyphFormat(Color, Alignment, TextSize, fontStyle);

            /// <summary>
            /// Returns a copy of the <see cref="GlyphFormat"/> using the font style associated with the given enum.
            /// </summary>
            public GlyphFormat WithStyle(FontStyles style)
            {
                if (FontManager.GetFont(StyleIndex.X).IsStyleDefined(style))
                    return new GlyphFormat(Color, Alignment, TextSize, new Vector2I(StyleIndex.X, (int)style));
                else
                    return this;
            }

            /// <summary>
            /// Returns a copy of the <see cref="GlyphFormat"/> using the font style associated with the given enum.
            /// </summary>
            public GlyphFormat WithStyle(int style)
            {
                if (FontManager.GetFont(StyleIndex.X).IsStyleDefined(style))
                    return new GlyphFormat(Color, Alignment, TextSize, new Vector2I(StyleIndex.X, style));
                else
                    return this;
            }

            /// <summary>
            /// Returns a copy of the <see cref="GlyphFormat"/> using the given text size.
            /// </summary>
            public GlyphFormat WithSize(float size) =>
                new GlyphFormat(Color, Alignment, size, StyleIndex);

            /// <summary>
            /// Determines whether or not two given <see cref="GlyphFormat"/>s share the same configuration.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj is GlyphFormat)
                    return data.Equals(((GlyphFormat)obj).data);
                else
                    return false;
            }

            public override int GetHashCode() =>
                data.GetHashCode();
        }
    }
}