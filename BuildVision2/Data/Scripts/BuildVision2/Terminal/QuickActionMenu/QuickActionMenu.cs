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

        private BlockData copiedProperties, lastCopiedProperties;
        private PropertyBlock block;
        private RadialSelectionBox<QuickActionEntryBase, Label> activeWheel;

        private readonly Label debugText;
        private int tick;

        public QuickActionMenu(HudParentBase parent = null) : base(parent)
        {
            copiedProperties.propertyList = new List<PropertyData>();
            lastCopiedProperties.propertyList = new List<PropertyData>();

            propertyWheel = new RadialSelectionBox<QuickActionEntryBase, Label>(this)
            {
                BackgroundColor = bodyColor,
                HighlightColor = selectionBoxColor,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding
            };

            dupeWheel = new RadialSelectionBox<QuickActionEntryBase, Label>(this) 
            {
                Visible = false,
                BackgroundColor = bodyColor,
                HighlightColor = selectionBoxColor,
                DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
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

            menuBody = new Body(this) { };

            propertyList = new PropertyListMenu(this) { Visible = false };

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

            propertyEntryPool = new ObjectPool<object>(
                () => new QuickBlockPropertyEntry(), 
                x => (x as QuickBlockPropertyEntry).Reset()
            );

            debugText = new Label(this)
            {
                Visible = false,
                AutoResize = true,
                BuilderMode = TextBuilderModes.Lined,
                ParentAlignment = ParentAlignments.Left
            };

            Size = new Vector2(512f);
        }

        /// <summary>
        /// Opens the menu and populates it with properties from the given property block
        /// </summary>
        public void OpenMenu(PropertyBlock block)
        {
            Clear();
            this.block = block;

            foreach (IBlockMember blockMember in block.BlockMembers)
            {
                var entry = propertyEntryPool.Get() as QuickBlockPropertyEntry;
                entry.SetMember(blockMember);
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
            block = null;

            propertyEntryPool.ReturnRange(propertyWheel.EntryList, 0, propertyWheel.EntryList.Count - 2);
            propertyWheel.Clear();
            propertyWheel.IsInputEnabled = false;
        }

        protected override void Layout()
        {
            Vector2 size = cachedSize - cachedPadding;
            menuBody.Size = 1.05f * propertyWheel.Size * propertyWheel.polyBoard.InnerRadius;

            if (tick == 0)
            {
                foreach (QuickActionEntryBase baseEntry in propertyWheel)
                {
                    var entry = baseEntry as QuickBlockPropertyEntry;

                    if (entry != null)
                        entry.UpdateText();
                }
            }

            tick++;
            tick %= textTickDivider;
        }
    }
}