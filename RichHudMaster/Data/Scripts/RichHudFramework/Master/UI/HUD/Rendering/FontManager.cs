using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using AtlasMembers = VRage.MyTuple<string, VRageMath.Vector2>;
using GlyphMembers = VRage.MyTuple<int, VRageMath.Vector2, VRageMath.Vector2, float, float>;

namespace DarkHelmet
{
    using FontMembers = MyTuple<
        string, // Name
        int, // Index
        float, // PtSize
        float, // BaseScale
        Func<int, bool> // IsStyleDefined
    >;
    using FontStyleDefinition = MyTuple<
        int, // styleID
        float, // height
        float, // baseline
        AtlasMembers[], // atlases
        KeyValuePair<char, GlyphMembers>[], // glyphs
        KeyValuePair<uint, float>[] // kernings
    >;

    namespace UI
    {
        using FontDefinition = MyTuple<
            string, // Name
            float, // PtSize
            FontStyleDefinition[] // styles
        >;

        namespace Rendering.Server
        {
            using FontManagerMembers = MyTuple<
                MyTuple<Func<int, FontMembers>, Func<int>>, // Font List
                Func<FontDefinition, FontMembers?>, // TryAddFont
                Func<string, FontMembers?> // GetFont
            >;

            /// <summary>
            /// Stores the texture and font data for a given character.
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

            /// <summary>
            /// Expanded font interface. Used internally by the HUD API.
            /// </summary>
            public interface IFont : IFontMin
            {
                /// <summary>
                /// Gets the style for the given font; returns null if the style isn't defined.
                /// </summary>
                IFontStyle this[FontStyleEnum type] { get; }

                /// <summary>
                /// Gets the style for the given font; returns null if the style isn't defined.
                /// </summary>
                IFontStyle this[int index] { get; }

                /// <summary>
                /// Attempts to add a style to the font using a FontStyleDefinition
                /// </summary>
                bool TryAddStyle(FontStyleDefinition styleData);

                /// <summary>
                /// Attempts to add a style to the font
                /// </summary>
                bool TryAddStyle(int style, float height, float baseLine, AtlasMembers[] atlasData, KeyValuePair<char, GlyphMembers>[] glyphData, KeyValuePair<uint, float>[] kernData);
                FontMembers GetApiData();
            }

            /// <summary>
            /// Style for a given font. Used internally by the HUD API for rendering text from
            /// the font's sprites.
            /// </summary>
            public interface IFontStyle
            {
                /// <summary>
                /// Gets font the style is registered to.
                /// </summary>
                IFont Font { get; }

                /// <summary>
                /// Gets the <see cref="Glyph"/> associated with the given <see cref="char"/>.
                /// Returns □ (U+25A1) if the requested character is not defined for the style.
                /// </summary>
                Glyph this[char ch] { get; }

                /// <summary>
                /// Gets the style enum associated with the <see cref="IFontStyle"/>
                /// </summary>
                FontStyleEnum Style { get; }

                /// <summary>
                /// Position of the base line starting from the origin
                /// </summary>
                float BaseLine { get; }

                /// <summary>
                /// Glyph scale used to normalize the font size to 12pts
                /// </summary>
                float FontScale { get; }

                /// <summary>
                /// Line height
                /// </summary>
                float Height { get; }

                /// <summary>
                /// Size of the font as it appears in its textures.
                /// </summary>
                float PtSize { get; }

                Vector2I GetIndex();

                float GetKerningAdjustment(char left, char right);
            }

            public static class FontManager
            {
                public static IFontStyle Default => fonts[0][0];
                public static ReadOnlyCollection<IFont> Fonts { get; }

                private static readonly List<IFont> fonts;

                static FontManager()
                {
                    fonts = new List<IFont>();
                    Fonts = new ReadOnlyCollection<IFont>(fonts);
                }

                public static bool TryAddFont(string name, float ptSize)
                {
                    IFont font;
                    return TryAddFont(name, ptSize, out font);
                }

                public static bool TryAddFont(string name, float ptSize, out IFont font)
                {
                    if (!fonts.Exists(x => x.Name == name))
                    {
                        font = new Font(name, ptSize, fonts.Count);
                        fonts.Add(font);

                        return true;
                    }
                    else
                    {
                        font = null;
                        return false;
                    }
                }

                public static bool TryAddFont(FontDefinition fontData)
                {
                    IFont font;
                    return TryAddFont(fontData, out font);
                }

                public static bool TryAddFont(FontDefinition fontData, out IFont font)
                {
                    if (!fonts.Exists(x => x.Name == fontData.Item1))
                    {
                        font = new Font(fontData, fonts.Count);
                        fonts.Add(font);

                        return true;
                    }
                    else
                    {
                        font = null;
                        return false;
                    }
                }

                private static FontMembers? TryAddApiFont(FontDefinition fontData)
                {
                    IFont font;

                    if (TryAddFont(fontData, out font))
                        return font.GetApiData();
                    else
                        return null;
                }

                public static IFont GetFont(string name)
                {
                    for (int n = 0; n < fonts.Count; n++)
                        if (fonts[n].Name == name)
                            return fonts[n];

                    return null;
                }

                private static FontMembers? GetApiFont(string name)
                {
                    for (int n = 0; n < fonts.Count; n++)
                        if (fonts[n].Name == name)
                            return fonts[n].GetApiData();

                    return null;
                }

                public static FontManagerMembers GetApiData()
                {
                    return new FontManagerMembers()
                    {
                        Item1 = new MyTuple<Func<int, FontMembers>, Func<int>>(x => Fonts[x].GetApiData(), () => Fonts.Count),
                        Item2 = TryAddApiFont,
                        Item3 = GetApiFont
                    };
                }

                private class Font : IFont
                {
                    public IFontStyle this[int index] => styles[index];
                    public IFontStyle this[FontStyleEnum type] => styles[(int)type];

                    public string Name { get; }
                    public int Index { get; }
                    public float PtSize { get; }
                    public float BaseScale { get; }

                    private readonly FontStyle[] styles;

                    public Font(string name, float ptSize, int index)
                    {
                        Name = name;
                        PtSize = ptSize;
                        Index = index;

                        BaseScale = 12f / ptSize;
                        styles = new FontStyle[4];
                    }

                    public Font(FontDefinition fontData, int index) : this(fontData.Item1, fontData.Item2, index)
                    {
                        var styleData = fontData.Item3;

                        for (int n = 0; n < styleData.Length; n++)
                        {
                            if (styleData[n].Item4 != null)
                                TryAddStyle(styleData[n]);
                        }
                    }

                    public bool TryAddStyle(FontStyleDefinition styleData) =>
                        TryAddStyle(styleData.Item1, styleData.Item2, styleData.Item3, styleData.Item4, styleData.Item5, styleData.Item6);

                    public bool TryAddStyle(int style, float height, float baseLine, AtlasMembers[] atlasData, KeyValuePair<char, GlyphMembers>[] glyphData, KeyValuePair<uint, float>[] kernData)
                    {
                        if (styles[style] == null)
                        {
                            Material[] atlases = new Material[atlasData.Length];
                            Dictionary<char, Glyph> glyphs = new Dictionary<char, Glyph>(glyphData.Length);
                            Dictionary<uint, float> kerningPairs = new Dictionary<uint, float>(kernData.Length);

                            for (int n = 0; n < atlasData.Length; n++)
                                atlases[n] = new Material(atlasData[n].Item1, atlasData[n].Item2);

                            for (int n = 0; n < glyphData.Length; n++)
                            {
                                GlyphMembers v = glyphData[n].Value;
                                glyphs.Add(glyphData[n].Key, new Glyph(atlases[v.Item1], v.Item2, v.Item3, v.Item4, v.Item5));
                            }

                            for (int n = 0; n < kernData.Length; n++)
                                kerningPairs.Add(kernData[n].Key, kernData[n].Value);

                            styles[style] = new FontStyle(this, (FontStyleEnum)style, height, baseLine, glyphs, kerningPairs);
                            return true;
                        }
                        else
                            return false;
                    }

                    public bool IsStyleDefined(FontStyleEnum styleEnum) =>
                        styles[(int)styleEnum] != null;

                    public bool IsStyleDefined(int style) =>
                        styles[style] != null;

                    public FontMembers GetApiData()
                    {
                        return new FontMembers()
                        {
                            Item1 = Name,
                            Item2 = Index,
                            Item3 = PtSize,
                            Item4 = BaseScale,
                            Item5 = IsStyleDefined
                        };
                    }

                    private class FontStyle : IFontStyle
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
                        public IFont Font { get; }
                        public FontStyleEnum Style { get; }
                        public float PtSize => Font.PtSize;
                        public float Height { get; }
                        public float BaseLine { get; }
                        public float FontScale => Font.BaseScale;

                        private readonly Dictionary<char, Glyph> glyphs;
                        private readonly Dictionary<uint, float> kerningPairs;

                        public FontStyle(Font parent, FontStyleEnum style, float height, float baseline, Dictionary<char, Glyph> glyphs, Dictionary<uint, float> kerningPairs)
                        {
                            Font = parent;
                            Style = style;
                            Height = height;
                            BaseLine = baseline;
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

                        public Vector2I GetIndex() =>
                            new Vector2I(Font.Index, (int)Style);
                    }
                }
            }
        }
    }
}