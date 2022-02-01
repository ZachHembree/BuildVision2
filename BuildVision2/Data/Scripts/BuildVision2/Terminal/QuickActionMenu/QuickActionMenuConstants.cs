using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Text;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using RichHudFramework;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Client;

namespace DarkHelmet.BuildVision2
{
    public enum QuickActionMenuState
    {
        Closed,
        PropertySelection,
        WidgetControl,
        ListMenuControl,
        PropertyDuplication,
    }

    public sealed partial class QuickActionMenu : HudElementBase
    {
        private static readonly Color
            headerColor = new Color(41, 54, 62, 225),
            bodyColor = new Color(70, 78, 86, 225),
            selectionBoxColor = new Color(41, 54, 62, 225);
        private static readonly GlyphFormat
            listHeaderText = new GlyphFormat(new Color(220, 235, 245), TextAlignment.Center, .9735f),
            headerText = new GlyphFormat(Color.White, TextAlignment.Center, 1f, FontStyles.Underline),
            bodyText = new GlyphFormat(Color.White, textSize: .885f),
            valueText = bodyText.WithColor(new Color(210, 210, 210)),
            bodyTextCenter = bodyText.WithAlignment(TextAlignment.Center),
            valueTextCenter = valueText.WithAlignment(TextAlignment.Center),
            footerTextLeft = bodyText.WithColor(new Color(220, 235, 245)),
            footerTextRight = footerTextLeft.WithAlignment(TextAlignment.Right),
            highlightText = bodyText.WithColor(new Color(220, 180, 50)),
            selectedText = bodyText.WithColor(new Color(50, 200, 50)),
            blockIncText = footerTextRight.WithColor(new Color(200, 35, 35));

        private const int textTickDivider = 4;
    }
}