﻿using RichHudFramework.Client;
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
    [Flags]
    public enum QuickActionMenuState : int
    {
        Closed = 0x0,

        WheelMenuControl = 0x1,
        WidgetControl = WheelMenuControl | 0x2,

        ListMenuControl = 0x4,

        PropertyDuplication = 0x8,
    }

    public sealed partial class QuickActionMenu : HudElementBase
    {
        private static readonly Color
            headerColor = new Color(41, 54, 62, 225),
            bodyColor = new Color(70, 78, 86, 225),
            selectionBoxColor = new Color(41, 54, 62, 225);
        private static readonly GlyphFormat
            listHeaderFormat = new GlyphFormat(new Color(220, 235, 245), TextAlignment.Center, .9735f),
            mainHeaderFormat = new GlyphFormat(Color.White, TextAlignment.Center, 1f, FontStyles.Underline),
            bodyFormat = new GlyphFormat(Color.White, textSize: .885f),
            valueFormat = bodyFormat.WithColor(new Color(210, 210, 210)),
            bodyFormatCenter = bodyFormat.WithAlignment(TextAlignment.Center),
            valueFormatCenter = valueFormat.WithAlignment(TextAlignment.Center),
            footerFormatLeft = bodyFormat.WithColor(new Color(220, 235, 245)),
            footerFormatRight = footerFormatLeft.WithAlignment(TextAlignment.Right),
            dupeCrossFormat = bodyFormat.WithColor(new Color(0, 30, 220)),
            highlightFormat = bodyFormat.WithColor(new Color(220, 180, 50)),
            selectedFormat = bodyFormat.WithColor(new Color(50, 200, 50)),
            blockIncFormat = footerFormatRight.WithColor(new Color(200, 35, 35));

        private const int textTickDivider = 4;
    }
}