using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
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
        private partial class PropertyListMenu : HudElementBase
        {
            /// <summary>
            /// Parent of the wheel menu
            /// </summary>
            public readonly QuickActionMenu quickActionMenu;

            /// <summary>
            /// Gets/sets the menu's state
            /// </summary>
            private QuickActionMenuState MenuState
            {
                get { return quickActionMenu.MenuState; }
                set { quickActionMenu.MenuState = value; }
            }

            /// <summary>
            /// Returns the current block property duplicator
            /// </summary>
            private BlockPropertyDuplicator Duplicator => quickActionMenu.Duplicator;

            /// <summary>
            /// Returns true if the list is open
            /// </summary>
            public bool IsOpen => Visible;

            private readonly LabelBox header;
            private readonly DoubleLabelBox footer;
            private readonly ScrollSelectionBox<PropertyListEntry, PropertyListEntryElement, IBlockMember> body;
            private readonly HudChain layout;

            private readonly Label debugText;
            private readonly RichText textBuf;
            private int textUpdateTick;

            public PropertyListMenu(QuickActionMenu parent) : base(parent)
            {
                quickActionMenu = parent;

                header = new LabelBox()
                {
                    Format = listHeaderFormat,
                    Text = "Build Vision",
                    AutoResize = false,
                    Size = new Vector2(300f, 34f),
                    Color = headerColor,
                };

                body = new ScrollSelectionBox<PropertyListEntry, PropertyListEntryElement, IBlockMember>()
                {
                    Color = bodyColor,
                    Format = bodyFormat,
                    LineHeight = 19f,
                    MemberPadding = Vector2.Zero,
                    HighlightPadding = new Vector2(4f, 0),
                    ListPadding = new Vector2(30f, 16f),
                    IsMasking = false,
                    EnableScrolling = false,
                    UseSmoothScrolling = false,
                    MinVisibleCount = 10,
                    SizingMode = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.FitChainBoth
                };

                body.MouseInput.Enabled = false;
                body.hudChain.MemberMinSize = new Vector2(300f, 0f);

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

                textBuf = new RichText();
                debugText = new Label(layout)
                {
                    ParentAlignment = ParentAlignments.Right,
                    BuilderMode = TextBuilderModes.Lined
                };
            }

            public void OpenMenu()
            {
                CloseMenu();
                UpdateConfig();

                for (int i = 0; i < Duplicator.BlockMembers.Count; i++)
                {
                    IBlockMember blockMember = Duplicator.BlockMembers[i];

                    if (blockMember is IBlockColor)
                    {
                        // Assign an entry for each color channel
                        var colorMember = blockMember as IBlockColor;
                        var entry = body.AddNew();
                        entry.SetMember(i, Duplicator);

                        entry = body.AddNew();
                        entry.SetMember(i, Duplicator);

                        entry = body.AddNew();
                        entry.SetMember(i, Duplicator);
                    }
                    else
                    {
                        var entry = body.AddNew();
                        entry.SetMember(i, Duplicator);
                    }
                }

                Visible = true;
            }

            public void CloseMenu()
            {
                body.ClearEntries();
                Visible = false;
            }

            private void UpdateConfig()
            {
                incrZ = BvConfig.Current.block.colorMult.Z; // x64
                incrY = BvConfig.Current.block.colorMult.Y; // x16
                incrX = BvConfig.Current.block.colorMult.X; // x8
            }

            protected override void Layout()
            {
                Size = layout.Size;

                if (body.EntryList.Count > 0)
                {
                    if (body.SelectionIndex > body.hudChain.End)
                        body.hudChain.End = body.SelectionIndex;
                    else if (body.SelectionIndex < body.hudChain.Start)
                        body.hudChain.Start = body.SelectionIndex;

                    textBuf.Clear();
                    textBuf.Add($"Selection: {body.Selection?.NameText}\n");
                    textBuf.Add($"Selection Open: {body.Selection?.PropertyOpen}\n");
                    textBuf.Add($"Selection Type: {body.Selection?.AssocMember.GetType().Name}\n");
                    textBuf.Add($"Selection Value Text: {body.Selection?.AssocMember.ValueText}\n");
                    textBuf.Add($"Chat Open: {BindManager.IsChatOpen}\n");
                    debugText.Text = textBuf;

                    bool isDuplicatingProperties = (MenuState & QuickActionMenuState.PropertyDuplication) > 0;

                    foreach (PropertyListEntry entry in body)
                    {
                        if (entry.Enabled && (textUpdateTick == 0 || entry == body.Selection))
                            entry.UpdateText(entry == body.Selection, isDuplicatingProperties);
                    }

                    textUpdateTick++;
                    textUpdateTick %= textTickDivider;
                }
            }
        }
    }
}