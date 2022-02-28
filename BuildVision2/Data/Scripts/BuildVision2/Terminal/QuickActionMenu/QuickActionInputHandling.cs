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
            HandleDupeInput();
            UpdateStateMain(); 
        }

        private void UpdateStateMain()
        {
            GetStateInput();

            if (MenuState == QuickActionMenuState.WheelPeek)
                propertyWheel.OpenSummary();
            else if (MenuState == QuickActionMenuState.ListPeek)
                propertyList.OpenSummary();
            else if (!BvConfig.Current.genUI.legacyModeEnabled)
                BindManager.RequestTempBlacklist(SeBlacklistModes.Mouse);

            // Open menus corresponding to their state flags
            if (!propertyWheel.IsOpen && (MenuState & QuickActionMenuState.WheelMenuControl) == QuickActionMenuState.WheelMenuControl)
                OpenPropertyWheel();
            else if (!propertyList.IsOpen && (MenuState & QuickActionMenuState.ListMenuControl) == QuickActionMenuState.ListMenuControl)
                OpenPropertyList();

            // Close menus if not in the correct state. Probably redundant.
            if ((MenuState & QuickActionMenuState.ListMenuControl) == 0 && propertyList.IsOpen)
                propertyList.CloseMenu();

            if ((MenuState & QuickActionMenuState.WheelMenuControl) == 0 && propertyWheel.IsOpen)
                propertyWheel.CloseMenu();

            if (MenuState == QuickActionMenuState.Closed)
                CloseMenu();
        }

        private void GetStateInput()
        {
            // Open property controls or remian in peek mode
            if ((MenuState & QuickActionMenuState.Controlled) == 0)
            {
                if (BvBinds.OpenWheel.IsNewPressed)
                {
                    MenuState = QuickActionMenuState.WheelMenuControl;
                }
                else if (BvBinds.OpenList.IsNewPressed)
                {
                    MenuState = QuickActionMenuState.ListMenuControl;
                }
                else if (BvBinds.EnableMouse.IsPressed)
                {
                    if (BvConfig.Current.genUI.legacyModeEnabled)
                        MenuState = QuickActionMenuState.ListPeek;
                    else
                        MenuState = QuickActionMenuState.WheelPeek;
                }
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
        }

        /// <summary>
        /// Opens property wheel
        /// </summary>
        private void OpenPropertyWheel()
        {
            propertyList.HideMenu();
            propertyWheel.OpenMenu();

            MenuState &= ~(QuickActionMenuState.ListMenuControl | QuickActionMenuState.Peek);
            MenuState |= QuickActionMenuState.WheelMenuControl;
        }

        /// <summary>
        /// Opens the list menu
        /// </summary>
        private void OpenPropertyList(bool isShortcut = false)
        {
            propertyWheel.HideMenu();
            propertyList.OpenMenu();

            MenuState &= ~(QuickActionMenuState.WheelMenuControl | QuickActionMenuState.Peek);
            MenuState |= QuickActionMenuState.ListMenuControl;

            if (isShortcut)
                MenuState |= QuickActionMenuState.WheelShortcutOpened;
        }
    }
}