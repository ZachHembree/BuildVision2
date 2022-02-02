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
                propertyWheel.OpenMenu();
            }
            else if ((MenuState & QuickActionMenuState.ListMenuControl) > 0 && !propertyList.IsOpen)
            {
                propertyList.OpenMenu();
            }

            // Close menus if not in the correct state
            if ((MenuState & QuickActionMenuState.WheelMenuControl) == 0 && propertyList.IsOpen)
            {
                propertyList.CloseMenu();
            }

            if ((MenuState & QuickActionMenuState.ListMenuControl) == 0 && propertyWheel.IsOpen)
            {
                propertyWheel.CloseMenu();
            }

            if (DrawDebug)
            {
                debugText.Visible = true;

                ITextBuilder debugBuilder = debugText.TextBoard;
                debugBuilder.Clear();
                debugBuilder.Append($"State: {MenuState}\n");
                debugBuilder.Append($"IsWidgetOpen: {propertyWheel.IsWidgetOpen}\n");
                debugBuilder.Append($"Cursor Mode: {HudMain.InputMode}\n");
                debugBuilder.Append($"Blacklist Mode: {BindManager.BlacklistMode}\n");
                debugBuilder.Append($"Enable Cursor Pressed: {BvBinds.EnableMouse.IsPressed}\n");
            }
            else
                debugText.Visible = false;
        }

        /// <summary>
        /// Opens the list menu
        /// </summary>
        private void OpenPropertyList()
        {
            propertyList.OpenMenu();
            MenuState = QuickActionMenuState.ListMenuControl;
        }
    }
}