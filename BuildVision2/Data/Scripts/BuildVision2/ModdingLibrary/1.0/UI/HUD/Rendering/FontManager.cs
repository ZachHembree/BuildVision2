using System;
using System.Collections.Generic;
using DarkHelmet.UI.FontData;
using VRageMath;

namespace DarkHelmet.UI.Rendering
{
    [Flags]
    public enum FontStyle
    {
        Regular = 0,
        Bold = 1,
        Italic = 2
    }

    /// <summary>
    /// Stores the texture and character value for a character in a given font.
    /// </summary>
    public class Glyph
    {
        public readonly Material material;
        public readonly float advanceWidth, leftSideBearing;

        public Glyph(Material atlas, Vector2 size, Vector2 origin, float aw, float lsb)
        {
            material = new Material(atlas.TextureID, atlas.size, origin, size);
            advanceWidth = aw;
            leftSideBearing = lsb;
        }
    }

    public static class FontManager
    {
        public static Font.Style Default => fonts[0][0];
        private static readonly List<Font> fonts;

        static FontManager()
        {
            fonts = new List<Font>();
        }

        public static bool TryAddFont(Font font)
        {
            if (fonts.Find(x => x.name == font.name) == null)
            {
                fonts.Add(font);
                return true;
            }
            else
                return false;
        }

        public static Font GetFont(string name)
        {
            foreach (Font font in fonts)
                if (font.name == name)
                    return font;

            return null;
        }
    }

    public class Font
    {
        public Style this[int index] => styles[index];
        public Style this[FontStyle type] => styles[(int)type];

        public readonly string name;
        public readonly float ptSize, baseScale;

        private readonly Style[] styles;

        public Font(string name, float ptSize)
        {
            this.name = name;
            this.ptSize = ptSize;

            baseScale = 12f / ptSize;
            styles = new Style[4];            
        }

        public bool TryAddStyle(int style, float height, float baseLine, Dictionary<char, Glyph> glyphs, Dictionary<uint, float> kerningPairs)
        {
            if (styles[style] == null)
            {
                styles[style] = new Style(this, height, baseLine, glyphs, kerningPairs);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Stores all <see cref="Material"/>s and related information needed for rendering a font from a set of sprites.
        /// </summary>
        public class Style
        {
            public Glyph this[char ch]
            {
                get
                {
                    Glyph value;

                    if (!glyphs.TryGetValue(ch, out value))
                    {
                        glyphs.TryGetValue((char)0x25a1, out value);
                    };

                    return value;
                }
            }
            public float PtSize => font.ptSize;
            public float FontScale => font.baseScale;

            public readonly Font font;
            public readonly float height, baseLine;

            private readonly Dictionary<char, Glyph> glyphs;
            private readonly Dictionary<uint, float> kerningPairs;

            public Style(Font parent, float height, float baseline, Dictionary<char, Glyph> glyphs, Dictionary<uint, float> kerningPairs)
            {
                this.font = parent;
                this.height = height;
                this.baseLine = baseline;
                this.glyphs = glyphs;
                this.kerningPairs = kerningPairs;
            }

            /// <summary>
            /// Returns the required adjustment for a given pair of characters.
            /// </summary>
            public float GetKerningAdjustment(char left, char right)
            {
                float value;

                if (kerningPairs.TryGetValue(left + (uint)(right << 16), out value))
                    return value;
                else
                    return 0f;
            }
        }
    }
}