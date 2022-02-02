using RichHudFramework.Client;
using RichHudFramework.IO;
using System;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using RichHudFramework.UI;
using RichHudFramework;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Client;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private abstract class BlockValueWidgetBase : QuickActionWidgetBase
        {
            public BlockValueWidgetBase(HudParentBase parent) : base(parent)
            { }
        }
    }
}