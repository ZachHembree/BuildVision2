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
            public bool IsOpen => Visible;

            /// <summary>
            /// Returns true if a property widget is currently open
            /// </summary>
            public bool IsWidgetOpen => menuBody.IsWidgetOpen;

            private readonly RadialSelectionBox<PropertyWheelEntryBase, Label> propertyWheel, dupeWheel;
            private readonly PropertyWheelMenuBody menuBody;

            private readonly ObjectPool<object> propertyEntryPool;
            private readonly List<PropertyWheelShortcutEntry> shortcutEntries;
            private RadialSelectionBox<PropertyWheelEntryBase, Label> activeWheel;
            private int textUpdateTick;

            public PropertyWheelMenu(QuickActionMenu parent) : base(parent)
            {
                this.quickActionMenu = parent;
                menuBody = new PropertyWheelMenuBody(this) { };

                // Selection wheel for block properties
                propertyWheel = new RadialSelectionBox<PropertyWheelEntryBase, Label>(menuBody)
                {
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
                propertyWheel.IsInputEnabled = true;
                Visible = true;
            }

            public void CloseMenu()
            {
                Clear();
                Visible = false;
            }

            /// <summary>
            /// Clears all entries from the menu and closes any open widgets
            /// </summary>
            private void Clear()
            {
                StopPropertyDuplication();
                menuBody.CloseWidget();

                propertyEntryPool.ReturnRange(propertyWheel.EntryList, 0,
                    propertyWheel.EntryList.Count - shortcutEntries.Count);

                propertyWheel.Clear();
                propertyWheel.IsInputEnabled = false;
            }

            protected override void Layout()
            {
                Size = propertyWheel.Size;
                menuBody.Size = 1.05f * propertyWheel.Size * propertyWheel.polyBoard.InnerRadius;

                if (textUpdateTick == 0)
                {
                    foreach (PropertyWheelEntryBase baseEntry in propertyWheel)
                    {
                        var entry = baseEntry as PropertyWheelEntry;

                        if (entry != null && entry.Enabled)
                            entry.UpdateText();
                    }
                }

                textUpdateTick++;
                textUpdateTick %= textTickDivider;
            }
        }
    }
}