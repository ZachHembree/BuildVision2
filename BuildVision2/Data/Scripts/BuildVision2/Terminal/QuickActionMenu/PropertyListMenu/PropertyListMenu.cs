using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Text;
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
using RichHudFramework.UI.Rendering.Client;

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
            private IPropertyBlock Target => quickActionMenu.Target;

            /// <summary>
            /// Returns true if the list is open
            /// </summary>
            public bool IsOpen { get; set; }

            private readonly LabelBox header;
            private readonly DoubleLabelBox footer;
            private readonly ScrollBox<PropertyListEntry, PropertyListEntryElement> listBody;
            private readonly LabelBox peekBody;

            private readonly HighlightBox highlightBox;
            private readonly HudChain layout;

            private readonly ObjectPool<PropertyListEntry> entryPool;
            private readonly Label debugText;
            private readonly Stopwatch listWrapTimer, notificationTimer;
            private readonly RichText peekBuilder;
            private StringBuilder notification;
            private bool contNotification;
            private int textUpdateTick, selectionIndex;

            public PropertyListMenu(QuickActionMenu parent) : base(parent)
            {
                quickActionMenu = parent;

                header = new LabelBox()
                {
                    Format = listHeaderFormat,
                    Text = BvMain.modName,
                    AutoResize = false,
                    Size = new Vector2(300f, 34f),
                    Color = headerColor,
                };

                listBody = new ScrollBox<PropertyListEntry, PropertyListEntryElement>(true)
                {
                    SizingMode = HudChainSizingModes.FitMembersOffAxis,
                    Padding = new Vector2(30f, 16f),
                    Color = bodyColor,
                    EnableScrolling = false,
                    UseSmoothScrolling = false,
                    Visible = false,
                };

                listBody.ScrollBar.Padding = new Vector2(12f, 16f);
                listBody.ScrollBar.Width = 4f;

                peekBody = new LabelBox()
                {
                    AutoResize = false,
                    VertCenterText = false,
                    Color = bodyColor,
                    TextPadding = new Vector2(48f, 16f),
                    BuilderMode = TextBuilderModes.Lined,
                };

                var border = new BorderBox(listBody)
                {
                    DimAlignment = DimAlignments.Both,
                    Color = new Color(58, 68, 77),
                    Thickness = 1f,
                };

                highlightBox = new HighlightBox(listBody.Background) 
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
                    SizingMode = HudChainSizingModes.FitMembersOffAxis,
                    CollectionContainer =
                    {
                        header,
                        { listBody, true },
                        { peekBody, true },
                        footer
                    }
                };

                debugText = new Label(layout)
                {
                    ParentAlignment = ParentAlignments.Right,
                    BuilderMode = TextBuilderModes.Lined
                };

                peekBuilder = new RichText();
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
                            listBody.Add(entry);

                            entry = entryPool.Get();
                            entry.SetMember(Target, colorMember.ColorChannels[1], i);
                            listBody.Add(entry);

                            entry = entryPool.Get();
                            entry.SetMember(Target, colorMember.ColorChannels[2], i);
                            listBody.Add(entry);
                        }
                        else if (blockMember is IBlockColorHSV)
                        {
                            // Assign an entry for each color channel
                            var colorMember = blockMember as IBlockColorHSV;
                            var entry = entryPool.Get();
                            entry.SetMember(Target, colorMember.ColorChannels[0], i);
                            listBody.Add(entry);

                            entry = entryPool.Get();
                            entry.SetMember(Target, colorMember.ColorChannels[1], i);
                            listBody.Add(entry);

                            entry = entryPool.Get();
                            entry.SetMember(Target, colorMember.ColorChannels[2], i);
                            listBody.Add(entry);
                        }
                        else
                        {
                            var entry = entryPool.Get();
                            entry.SetMember(Target, blockMember, i);
                            listBody.Add(entry);
                        }
                    }

                    listWrapTimer.Restart();
                    IsOpen = true;
                    peekBody.Visible = false;
                    listBody.Visible = true;
                }

                Visible = true;
            }

            public void OpenSummary()
            {
                peekBody.Visible = true;
                listBody.Visible = false;
                Visible = true;
                IsOpen = false;
            }

            public void HideMenu()
            {
                Visible = false;
            }

            public void CloseMenu()
            {
                entryPool.ReturnRange(listBody.Collection);
                listBody.Clear();
                selectionIndex = 0;
                Visible = false;
                IsOpen = false;
            }

            /// <summary>
            /// Shows a notification in the footer for a second or so
            /// </summary>
            public void ShowNotification(StringBuilder notification, bool continuous)
            {
                this.notification = notification;
                contNotification = continuous;
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
                listBody.SetMemberSize(new Vector2(0f, 20f));

                // Update colors
                float opacity = BvConfig.Current.genUI.hudOpacity;
                header.Color = header.Color.SetAlphaPct(opacity);
                listBody.Color = listBody.Color.SetAlphaPct(opacity);
                peekBody.Color = listBody.Color;
                footer.Color = footer.Color.SetAlphaPct(opacity);

                int maxVis = Math.Min(BvConfig.Current.genUI.listMaxVisible, listBody.EnabledCount),
                    visCount = 0;
                Vector2 listSize = Vector2.Zero;

                for (int i = listBody.Start; i < listBody.Count; i++)
                {
                    if (listBody[i].Enabled)
                    {
                        if (visCount < maxVis)
                        {
                            Vector2 memberSize = listBody[i].Element.Size;
                            listSize.X = MathHelper.Max(listSize.X, memberSize.X);
                            listSize.Y += memberSize.Y;

                            visCount++;
                        }
                        else
                            break; 
                    }
                }

                listBody.UnpaddedSize = listSize;
                peekBody.Size = Vector2.Max(peekBody.TextBoard.TextSize + peekBody.TextPadding, new Vector2(300f, 0f));

                Vector2 layoutSize = layout.GetRangeSize();
                layoutSize.X = Math.Max(listBody.Width, 300f);
                layout.Size = layoutSize;
                Size = layoutSize;

                if (listBody.Count > 0)
                {
                    // Update visible range
                    if (selectionIndex > listBody.End)
                        listBody.End = selectionIndex;
                    else if (selectionIndex < listBody.Start)
                        listBody.Start = selectionIndex;

                    if (DrawDebug && textUpdateTick == 0)
                        UpdateDebugText();
                }

                UpdateBodyText();
                UpdateFooterText();

                debugText.Visible = DrawDebug;

                textUpdateTick++;
                textUpdateTick %= textTickDivider;
            }

            protected override void Draw()
            {
                if (listBody.Count > 0)
                {
                    Vector2 bodySize = listBody.Size;

                    float memberWidth = 0f;
                    int visCount = 0;
                    var entries = listBody.Collection;

                    for (int i = 0; i < entries.Count && visCount < listBody.VisCount; i++)
                    {
                        int j = Math.Min(listBody.VisStart + i, entries.Count - 1);
                        HudElementBase element = entries[j].Element;

                        if (element.Visible)
                        {
                            memberWidth = Math.Max(memberWidth, element.Width);
                            visCount++;
                        }
                    }

                    highlightBox.Size = new Vector2(
                        bodySize.X - listBody.Divider.Width - listBody.ScrollBar.Width,
                        listBody[selectionIndex].Element.Height + 2f
                    );
                    highlightBox.Offset = new Vector2(0f, listBody[selectionIndex].Element.Offset.Y - 1f);
                };
            }

            private void UpdateBodyText()
            {
                if ((MenuState & QuickActionMenuState.ListPeek) == QuickActionMenuState.ListPeek)
                {
                    if (textUpdateTick == 0)
                    {
                        peekBuilder.Clear();
                        quickActionMenu.Target.GetSummary(peekBuilder, bodyFormat, valueFormat);
                        peekBody.TextBoard.SetText(peekBuilder);
                    }
                }
                else if (listBody.Collection.Count > 0)
                {
                    bool isDuplicatingProperties = (MenuState & QuickActionMenuState.PropertyDuplication) > 0;

                    for (int i = 0; i < listBody.Collection.Count; i++)
                    {
                        PropertyListEntry entry = listBody.Collection[i];

                        if (entry.Enabled && (textUpdateTick == 0 || i == selectionIndex))
                            entry.UpdateText(i == selectionIndex, isDuplicatingProperties);
                    }
                }
            }

            /// <summary>
            /// Updates footer text
            /// </summary>
            private void UpdateFooterText()
            {
                if (notification != null && notificationTimer.ElapsedMilliseconds < notificationTime)
                {
                    footer.LeftTextBuilder.Clear();
                    footer.LeftTextBuilder.Append("[", footerFormatLeft);
                    footer.LeftTextBuilder.Append(notification, footerFormatLeft);
                    footer.LeftTextBuilder.Append("]", footerFormatLeft);

                    if (contNotification)
                    {
                        notification = null;
                        contNotification = false;
                    }
                }
                else if ((MenuState & QuickActionMenuState.PropertyDuplication) > 0)
                {
                    int copyCount = 0;

                    for (int n = 0; n < listBody.Collection.Count; n++)
                    {
                        if (listBody.Collection[n].IsSelectedForDuplication)
                            copyCount++;
                    }

                    footer.LeftTextBuilder.SetText(
                        $"[Copying {Target.Duplicator.GetSelectedEntryCount()} " +
                        $"of {Target.Duplicator.GetValidEntryCount()}]", footerFormatLeft);
                }
                else if ((MenuState & QuickActionMenuState.Peek) > 0)
                    footer.LeftTextBuilder.SetText("[Peeking]", footerFormatLeft);
                else
                    footer.LeftTextBuilder.SetText($"[{listBody.VisStart + 1} - {listBody.VisStart + listBody.VisCount} of {listBody.EnabledCount}]", footerFormatLeft);

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
                textBuilder.Append($"Selection: {listBody[selectionIndex].NameText}\n");
                textBuilder.Append($"ID: {listBody[selectionIndex].AssocMember.PropName}\n");
                textBuilder.Append($"Type: {listBody[selectionIndex].AssocMember.GetType().Name}\n");
                textBuilder.Append($"Value Text: {listBody[selectionIndex].AssocMember.ValueText}\n");
                textBuilder.Append($"Open: {listBody[selectionIndex].PropertyOpen}\n");
                textBuilder.Append($"Is Duplicating: {listBody[selectionIndex].IsSelectedForDuplication}\n");

                if (listBody[selectionIndex].AssocMember is IBlockComboBox)
                {
                    var member = listBody[selectionIndex].AssocMember as IBlockComboBox;
                    textBuilder.Append($"Value: {member.Value}\n");

                    for (int i = 0; i < member.ComboEntries.Count; i++)
                    {
                        textBuilder.Append($"Entry {member.ComboEntries[i].Key}: {member.ComboEntries[i].Value}\n");
                    }
                }
            }
        }
    }
}