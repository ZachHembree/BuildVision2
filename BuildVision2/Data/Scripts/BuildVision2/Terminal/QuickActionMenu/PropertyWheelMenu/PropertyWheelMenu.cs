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
using RichHudFramework.Internal;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu : HudElementBase
    {
        private sealed partial class PropertyWheelMenu : HudElementBase
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
            /// Returns the current target block
            /// </summary>
            private PropertyBlock Target => quickActionMenu.Target;

            /// <summary>
            /// Returns true if the menu is open and visible
            /// </summary>
            public bool IsOpen { get; set; }

            /// <summary>
            /// Returns true if a property widget is currently open
            /// </summary>
            public bool IsWidgetOpen => menuBody.IsWidgetOpen;

            private readonly RadialSelectionBox<PropertyWheelEntryBase, Label> propertyWheel, dupeWheel;
            private readonly PropertyWheelMenuBody menuBody;
            private readonly Label debugText;

            private readonly ObjectPool<object> propertyEntryPool;
            private readonly List<PropertyWheelShortcutEntry> shortcutEntries;
            private RadialSelectionBox<PropertyWheelEntryBase, Label> activeWheel;
            private readonly StringBuilder notifText;
            private int textUpdateTick;

            public PropertyWheelMenu(QuickActionMenu parent) : base(parent)
            {
                this.quickActionMenu = parent;
                menuBody = new PropertyWheelMenuBody(this) { };

                // Selection wheel for block properties
                propertyWheel = new RadialSelectionBox<PropertyWheelEntryBase, Label>(menuBody)
                {
                    Visible = false,
                    BackgroundColor = bodyColor,
                    HighlightColor = highlightColor,
                    ZOffset = -1,
                };

                // Selection wheel for dupe shortcuts
                dupeWheel = new RadialSelectionBox<PropertyWheelEntryBase, Label>(menuBody)
                {
                    Visible = false,
                    BackgroundColor = bodyColor,
                    HighlightColor = highlightColor,
                    ZOffset = -1,
                    CollectionContainer =
                    {
                        new PropertyWheelShortcutEntry()
                        {
                            Text = "Back",
                            ShortcutAction = StopPropertyDuplication,
                        },
                        new PropertyWheelShortcutEntry()
                        {
                            Text = "Open List",
                            ShortcutAction = OpenDupeList,
                        },
                        new PropertyWheelShortcutEntry()
                        {
                            Text = "Open List and Select All",
                            ShortcutAction = OpenDupeListAndSelectAll,
                        },
                        new PropertyWheelShortcutEntry()
                        {
                            Text = "Clear Selection",
                            ShortcutAction = ClearSelection,
                        },
                        new PropertyWheelShortcutEntry()
                        {
                            Text = "Copy Selected",
                            ShortcutAction = CopySelectedProperties,
                        },
                        new PropertyWheelShortcutEntry()
                        {
                            Text = "Copy All but Name",
                            ShortcutAction = () => CopyAllProperties(false),
                        },
                        new PropertyWheelShortcutEntry()
                        {
                            Text = "Copy All",
                            ShortcutAction = () => CopyAllProperties(true),
                        },
                        new PropertyWheelShortcutEntry()
                        {
                            Text = "Paste",
                            ShortcutAction = PasteCopiedProperties,
                        },
                        new PropertyWheelShortcutEntry()
                        {
                            Text = "Undo",
                            ShortcutAction = UndoPropertyPaste,
                        },
                    }
                };

                // Shortcuts to be added to the property wheel later
                shortcutEntries = new List<PropertyWheelShortcutEntry>()
                {
                    new PropertyWheelShortcutEntry()
                    {
                        Text = "Copy Settings",
                        ShortcutAction = StartPropertyDuplication,
                    }
                };

                // I'm using a generic pool because I'm using two types of entry in the same list, but only
                // one is pooled, and I can't be arsed to do this more neatly.
                propertyEntryPool = new ObjectPool<object>(
                    () => new PropertyWheelEntry(),
                    x => (x as PropertyWheelEntry).Reset()
                );
                notifText = new StringBuilder();

                debugText = new Label(this)
                {
                    Visible = false,
                    BuilderMode = TextBuilderModes.Lined,
                    ParentAlignment = ParentAlignments.Right
                };
            }

            /// <summary>
            /// Adds a permanent shortcut to the property wheel
            /// </summary>
            public void RegisterShortcut(PropertyWheelShortcutEntry shortcut)
            {
                shortcutEntries.Add(shortcut);
            }

            /// <summary>
            /// Opens the menu and populates it with properties from the given property block
            /// </summary>
            public void OpenMenu()
            {
                if (!IsOpen)
                {
                    Clear();

                    // Add entries for block members
                    for (int i = 0; i < Target.BlockMembers.Count; i++)
                    {
                        var entry = propertyEntryPool.Get() as PropertyWheelEntry;
                        entry.SetMember(i, Target);
                        propertyWheel.Add(entry);
                    }

                    // Append registered shortcuts to end
                    propertyWheel.AddRange(shortcutEntries);               
                }

                propertyWheel.IsInputEnabled = true;
                propertyWheel.Visible = true;
                dupeWheel.Visible = false;
                IsOpen = true;
                Visible = true;
            }

            public void OpenSummary()
            {
                propertyWheel.Visible = false;
                dupeWheel.Visible = false;
                Visible = true;
            }

            public void HideMenu()
            {
                Visible = false;
                propertyWheel.Visible = false;
                dupeWheel.Visible = false;
            }

            public void CloseMenu()
            {
                Clear();
                HideMenu();
                IsOpen = false;
            }

            /// <summary>
            /// Clears all entries from the menu and closes any open widgets
            /// </summary>
            private void Clear()
            {
                menuBody.CloseWidget();

                propertyEntryPool.ReturnRange(propertyWheel.EntryList, 0,
                    propertyWheel.EntryList.Count - shortcutEntries.Count);

                propertyWheel.SetSelectionAt(0);
                propertyWheel.Clear();
                propertyWheel.IsInputEnabled = false;
            }

            protected override void Layout()
            {
                float opacity = BvConfig.Current.hudConfig.hudOpacity;
                menuBody.background.Color = menuBody.background.Color.SetAlphaPct(opacity);
                propertyWheel.BackgroundColor = propertyWheel.BackgroundColor.SetAlphaPct(opacity);
                dupeWheel.BackgroundColor = dupeWheel.BackgroundColor.SetAlphaPct(opacity);

                if (MenuState == QuickActionMenuState.Peek)
                {
                    Size = menuBody.Size;
                }
                else
                {
                    Size = propertyWheel.Size;
                    menuBody.Size = 1.05f * propertyWheel.Size * propertyWheel.polyBoard.InnerRadius;

                    if ((MenuState & QuickActionMenuState.WidgetControl) == QuickActionMenuState.WidgetControl)
                    {
                        propertyWheel.HighlightColor = highlightFocusColor;
                    }
                    else
                    {
                        propertyWheel.HighlightColor = highlightColor;
                    }

                    if (textUpdateTick == 0 && (MenuState & QuickActionMenuState.PropertyDuplication) == 0)
                    {
                        foreach (PropertyWheelEntryBase baseEntry in propertyWheel)
                        {
                            var entry = baseEntry as PropertyWheelEntry;

                            if (entry != null && entry.Enabled)
                                entry.UpdateText(entry == propertyWheel.Selection 
                                    && (MenuState & QuickActionMenuState.WidgetControl) == QuickActionMenuState.WidgetControl);
                        }
                    }
                }

                if (textUpdateTick == 0)
                {
                    int selectedEntryCount = Target.Duplicator.GetSelectedEntryCount();

                    if (selectedEntryCount > 0)
                    {
                        notifText.Clear();
                        notifText.Append(selectedEntryCount);
                        notifText.Append(" properties selected");
                        menuBody.ShowNotification(notifText, true);
                    }

                    debugText.Visible = DrawDebug;

                    if (DrawDebug)
                        UpdateDebugText();
                }

                textUpdateTick++;
                textUpdateTick %= textTickDivider;
            }

            private void UpdateDebugText()
            {             
                ITextBuilder textBuilder = debugText.TextBoard;
                PropertyWheelEntryBase selection = activeWheel.Selection;
                var propertyEntry = selection as PropertyWheelEntry;

                textBuilder.Clear();

                if (selection != null)
                {
                    textBuilder.Append($"Prioritized Members: {Target.Prioritizer.PrioritizedMemberCount}\n");
                    textBuilder.Append($"Enabled Members: {Target.EnabledMemberCount}\n");
                    textBuilder.Append($"Selection: {selection.Element.TextBoard}\n");

                    if (propertyEntry != null)
                    {
                        textBuilder.Append($"ID: {propertyEntry.BlockMember.PropName}\n");
                        textBuilder.Append($"Type: {propertyEntry.BlockMember.GetType().Name}\n");
                        textBuilder.Append($"Entry Enabled: {propertyEntry.Enabled}\n");
                        textBuilder.Append($"Prop Enabled: {propertyEntry.BlockMember.Enabled}\n");
                        textBuilder.Append($"Value Text: {propertyEntry.BlockMember.ValueText}\n");
                        textBuilder.Append($"Is Duplicating: {propertyEntry.IsSelectedForDuplication}\n");
                    }                  
                }
            }
        }
    }
}