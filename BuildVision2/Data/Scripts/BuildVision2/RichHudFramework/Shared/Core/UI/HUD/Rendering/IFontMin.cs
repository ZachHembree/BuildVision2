using System;

namespace RichHudFramework.UI.Rendering
{
    [Flags]
    public enum FontStyleEnum : int
    {
        Regular = 0,
        Bold = 1,
        Italic = 2,
        BoldItalic = 3
    }

    /// <summary>
    /// Simplified Font interface for use by HUD API clients.
    /// </summary>
    public interface IFontMin
    {
        string Name { get; }
        int Index { get; }

        /// <summary>
        /// Font size at which the textures were created.
        /// </summary>
        float PtSize { get; }

        /// <summary>
        /// Default scaling applied to font.
        /// </summary>
        float BaseScale { get; }

        /// <summary>
        /// Returns true if the font is defined for the given style.
        /// </summary>
        bool IsStyleDefined(FontStyleEnum styleEnum);

        /// <summary>
        /// Returns true if the font is defined for the given style.
        /// </summary>
        bool IsStyleDefined(int style);
    }
}