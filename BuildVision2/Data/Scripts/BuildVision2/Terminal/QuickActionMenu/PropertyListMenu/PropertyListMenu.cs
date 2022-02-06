using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Diagnostics;
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
            /// Returns the current block target
            /// </summary>
            private PropertyBlock Target => quickActionMenu.Target;

            /// <summary>
            /// Returns true if the list is open
            /// </summary>
            public bool IsOpen { get; set; }

            private readonly LabelBox header;
            private readonly DoubleLabelBox footer;
            private readonly ScrollBox<PropertyListEntry, PropertyListEntryElement> body;
            private readonly HighlightBox highlightBox;
            private readonly HudChain layout;

            private readonly ObjectPool<PropertyListEntry> entryPool;
            private readonly Label debugText;
            private readonly Stopwatch listWrapTimer, notificationTimer;
            private string notification;
            private int textUpdateTick, selectionIndex;

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

                body = new ScrollBox<PropertyListEntry, PropertyListEntryElement>(true)
                {
                    MemberMinSize = new Vector2(300f, 0f),
                    SizingMode = HudChainSizingModes.ClampChainOffAxis | HudChainSizingModes.FitChainAlignAxis,
                    Padding = new Vector2(30f, 16f),
                    Color = bodyColor,
                    EnableScrolling = false,
                    UseSmoothScrolling = false,
                    MinVisibleCount = 10,
                };

                body.ScrollBar.Padding = new Vector2(12f, 16f);
                body.ScrollBar.Width = 4f;

                var border = new BorderBox(body)
                {
                    DimAlignment = DimAlignments.Both,
                    Color = new Color(58, 68, 77),
                    Thickness = 1f,
                };

                highlightBox = new HighlightBox(body.Background) 
                {
                    Padding = new Vector2(16f, 0f)
                };

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

                debugText = new Label(layout)
                {
                    ParentAlignment = ParentAlignments.Right,
                    BuilderMode = TextBuilderModes.Lined
                };

                entryPool = new ObjectPool<PropertyListEntry>(() => new PropertyListEntry(), x => x.Reset());
                notificationTimer = new Stopwatch();
                listWrapTimer = new Stopwatch();
            }

            public void OpenMenu()
            {
                if (!IsOpen)
                {
                    CloseMenu();
                    UpdateConfig();

                    for (int i = 0; i < Target.BlockMembers.Count; i++)
                    {
                        IBlockMember blockMember = Target.BlockMembers[i];

                        if (blockMember is IBlockColor)
                        {
                            // Assign an entry for each color channel
                            var colorMember = blockMember as IBlockColor;
                            var entry = entryPool.Get();
                            entry.SetMember(Target, colorMember.ColorChannels[0], i);
                            body.Add(entry);

                            entry = entryPool.Get();
                            entry.SetMember(Target, colorMember.ColorChannels[1], i);
                            body.Add(entry);

                            entry = entryPool.Get();
                            entry.SetMember(Target, colorMember.ColorChannels[2], i);
                            body.Add(entry);
                        }
                        else
                        {
                            var entry = entryPool.Get();
                            entry.SetMember(Target, blockMember, i);
                            body.Add(entry);
                        }
                    }

                    listWrapTimer.Restart();
                    IsOpen = true;
                }

                Visible = true;
            }

            public void HideMenu()
            {
                Visible = false;
            }

            public void CloseMenu()
            {
                entryPool.ReturnRange(body.Collection);
                body.Clear();
                selectionIndex = 0;
                Visible = false;
                IsOpen = false;
            }

            /// <summary>
            /// Shows a notification in the footer for a second or so
            /// </summary>
            public void ShowNotification(string notification)
            {
                this.notification = notification;
                notificationTimer.Restart();
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

                if (body.Collection.Count > 0)
                {
                    // Update visible range
                    if (selectionIndex > body.End)
                        body.End = selectionIndex;
                    else if (selectionIndex < body.Start)
                        body.Start = selectionIndex;

                    UpdateBodyText();
                    UpdateFooterText();

                    if (DrawDebug && textUpdateTick == 0)
                    {
                        UpdateDebugText();
                    }

                    debugText.Visible = DrawDebug;
                }

                textUpdateTick++;
                textUpdateTick %= textTickDivider;
            }

            protected override void Draw()
            {
                if (body.Collection.Count > 0)
                {
                    Vector2 bodySize = body.Size,
                        bodyPadding = body.Padding;

                    float memberWidth = 0f;
                    int visCount = 0;
                    var entries = body.Collection;

                    for (int i = 0; i < entries.Count && visCount < body.VisCount; i++)
                    {
                        int j = Math.Min(body.VisStart + i, entries.Count - 1);
                        HudElementBase element = entries[j].Element;

                        if (element.Visible)
                        {
                            memberWidth = Math.Max(memberWidth, element.Width);
                            visCount++;
                        }
                    }

                    memberWidth += bodyPadding.X + body.ScrollBar.Width + body.Divider.Width;
                    layout.Width = Math.Max(memberWidth, layout.MemberMinSize.X);

                    highlightBox.Size = new Vector2(
                        bodySize.X - body.Divider.Width - body.ScrollBar.Width,
                        body[selectionIndex].Element.Height + 2f
                    );
                    highlightBox.Offset = new Vector2(0f, body[selectionIndex].Element.Offset.Y - 1f);
                };
            }

            private void UpdateBodyText()
            {
                bool isDuplicatingProperties = (MenuState & QuickActionMenuState.PropertyDuplication) > 0;

                for (int i = 0; i < body.Collection.Count; i++)
                {
                    PropertyListEntry entry = body.Collection[i];

                    if (entry.Enabled && (textUpdateTick == 0 || i == selectionIndex))
                        entry.UpdateText(i == selectionIndex, isDuplicatingProperties);
                }
            }

            /// <summary>
            /// Updates footer text
            /// </summary>
            private void UpdateFooterText()
            {
                if (notification != null)
                {
                    footer.LeftTextBuilder.SetText($"[{notification}]");

                    if (notificationTimer.ElapsedMilliseconds > notificationTime)
                        notification = null;
                }
                else if ((MenuState & QuickActionMenuState.PropertyDuplication) > 0)
                {
                    int copyCount = 0;

                    for (int n = 0; n < body.Collection.Count; n++)
                    {
                        if (body.Collection[n].IsSelectedForDuplication)
                            copyCount++;
                    }

                    footer.LeftTextBuilder.SetText(
                        $"[Copying {Target.Duplicator.GetSelectedEntryCount()} " +
                        $"of {Target.Duplicator.GetValidEntryCount()}]", footerFormatLeft);
                }
                else
                    footer.LeftTextBuilder.SetText($"[{body.VisStart + 1} - {body.VisStart + body.VisCount} of {body.EnabledCount}]", footerFormatLeft);

                if (Target.IsWorking)
                    footer.RightTextBuilder.SetText("[Working]", footerFormatLeft);
                else if (Target.IsFunctional)
                    footer.RightTextBuilder.SetText("[Functional]", footerFormatRight);
                else
                    footer.RightTextBuilder.SetText("[Incomplete]", blockIncFormat);
            }

            private void UpdateDebugText()
            {
                ITextBuilder textBuilder = debugText.TextBoard;
                textBuilder.Clear();
                textBuilder.Append($"Selection: {body[selectionIndex].NameText}\n");
                textBuilder.Append($"ID: {body[selectionIndex].AssocMember.PropName}\n");
                textBuilder.Append($"Type: {body[selectionIndex].AssocMember.GetType().Name}\n");
                textBuilder.Append($"Value Text: {body[selectionIndex].AssocMember.ValueText}\n");
                textBuilder.Append($"Open: {body[selectionIndex].PropertyOpen}\n");
                textBuilder.Append($"Is Duplicating: {body[selectionIndex].IsSelectedForDuplication}\n");
                textBuilder.Append($"Chat Open: {BindManager.IsChatOpen}\n");
            }
        }
    }
}