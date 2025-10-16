using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Rendering.Client;
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
        WheelShortcutOpened = 0x40,

        ListPeek = Peek | ListMenuOpen,
        WheelPeek = Peek | WheelMenuOpen,

        WheelMenuControl = WheelMenuOpen | Controlled,
        WidgetControl = WheelMenuControl | PropertyOpen,
        ListMenuControl = ListMenuOpen | Controlled,
        ListPropertyControl = ListMenuControl | PropertyOpen,
    }

    public sealed partial class QuickActionMenu : HudElementBase
    {
        public static readonly Color
            HeaderColor = new Color(41, 54, 62),
            BodyColor = new Color(70, 78, 86),
            HighlightColor = TerminalFormatting.DarkSlateGrey,
            HighlightFocusColor = TerminalFormatting.Mint,
            HighlightTextColor = new Color(210, 190, 90),
            ValueTextColor = new Color(210, 210, 210);
        public static GlyphFormat
            ListHeaderFormat = new GlyphFormat(new Color(220, 235, 245), TextAlignment.Center, .9735f),
            WheelHeaderFormat = new GlyphFormat(Color.White, TextAlignment.Center, 1f, FontStyles.Underline),

            BodyFormat = new GlyphFormat(Color.White, textSize: .885f),
            BodyFormatCenter = BodyFormat.WithAlignment(TextAlignment.Center),

            WheelNameColor = new GlyphFormat(Color.White, textSize: 1f),
            WheelValueColor = new GlyphFormat(ValueTextColor),
            ValueFormat = BodyFormat.WithColor(ValueTextColor),
            HighlightFormat = BodyFormat.WithColor(HighlightTextColor),
            SelectedFormat = BodyFormat.WithColor(TerminalFormatting.Charcoal),
            DupeCrossFormat = BodyFormat.WithColor(new Color(0, 220, 30)),

            BodyValueFormatCenter = ValueFormat.WithAlignment(TextAlignment.Center),
            NameFormatCenter = new GlyphFormat(BodyFormat.Color, TextAlignment.Center, 0.8f),
            ValueFormatCenter = NameFormatCenter.WithColor(ValueFormat.Color),
            SelectedFormatCenter = NameFormatCenter.WithColor(SelectedFormat.Color),

            FooterFormatLeft = BodyFormat.WithColor(new Color(220, 235, 245)),
            FooterFormatRight = FooterFormatLeft.WithAlignment(TextAlignment.Right),
            BlockIncFormat = FooterFormatRight.WithColor(new Color(200, 35, 35));

        public const string TextEntryWarning = "Open Chat to Enable Input";
        public const long NotificationTime = 1500;
        public const int TextTickDivider = 4;
        public const int MaxEntryCharCount = 33;
        public const double FloatPropLogThreshold = 1E6;
        public const float Sqrt2 = 1.414213f,
			RcpSqrt2 = 0.7071067f,
			WheelOuterDiam = 512f,
            WheelInnerDiamScale = 0.6f,
            WheelBodyMaxDiam = 1.05f * WheelInnerDiamScale * WheelOuterDiam,
            MaxWheelPeekPadding = (1.0f - RcpSqrt2) * WheelBodyMaxDiam, // maxSize - (sqrt(2) * maxSize)
			MaxWheelPeekWrap = WheelBodyMaxDiam - MaxWheelPeekPadding,
            MinWheelPeekWrapWidth = 190f,
            WheelNotifiationWidth = 150f,
            WheelBodyPeekPadding = 43f,
            WidgetInnerPadding = 4f,
            ListMinWidth = 296f,
            ListEntryHeight = 19f;

        public static void SetFont(string fontName)
        {
            IFontMin newFont = FontManager.GetFont(BvConfig.Current.genUI.fontName);

            if (newFont != null)
            {
                ListHeaderFormat = ListHeaderFormat.WithFont(newFont.Regular);
                WheelHeaderFormat = WheelHeaderFormat.WithFont(newFont.Underline);

                BodyFormat = BodyFormat.WithFont(newFont.Regular);
                BodyFormatCenter = BodyFormatCenter.WithFont(newFont.Regular);

                WheelNameColor = WheelNameColor.WithFont(newFont.Regular);
                WheelValueColor = WheelValueColor.WithFont(newFont.Regular);
                ValueFormat = ValueFormat.WithFont(newFont.Regular);
                HighlightFormat = HighlightFormat.WithFont(newFont.Regular);
                SelectedFormat = SelectedFormat.WithFont(newFont.Regular);
                DupeCrossFormat = DupeCrossFormat.WithFont(newFont.Regular);

                BodyValueFormatCenter = BodyValueFormatCenter.WithFont(newFont.Regular);
                NameFormatCenter = NameFormatCenter.WithFont(newFont.Regular);
                ValueFormatCenter = ValueFormatCenter.WithFont(newFont.Regular);
                SelectedFormatCenter = SelectedFormatCenter.WithFont(newFont.Regular);

                FooterFormatLeft = FooterFormatLeft.WithFont(newFont.Regular);
                FooterFormatRight = FooterFormatRight.WithFont(newFont.Regular);
                BlockIncFormat = BlockIncFormat.WithFont(newFont.Regular);
            }
        }
    }
}