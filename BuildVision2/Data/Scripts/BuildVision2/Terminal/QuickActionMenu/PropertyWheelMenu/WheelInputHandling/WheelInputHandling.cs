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
            protected override void HandleInput(Vector2 cursorPos)
            {
                if (propertyWheel.EntryList.Count > 0)
                {
                    if ((MenuState & QuickActionMenuState.WidgetControl) == QuickActionMenuState.WidgetControl)
                    {
                        if (!menuBody.IsWidgetOpen)
                            MenuState = QuickActionMenuState.WheelMenuControl;
                        else
                            HudMain.EnableCursor = BvBinds.EnableMouse.IsPressed;
                    }
                    else
                    {
                        if ((MenuState & QuickActionMenuState.PropertyDuplication) > 0)
                            activeWheel = dupeWheel;
                        else
                            activeWheel = propertyWheel;

                        if (menuBody.IsWidgetOpen)
                            menuBody.CloseWidget();

                        HandleWheelInput();
                    }
                }
            }

            /// <summary>
            /// Handles input for both selection wheels
            /// </summary>
            private void HandleWheelInput()
            {
                var selection = activeWheel.Selection;

                if (BvBinds.EnableMouse.IsPressed)
                    BindManager.RequestTempBlacklist(SeBlacklistModes.MouseAndCam);

                activeWheel.IsInputEnabled = BvBinds.EnableMouse.IsPressed;

                if (BvBinds.Select.IsReleased)
                {
                    if ((MenuState & QuickActionMenuState.PropertyDuplication) > 0)
                        HandleDupeSelection(selection);
                    else
                        HandlePropertySelection(selection);
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

            /// <summary>
            /// Handles the selection of an entry in one of the selection wheels
            /// </summary>
            private void HandlePropertySelection(PropertyWheelEntryBase selection)
            {
                var propertyEntry = selection as PropertyWheelEntry;

                if (selection is PropertyWheelShortcutEntry)
                {
                    var shortcut = selection as PropertyWheelShortcutEntry;
                    shortcut.ShortcutAction();
                }
                else if (propertyEntry.BlockMember != null && selection.Enabled)
                {
                    if (propertyEntry.BlockMember is IBlockAction)
                    {
                        var blockAction = propertyEntry.BlockMember as IBlockAction;
                        blockAction.Action();
                    }
                    else
                    {
                        menuBody.OpenBlockMemberWidget(propertyEntry.BlockMember);
                        activeWheel.IsInputEnabled = false;
                        MenuState = QuickActionMenuState.WidgetControl;
                    }
                }
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
}