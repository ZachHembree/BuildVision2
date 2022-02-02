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
        /// <summary>
        /// Returns the current state of the menu
        /// </summary>
        public QuickActionMenuState MenuState { get; private set; }

        public static bool DrawDebug { get; set; }

        private readonly RadialSelectionBox<QuickActionEntryBase, Label> propertyWheel, dupeWheel;
        private readonly Body menuBody;
        private readonly PropertyListMenu propertyList;

        private readonly ObjectPool<object> propertyEntryPool;
        private readonly QuickActionShortcutEntry scrollMenuShortcut, dupeShortcut;
        private RadialSelectionBox<QuickActionEntryBase, Label> activeWheel;

        private readonly Label debugText;
        private int tick;
        private BlockPropertyDuplicator duplicator;

        public QuickActionMenu(HudParentBase parent = null) : base(parent)
        {
            menuBody = new Body(this) { };

            // Selection wheel for block properties
            propertyWheel = new RadialSelectionBox<QuickActionEntryBase, Label>()
            {
                BackgroundColor = bodyColor,
                HighlightColor = selectionBoxColor,
                ZOffset = -1,
            };
            propertyWheel.Register(menuBody, true);

            // Selection wheel for dupe shortcuts
            dupeWheel = new RadialSelectionBox<QuickActionEntryBase, Label>() 
            {
                Visible = false,
                BackgroundColor = bodyColor,
                HighlightColor = selectionBoxColor,
                ZOffset = -1,
                CollectionContainer = 
                {
                    new QuickActionShortcutEntry()
                    {
                        Text = "Back",
                        ShortcutAction = StopPropertyDuplication,
                    },
                    new QuickActionShortcutEntry()
                    {
                        Text = "Copy All",
                        ShortcutAction = CopyAllProperties,
                    },
                    new QuickActionShortcutEntry()
                    {
                        Text = "Paste",
                        ShortcutAction = PasteCopiedProperties,
                    },
                }
            };
            propertyWheel.Register(dupeWheel, true);

            // Old-style list menu as fallback
            propertyList = new PropertyListMenu(this) { Visible = false };

            // Shortcuts to be added to the property wheel later
            scrollMenuShortcut = new QuickActionShortcutEntry()
            { 
                Text = "Open List Menu",
                ShortcutAction = OpenPropertyList,
            };
            dupeShortcut = new QuickActionShortcutEntry()
            {
                Text = "Copy Settings",
                ShortcutAction = StartPropertyDuplication,
            };

            // I'm using a generic pool because I'm using two types of entry in the same list, but only
            // one is pooled.
            propertyEntryPool = new ObjectPool<object>(
                () => new QuickActionPropertyEntry(), 
                x => (x as QuickActionPropertyEntry).Reset()
            );

            debugText = new Label(this)
            {
                Visible = false,
                AutoResize = true,
                BuilderMode = TextBuilderModes.Lined,
                ParentAlignment = ParentAlignments.Left
            };
        }

        /// <summary>
        /// Opens the menu and populates it with properties from the given property block
        /// </summary>
        public void OpenRaidalMenu(BlockPropertyDuplicator duplicator)
        {
            Clear();

            for (int i = 0; i < duplicator.BlockMembers.Count; i++)
            {
                var entry = propertyEntryPool.Get() as QuickActionPropertyEntry;
                entry.SetMember(i, duplicator);
                propertyWheel.Add(entry);
            }

            propertyWheel.Add(scrollMenuShortcut);
            propertyWheel.Add(dupeShortcut);

            propertyWheel.IsInputEnabled = true;
            MenuState = QuickActionMenuState.PropertySelection;
            Visible = true;
        }

        /// <summary>
        /// Closes and resets the menu
        /// </summary>
        public void CloseMenu()
        {
            Clear();
            Visible = false;
            MenuState = QuickActionMenuState.Closed;
        }

        /// <summary>
        /// Clears all entries from the menu and closes any open widgets
        /// </summary>
        private void Clear()
        {
            StopPropertyDuplication();
            menuBody.CloseWidget();
            propertyList.CloseMenu();
            duplicator = null;

            propertyEntryPool.ReturnRange(propertyWheel.EntryList, 0, propertyWheel.EntryList.Count - 2);
            propertyWheel.Clear();
            propertyWheel.IsInputEnabled = false;
        }

        protected override void Layout()
        {
            Vector2 size = cachedSize - cachedPadding;
            menuBody.Size = 1.05f * propertyWheel.Size * propertyWheel.polyBoard.InnerRadius;

            if (MenuState == QuickActionMenuState.ListMenuControl)
            {
                menuBody.Visible = false;
                Size = propertyList.Size;
            }
            else
            {
                menuBody.Visible = true;
                Size = propertyWheel.Size;

                if (tick == 0)
                {
                    foreach (QuickActionEntryBase baseEntry in propertyWheel)
                    {
                        var entry = baseEntry as QuickActionPropertyEntry;

                        if (entry != null)
                            entry.UpdateText();
                    }
                }
            }

            tick++;
            tick %= textTickDivider;
        }
    }
}