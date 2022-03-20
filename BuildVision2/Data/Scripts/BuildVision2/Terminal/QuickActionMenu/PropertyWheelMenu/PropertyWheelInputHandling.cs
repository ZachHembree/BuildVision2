using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using Sandbox.ModAPI;
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
                    if ((MenuState & QuickActionMenuState.WheelMenuControl) == QuickActionMenuState.WheelMenuControl)
                    {
                        if (BvBinds.MultXOrMouse.IsPressed)
                            BindManager.RequestTempBlacklist(SeBlacklistModes.MouseAndCam);

                        if (activeWheel != null)
                        {
                            if ((MenuState & QuickActionMenuState.PropertyOpen) == 0 ^ activeWheel.UseGestureInput)
                                activeWheel.SetHighlightAt(activeWheel.SelectionIndex);

                            activeWheel.InputEnabled = BvBinds.MultXOrMouse.IsPressed || HudMain.Cursor.Visible;
                            activeWheel.UseGestureInput = (MenuState & QuickActionMenuState.PropertyOpen) == 0;
                        }

                        if ((MenuState & QuickActionMenuState.WidgetControl) == QuickActionMenuState.WidgetControl)
                        {
                            if (activeWheel.IsMousedOver && SharedBinds.LeftButton.IsReleased
                                && !(wheelBody.ActiveWidget?.MouseInput.IsLeftClicked ?? false))
                            {
                                HandleWheelInput();
                            }

                            if (!wheelBody.IsWidgetOpen)
                                MenuState = QuickActionMenuState.WheelMenuControl;
                            else
                            {
                                wheelBody.ActiveWidget.InputEnabled = !(activeWheel.IsMousedOver && SharedBinds.LeftButton.IsPressed);
                                HudMain.EnableCursor = BvBinds.MultXOrMouse.IsPressed;
                            }                            
                        }
                        else
                        {
                            if ((MenuState & QuickActionMenuState.PropertyDuplication) > 0)
                            {
                                activeWheel = dupeWheel;
                                propertyWheel.Visible = false;
                                dupeWheel.Visible = true;
                            }
                            else
                            {
                                activeWheel = propertyWheel;
                                propertyWheel.Visible = true;
                                dupeWheel.Visible = false;
                            }

                            if (wheelBody.IsWidgetOpen)
                                wheelBody.CloseWidget();

                            activeWheel.ClearSelection();
                            HandleWheelInput();
                        }
                    }
                }
            }

            /// <summary>
            /// Handles input for both selection wheels
            /// </summary>
            private void HandleWheelInput()
            {
                var entry = activeWheel.HighlightedEntry;

                if (!(BvBinds.StartDupe.IsPressed || BvBinds.StopDupe.IsPressed))
                {
                    if (BvBinds.ScrollUp.IsNewPressed || BvBinds.ScrollUp.IsPressedAndHeld)
                        ScrollSelection(1);
                    if (BvBinds.ScrollDown.IsNewPressed || BvBinds.ScrollDown.IsPressedAndHeld)
                        ScrollSelection(-1);

                    if (BvBinds.Cancel.IsNewPressed)
                    {
                        if ((MenuState & QuickActionMenuState.PropertyDuplication) > 0)
                            MenuState &= ~QuickActionMenuState.PropertyDuplication;
                        else
                            quickActionMenu.CloseMenu();
                    }
                    else
                    {
                        if ((MenuState & QuickActionMenuState.PropertyDuplication) > 0)
                            HandleDupeSelection(entry);
                        else
                            HandlePropertySelection(entry);
                    }
                }
            }

            /// <summary>
            /// Handles selection for an entry in the duplication selection wheel
            /// </summary>
            private void HandleDupeSelection(PropertyWheelEntryBase entry)
            {
                if (BvBinds.Select.IsReleased)
                {
                    var shortcutEntry = entry as PropertyWheelShortcutEntry;
                    shortcutEntry.ShortcutAction();
                }
            }

            /// <summary>
            /// Handles the selection of an entry in one of the selection wheels
            /// </summary>
            private void HandlePropertySelection(PropertyWheelEntryBase entry)
            {
                var propertyEntry = entry as PropertyWheelEntry;
                bool clicked = BvBinds.Select.IsReleased && !activeWheel.IsMousedOver
                    || SharedBinds.LeftButton.IsReleased && activeWheel.IsMousedOver;

                if (clicked || (
                    propertyEntry?.BlockMember is IBlockTextMember 
                    && BindManager.IsChatOpen
                    && MyAPIGateway.Input.IsNewGameControlPressed(MyStringId.Get("CHAT_SCREEN")) 
                ))
                {
                    if (entry is PropertyWheelShortcutEntry)
                    {
                        var shortcut = entry as PropertyWheelShortcutEntry;
                        shortcut.ShortcutAction();
                    }
                    else if (propertyEntry.BlockMember != null && entry.Enabled)
                    {
                        if (propertyEntry.BlockMember is IBlockAction)
                        {
                            var blockAction = propertyEntry.BlockMember as IBlockAction;
                            blockAction.Action();
                        }
                        else if (entry != activeWheel.Selection)
                        {
                            wheelBody.OpenBlockMemberWidget(propertyEntry.BlockMember);
                            MenuState = QuickActionMenuState.WidgetControl;
                            activeWheel.SetSelectionAt(activeWheel.HighlightIndex);
                        }
                    }
                }
            }

            /// <summary>
            /// Offsets scroll wheel selection in the direction of the given offset to the next enabled entry, 
            /// with the magnitude determining the number of steps. Wraps around.
            /// </summary>
            private void ScrollSelection(int offset)
            {
                int index = Math.Max(activeWheel.HighlightIndex, 0),
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

                activeWheel.SetHighlightAt(index);
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