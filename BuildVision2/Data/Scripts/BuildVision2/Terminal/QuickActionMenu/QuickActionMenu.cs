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

        /// <summary>
        /// Currently assigned block property duplicator
        /// </summary>
        public BlockPropertyDuplicator Duplicator { get; private set; }

        /// <summary>
        /// Enables/disables debug text
        /// </summary>
        public static bool DrawDebug { get; set; }

        private readonly PropertyWheelMenu propertyWheel;
        private readonly PropertyListMenu propertyList;
        private readonly Label debugText;

        public QuickActionMenu(HudParentBase parent = null) : base(parent)
        {
            propertyWheel = new PropertyWheelMenu(this) { Visible = false };
            // Old-style list menu as fallback
            propertyList = new PropertyListMenu(this) { Visible = false };

            propertyWheel.RegisterShortcut(new PropertyWheelShortcutEntry()
            {
                Text = "Open List Menu",
                ShortcutAction = OpenPropertyList
            });

            debugText = new Label(this)
            {
                Visible = false,
                AutoResize = true,
                BuilderMode = TextBuilderModes.Lined,
                ParentAlignment = ParentAlignments.Left
            };
        }

        /// <summary>
        /// Opens the wheel menu and populates it with properties from the given property block
        /// </summary>
        public void OpenRaidalMenu(BlockPropertyDuplicator duplicator)
        {
            CloseMenu();
            Duplicator = duplicator;

            MenuState = QuickActionMenuState.WheelMenuControl;
            Visible = true;
        }

        /// <summary>
        /// Opens the property list menu and populates it with properties from the given property block
        /// </summary>
        public void OpenListMenu(BlockPropertyDuplicator duplicator)
        {
            CloseMenu();
            Duplicator = duplicator;

            MenuState = QuickActionMenuState.ListMenuControl;
            Visible = true;
        }

        /// <summary>
        /// Closes and resets the menu
        /// </summary>
        public void CloseMenu()
        {
            propertyWheel.CloseMenu();
            propertyList.CloseMenu();

            Duplicator = null;
            Visible = false;
            MenuState = QuickActionMenuState.Closed;
        }
    }
}