using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Rendering.Client;
using System;
using System.Runtime.Remoting.Messaging;
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
            headerColor = new Color(41, 54, 62),
            bodyColor = new Color(70, 78, 86),
            highlightColor = TerminalFormatting.DarkSlateGrey,
            highlightFocusColor = TerminalFormatting.Mint,
            highlightTextColor = new Color(210, 190, 90),
            valueTextColor = new Color(210, 210, 210);
        public static GlyphFormat
            listHeaderFormat = new GlyphFormat(new Color(220, 235, 245), TextAlignment.Center, .9735f),
            wheelHeaderFormat = new GlyphFormat(Color.White, TextAlignment.Center, 1f, FontStyles.Underline),

            bodyFormat = new GlyphFormat(Color.White, textSize: .885f),
            bodyFormatCenter = bodyFormat.WithAlignment(TextAlignment.Center),

            wheelNameColor = new GlyphFormat(Color.White, textSize: 1f),
            wheelValueColor = new GlyphFormat(valueTextColor),
            valueFormat = bodyFormat.WithColor(valueTextColor),
            highlightFormat = bodyFormat.WithColor(highlightTextColor),
            selectedFormat = bodyFormat.WithColor(TerminalFormatting.Charcoal),
            dupeCrossFormat = bodyFormat.WithColor(new Color(0, 220, 30)),

            bodyValueFormatCenter = valueFormat.WithAlignment(TextAlignment.Center),
            nameFormatCenter = new GlyphFormat(bodyFormat.Color, TextAlignment.Center, 0.8f),
            valueFormatCenter = nameFormatCenter.WithColor(valueFormat.Color),
            selectedFormatCenter = nameFormatCenter.WithColor(selectedFormat.Color),

            footerFormatLeft = bodyFormat.WithColor(new Color(220, 235, 245)),
            footerFormatRight = footerFormatLeft.WithAlignment(TextAlignment.Right),
            blockIncFormat = footerFormatRight.WithColor(new Color(200, 35, 35));

        public const string textEntryWarning = "Open Chat to Enable Input";
        public const long notificationTime = 1500;
        public const int textTickDivider = 4;
        public const int maxEntryCharCount = 33;
        public const double floatPropLogThreshold = 1E6;
        public const float maxPeekWrapWidth = 270f,
            minPeekWrapWidth = 190f,
            wheelBodyPeekPadding = 43f,
            wheelBodyPadding = 90f,
            widgetInnerPadding = 4f,
            listMinWidth = 300f,
            listEntryHeight = 20f;

        public static void SetFont(string fontName)
        {
            IFontMin newFont = FontManager.GetFont(BvConfig.Current.genUI.fontName);

            if (newFont != null)
            {
                listHeaderFormat = listHeaderFormat.WithFont(newFont.Regular);
                wheelHeaderFormat = wheelHeaderFormat.WithFont(newFont.Underline);

                bodyFormat = bodyFormat.WithFont(newFont.Regular);
                bodyFormatCenter = bodyFormatCenter.WithFont(newFont.Regular);

                wheelNameColor = wheelNameColor.WithFont(newFont.Regular);
                wheelValueColor = wheelValueColor.WithFont(newFont.Regular);
                valueFormat = valueFormat.WithFont(newFont.Regular);
                highlightFormat = highlightFormat.WithFont(newFont.Regular);
                selectedFormat = selectedFormat.WithFont(newFont.Regular);
                dupeCrossFormat = dupeCrossFormat.WithFont(newFont.Regular);

                bodyValueFormatCenter = bodyValueFormatCenter.WithFont(newFont.Regular);
                nameFormatCenter = nameFormatCenter.WithFont(newFont.Regular);
                valueFormatCenter = valueFormatCenter.WithFont(newFont.Regular);
                selectedFormatCenter = selectedFormatCenter.WithFont(newFont.Regular);

                footerFormatLeft = footerFormatLeft.WithFont(newFont.Regular);
                footerFormatRight = footerFormatRight.WithFont(newFont.Regular);
                blockIncFormat = blockIncFormat.WithFont(newFont.Regular);
            }
        }
    }
}