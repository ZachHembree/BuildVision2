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
            if (propertyWheel.EntryList.Count > 0)
            {
                BindManager.RequestTempBlacklist(SeBlacklistModes.Mouse);

                if (MenuState == QuickActionMenuState.PropertySelection || MenuState == QuickActionMenuState.PropertyDuplication)
                {
                    if (MenuState == QuickActionMenuState.PropertySelection)
                        activeWheel = propertyWheel;
                    else
                        activeWheel = dupeWheel;

                    if (menuBody.IsWidgetOpen)
                        menuBody.CloseWidget();

                    HandleWheelInput();
                }
                else if (MenuState == QuickActionMenuState.WidgetControl)
                {
                    // Switch back to radial list after the widget is closed
                    if (!menuBody.IsWidgetOpen)
                        MenuState = QuickActionMenuState.PropertySelection;
                    else
                        HudMain.EnableCursor = BvBinds.EnableMouse.IsPressed;
                }
            }

            if (DrawDebug)
            {
                debugText.Visible = true;

                ITextBuilder debugBuilder = debugText.TextBoard;
                debugBuilder.Clear();
                debugBuilder.Append($"State: {MenuState}\n");
                debugBuilder.Append($"IsWidgetOpen: {menuBody.IsWidgetOpen}\n");
                debugBuilder.Append($"Wheel Input Enabled: {propertyWheel.IsInputEnabled}\n");
                debugBuilder.Append($"Cursor Mode: {HudMain.InputMode}\n");
                debugBuilder.Append($"Blacklist Mode: {BindManager.BlacklistMode}\n");
                debugBuilder.Append($"Enable Cursor Pressed: {BvBinds.EnableMouse.IsPressed}\n");
            }
            else
                debugText.Visible = false;
        }

        private void HandleWheelInput()
        {
            var selection = activeWheel.Selection;

            if (BvBinds.EnableMouse.IsPressed)
                BindManager.RequestTempBlacklist(SeBlacklistModes.MouseAndCam);

            activeWheel.IsInputEnabled = BvBinds.EnableMouse.IsPressed;

            if (BvBinds.Select.IsReleased)
            {
                if (MenuState == QuickActionMenuState.PropertySelection)
                    HandlePropertySelection(selection);
                else if (MenuState == QuickActionMenuState.PropertyDuplication)
                    HandleDupeSelection(selection);
            }
            else if (BvBinds.ScrollUp.IsPressed)
            {
                ScrollSelection(1);
            }
            else if (BvBinds.ScrollDown.IsPressed)
            {
                ScrollSelection(-1);
            }
        }

        private void HandlePropertySelection(QuickActionEntryBase selection)
        {
            var propertyEntry = selection as QuickBlockPropertyEntry;
            var member = propertyEntry?.BlockMember;

            if (selection is QuickActionShortcutEntry)
            {
                var shortcut = selection as QuickActionShortcutEntry;
                shortcut.ShortcutAction();
            }
            else if (member != null && selection.Enabled)
            {
                if (member is IBlockAction)
                {
                    var blockAction = member as IBlockAction;
                    blockAction.Action();
                }
                else
                {
                    menuBody.OpenBlockMemberWidget(member);
                    activeWheel.IsInputEnabled = false;
                    MenuState = QuickActionMenuState.WidgetControl;
                }
            }
        }

        private void OpenPropertyListWidget()
        {
            menuBody.OpenPropertyListWidget();
            activeWheel.IsInputEnabled = false;
            MenuState = QuickActionMenuState.WidgetControl;
        }

        /// <summary>
        /// Offsets scroll wheel selection in the direction of the given offset to the next enabled entry, 
        /// with the magnitude determining the number of steps. Wraps around.
        /// </summary>
        private void ScrollSelection(int offset)
        {
            int index = activeWheel.SelectionIndex,
                dir = offset > 0 ? 1 : -1,
                absOffset = Math.Abs(offset);

            if (dir > 0)
            {
                for (int i = 0; i < absOffset; i++)
                {
                    index = (index + dir) % activeWheel.Count;
                    index = FindFirstEnabled(index);
                }
            }
            else
            {
                for (int i = 0; i < absOffset; i++)
                {
                    index = (index + dir) % activeWheel.Count;

                    if (index < 0)
                        index += activeWheel.Count;

                    index = FindLastEnabled(index);
                }
            }

            activeWheel.SetSelectionAt(index);
        }

        /// <summary>
        /// Returns first enabled element at or after the given index. Wraps around.
        /// </summary>
        private int FindFirstEnabled(int index)
        {
            int j = index;

            for (int n = 0; n < 2 * activeWheel.Count; n++)
            {
                if (activeWheel.EntryList[j].Enabled)
                    break;

                j++;
                j %= activeWheel.Count;
            }

            return j;
        }

        /// <summary>
        /// Returns preceeding enabled element at or after the given index. Wraps around.
        /// </summary>
        private int FindLastEnabled(int index)
        {
            int j = index;

            for (int n = 0; n < 2 * activeWheel.Count; n++)
            {
                if (activeWheel.EntryList[j].Enabled)
                    break;

                j--;
                j %= activeWheel.Count;
            }

            return j;
        }
    }
}