using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using VRage;
using VRageMath;
using VRage.ModAPI;
using VRage.Utils;
using RichHudFramework;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Client;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private abstract class BlockValueWidgetBase : HudElementBase
        {
            protected readonly BorderedButton cancelButton, confirmButton;
            protected readonly HudChain buttonChain;

            protected Action CloseWidgetCallback;

            public BlockValueWidgetBase(HudParentBase parent = null) : base(parent)
            {
                cancelButton = new BorderedButton()
                {
                    Text = "Cancel",
                    Height = 40f,
                    Width = 150f,
                    Padding = Vector2.Zero,
                    TextPadding = Vector2.Zero,
                };
                confirmButton = new BorderedButton()
                {
                    Text = "Confirm",
                    Height = 40f,
                    Width = 150f,
                    Padding = Vector2.Zero,
                    TextPadding = Vector2.Zero,
                };

                buttonChain = new HudChain(false)
                {
                    ParentAlignment = ParentAlignments.Left | ParentAlignments.Inner,
                    Spacing = 8f,
                    CollectionContainer = { cancelButton, confirmButton }
                };

                DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding;
            }

            public abstract void SetMember(IBlockMember member, Action CloseWidgetCallback);

            public abstract void Reset();
        }
    }
}