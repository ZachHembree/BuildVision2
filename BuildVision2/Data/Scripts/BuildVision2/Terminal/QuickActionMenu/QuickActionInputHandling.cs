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
        protected override void HandleInput(Vector2 cursorPos)
        {
            BindManager.RequestTempBlacklist(SeBlacklistModes.Mouse);

            // Open menus corresponding to their state flags
            if ((MenuState & QuickActionMenuState.WheelMenuControl) > 0 && !propertyWheel.IsOpen)
            {
                OpenPropertyWheel();
            }
            else if ((MenuState & QuickActionMenuState.ListMenuControl) > 0 && !propertyList.IsOpen)
            {
                OpenPropertyList();
            }

            // Close menus if not in the correct state. Probably redundant.
            if ((MenuState & QuickActionMenuState.ListMenuControl) == 0 && propertyList.IsOpen)
            {
                propertyList.CloseMenu();
            }

            if ((MenuState & QuickActionMenuState.WheelMenuControl) == 0 && propertyWheel.IsOpen)
            {
                propertyWheel.CloseMenu();
            }

            if (MenuState == QuickActionMenuState.Closed)
                CloseMenu();
        }

        private void OpenPropertyWheel()
        {
            propertyList.HideMenu();
            propertyWheel.OpenMenu();

            MenuState &= ~QuickActionMenuState.ListMenuControl;
            MenuState |= QuickActionMenuState.WheelMenuControl;
        }

        /// <summary>
        /// Opens the list menu
        /// </summary>
        private void OpenPropertyList()
        {
            propertyWheel.HideMenu();
            propertyList.OpenMenu();

            MenuState &= ~QuickActionMenuState.WheelMenuControl;
            MenuState |= QuickActionMenuState.ListMenuControl;
        }
    }
}