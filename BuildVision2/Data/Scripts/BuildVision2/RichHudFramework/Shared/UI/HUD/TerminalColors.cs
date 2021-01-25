using System;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Contains colors and text formatting used in styling the Rich HUD Terminal
    /// </summary>
    public static class TerminalFormatting
    {
        public static readonly GlyphFormat HeaderFormat = new GlyphFormat(Color.White, TextAlignment.Center, 1.15f);
        public static readonly GlyphFormat ControlFormat = GlyphFormat.Blueish.WithSize(1.08f);
        public static readonly GlyphFormat WarningFormat = new GlyphFormat(new Color(200, 55, 55));

        public static readonly Color ScrollBarColor = new Color(41, 51, 61);
        public static readonly Color TileColor = new Color(39, 50, 57);
        public static readonly Color ListHeaderColor = new Color(32, 39, 45);
        public static readonly Color ListBgColor = new Color(41, 54, 62);
        public static readonly Color BorderColor = new Color(53, 66, 75);
        public static readonly Color SelectionBgColor = new Color(34, 44, 53);
        public static readonly Color HighlightOverlayColor = new Color(255, 255, 255, 40);
        public static readonly Color HighlightColor = new Color(214, 213, 218);
        public static readonly Color AccentHighlightColor = new Color(181, 185, 190);
    }
}
