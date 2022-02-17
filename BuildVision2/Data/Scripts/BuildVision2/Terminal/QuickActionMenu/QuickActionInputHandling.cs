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
            // Open property controls or remian in peek mode
            if (MenuState == QuickActionMenuState.Closed || MenuState == QuickActionMenuState.Peek)
            {
                if (BvBinds.OpenWheel.IsNewPressed)
                    MenuState = QuickActionMenuState.WheelMenuControl;
                else if (BvBinds.OpenList.IsNewPressed)
                    MenuState = QuickActionMenuState.ListMenuControl;
                else if (BvBinds.EnableMouse.IsPressed)
                    MenuState = QuickActionMenuState.Peek;
            }

            // Start duplication
            if ((MenuState & QuickActionMenuState.PropertyDuplication) == 0 
                && (MenuState & QuickActionMenuState.PropertyOpen) == 0)
            {
                if (BvBinds.StartDupe.IsNewPressed)
                {
                    MenuState |= QuickActionMenuState.PropertyDuplication;

                    // If not controlled, open wheel menu
                    if ((MenuState & QuickActionMenuState.Controlled) == 0)
                        MenuState |= QuickActionMenuState.WheelMenuControl;
                }
            }
            // Stop duplication
            else if ((MenuState & QuickActionMenuState.PropertyDuplication) > 0 
                && BvBinds.StopDupe.IsNewPressed)
            {
                MenuState &= ~QuickActionMenuState.PropertyDuplication;
            }

            if (MenuState == QuickActionMenuState.Peek)
                // Only draw wheel body while peeking
                propertyWheel.OpenSummary();
            else
                BindManager.RequestTempBlacklist(SeBlacklistModes.Mouse);

            // Open menus corresponding to their state flags
            if ((MenuState & QuickActionMenuState.WheelMenuOpen) > 0 && !propertyWheel.IsOpen)
                OpenPropertyWheel();
            else if ((MenuState & QuickActionMenuState.ListMenuOpen) > 0 && !propertyList.IsOpen)
                OpenPropertyList();

            // Close menus if not in the correct state. Probably redundant.
            if ((MenuState & QuickActionMenuState.ListMenuOpen) == 0 && propertyList.IsOpen)
                propertyList.CloseMenu();

            if ((MenuState & QuickActionMenuState.WheelMenuOpen) == 0 && propertyWheel.IsOpen)
                propertyWheel.CloseMenu();

            if (MenuState == QuickActionMenuState.Closed)
                CloseMenu();
        }

        /// <summary>
        /// Opens property wheel
        /// </summary>
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