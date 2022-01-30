using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using RichHudFramework;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Client;
using RichHudFramework.Internal;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private sealed class PropertyListMenu : HudElementBase
        {
            public bool ListOpen { get; private set; }

            private readonly LabelBox header;
            private readonly DoubleLabelBox footer;
            private readonly ScrollSelectionBox<PropertyListEntry, Label, IBlockMember> body;
            private readonly HudChain layout;

            public PropertyListMenu(HudParentBase parent = null) : base(parent)
            {
                header = new LabelBox()
                {
                    Format = headerText.WithStyle(FontStyles.Regular),
                    Text = "Build Vision",
                    AutoResize = false,
                    Size = new Vector2(300f, 34f),
                    Color = headerColor,
                };

                body = new ScrollSelectionBox<PropertyListEntry, Label, IBlockMember>()
                {
                    BackgroundColor = bodyColor,
                    Format = valueText,
                    LineHeight = 24f
                };

                var scrollbar = body.hudChain.ScrollBar;
                scrollbar.Padding = new Vector2(12f, 16f);
                scrollbar.Width = 4f;

                footer = new DoubleLabelBox()
                {
                    AutoResize = false,
                    TextPadding = new Vector2(48f, 0f),
                    Size = new Vector2(300f, 24f),
                    Color = headerColor,
                };

                layout = new HudChain(true, this)
                {
                    MemberMinSize = new Vector2(300f, 0f),
                    SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.FitChainBoth,
                    CollectionContainer = 
                    {
                        header,
                        body,
                        footer
                    }
                };

                body.SelectionChanged += Body_SelectionChanged;
            }

            private void Body_SelectionChanged(object sender, EventArgs e)
            {
                ExceptionHandler.SendChatMessage($"List Selection: {body?.Selection?.Element.TextBoard}");
            }

            public void SetBlockMembers(IReadOnlyList<IBlockMember> blockMembers)
            {
                CloseMenu();

                foreach (IBlockMember member in blockMembers)
                {
                    var entry = body.AddNew();
                    entry.SetMember(member);
                    entry.UpdateText();
                }

                Visible = true;
                ListOpen = true;
            }

            public void CloseMenu()
            {
                body.ClearEntries();
                Visible = false;
                ListOpen = false;
            }
        }
    }
}