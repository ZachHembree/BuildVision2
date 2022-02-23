using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using System;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    [Flags]
    public enum QuickActionMenuState : int
    {
        Closed = 0x0,
        Peek = 0x1,

        WheelMenuOpen = 0x2,
        ListMenuOpen = 0x4,
        PropertyOpen = 0x8,
        PropertyDuplication = 0x10,
        Controlled = 0x20,
        LegacyMode = 0x40,

        WheelMenuControl = WheelMenuOpen | Controlled,
        WidgetControl = WheelMenuControl | PropertyOpen,
        ListMenuControl = ListMenuOpen | Controlled,
        PropertyControl = ListMenuControl | PropertyOpen,
    }

    public sealed partial class QuickActionMenu : HudElementBase
    {
        private static readonly Color
            headerColor = new Color(41, 54, 62),
            bodyColor = new Color(70, 78, 86),
            highlightColor = TerminalFormatting.DarkSlateGrey,
            highlightFocusColor = TerminalFormatting.Mint;
        private static readonly GlyphFormat
            listHeaderFormat = new GlyphFormat(new Color(220, 235, 245), TextAlignment.Center, .9735f),
            mainHeaderFormat = new GlyphFormat(Color.White, TextAlignment.Center, 1f, FontStyles.Underline),

            bodyFormat = new GlyphFormat(Color.White, textSize: .885f),
            bodyFormatCenter = bodyFormat.WithAlignment(TextAlignment.Center),

            valueFormat = bodyFormat.WithColor(new Color(210, 210, 210)),
            highlightFormat = bodyFormat.WithColor(new Color(210, 190, 90)),
            selectedFormat = bodyFormat.WithColor(TerminalFormatting.Charcoal),
            dupeCrossFormat = bodyFormat.WithColor(new Color(0, 220, 30)),

            bodyValueFormatCenter = valueFormat.WithAlignment(TextAlignment.Center),
            nameFormatCenter = new GlyphFormat(bodyFormat.Color, TextAlignment.Center, 0.8f),
            valueFormatCenter = nameFormatCenter.WithColor(valueFormat.Color),
            selectedFormatCenter = nameFormatCenter.WithColor(selectedFormat.Color),

            footerFormatLeft = bodyFormat.WithColor(new Color(220, 235, 245)),
            footerFormatRight = footerFormatLeft.WithAlignment(TextAlignment.Right),
            blockIncFormat = footerFormatRight.WithColor(new Color(200, 35, 35));

        private const string textEntryWarning = "Open Chat to Enable Input";
        private const long notificationTime = 1500;
        private const int textTickDivider = 4;
        private const int maxEntryCharCount = 33;
    }
}