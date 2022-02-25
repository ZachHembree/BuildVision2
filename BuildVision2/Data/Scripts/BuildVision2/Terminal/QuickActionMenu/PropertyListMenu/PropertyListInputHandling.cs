using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Text;
using System.Collections.Generic;
using Sandbox.ModAPI;
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
    public sealed partial class QuickActionMenu
    {
        private partial class PropertyListMenu : HudElementBase
        {
            private static int incrX, incrY, incrZ;

            protected override void HandleInput(Vector2 cursorPos)
            {
                if (listBody.Collection.Count > 0 
                    && (MenuState & QuickActionMenuState.ListMenuControl) == QuickActionMenuState.ListMenuControl)
                {
                    // Highlight selection
                    if (!(BvBinds.StartDupe.IsPressed || BvBinds.StopDupe.IsPressed))
                    {
                        if(!listBody[selectionIndex].PropertyOpen)
                        {
                            bool multXPressed = BvBinds.MultX.IsPressed;
                            int offset = multXPressed ? 4 : 1;

                            if (BvBinds.ScrollUp.IsNewPressed)
                            {
                                OffsetSelectionIndex(-offset);
                                listWrapTimer.Restart();
                            }
                            else if (BvBinds.ScrollDown.IsNewPressed)
                            {
                                OffsetSelectionIndex(offset);
                                listWrapTimer.Restart();
                            }
                        }

                        selectionIndex = MathHelper.Clamp(selectionIndex, 0, listBody.Count - 1);

                        if (BvBinds.Cancel.IsReleased)
                        {
                            if ((MenuState & QuickActionMenuState.PropertyDuplication) > 0)
                                quickActionMenu.OpenPropertyWheel();
                            else
                                quickActionMenu.CloseMenu();
                        }
                        else
                        {
                            if ((MenuState & QuickActionMenuState.PropertyDuplication) > 0)
                                HandleDuplicationInput();
                            else
                                HandlePropertySelectionInput();
                        }
                    }
                }
            }

            private void HandleDuplicationInput()
            {
                var selection = listBody[selectionIndex];

                if (BvBinds.Select.IsReleased)
                    selection.IsSelectedForDuplication = !selection.IsSelectedForDuplication;
            }

            private void HandlePropertySelectionInput()
            {
                var selection = listBody[selectionIndex];
                IBlockMember blockMember = selection.AssocMember;

                if (BvBinds.Select.IsReleased)
                    selection.PropertyOpen = !selection.PropertyOpen;
                else if (!selection.PropertyOpen && BindManager.IsChatOpen && 
                    MyAPIGateway.Input.IsNewGameControlPressed(MyStringId.Get("CHAT_SCREEN")) && 
                    blockMember is IBlockTextMember)
                {
                    selection.PropertyOpen = true;
                }

                // Close text input on chat close or property deselect
                if ((!BindManager.IsChatOpen && selection.InputOpen) ||
                    (!selection.PropertyOpen && (selection.WaitingForChatInput || selection.InputOpen)))
                {
                    // If no input was recieved, don't write anything
                    if (!selection.WaitingForChatInput)
                        selection.SetValueText(selection.ValueText.ToString());

                    selection.WaitingForChatInput = false;
                    selection.PropertyOpen = false;
                    selection.CloseInput();
                }

                // Handle input for selected entry
                if (selection.PropertyOpen)
                {
                    MenuState |= QuickActionMenuState.PropertyOpen;

                    if (blockMember is IBlockAction)
                        HandleActionInput();
                    else if (blockMember is IBlockNumericValue<float>)
                        HandleFloatInput();
                    else if (blockMember is IBlockNumericValue<byte>)
                        HandleColorInput();
                    else if (blockMember is IBlockComboBox)
                        HandleComboInput();
                    else if (blockMember is IBlockTextMember)
                        HandleTextInput();
                }
                else
                    MenuState &= ~QuickActionMenuState.PropertyOpen;

                if (selection.PropertyOpen)
                    highlightBox.Color = highlightFocusColor;
                else
                    highlightBox.Color = highlightColor;
            }

            /// <summary>
            /// Handles input for actions in list
            /// </summary>
            private void HandleActionInput()
            {
                IBlockMember blockMember = listBody[selectionIndex].AssocMember;
                var member = blockMember as IBlockAction;

                member.Action();
                listBody[selectionIndex].PropertyOpen = false;
            }

            /// <summary>
            /// Handles input for fp values in list
            /// </summary>
            private void HandleFloatInput()
            {
                if (BindManager.IsChatOpen)
                {
                    HandleTextInput();
                }
                else
                {
                    IBlockMember blockMember = listBody[selectionIndex].AssocMember;
                    var floatMember = blockMember as IBlockNumericValue<float>;
                    double absRange = Math.Abs(floatMember.MaxValue - floatMember.MinValue),
                        logRange = Math.Ceiling(Math.Log10(absRange)),
                        offset, value;

                    if (absRange > floatPropLogThreshold)
                    {
                        offset = .1;
                        value = (float)(Math.Log10(Math.Abs(floatMember.Value - floatMember.MinValue) + 1d) / logRange);
                    }
                    else
                    {
                        value = floatMember.Value;
                        offset = floatMember.Increment;
                    }

                    if (BvBinds.MultZ.IsPressed)
                        offset *= BvConfig.Current.block.floatMult.Z;
                    else if (BvBinds.MultY.IsPressed)
                        offset *= BvConfig.Current.block.floatMult.Y;
                    else if (BvBinds.MultX.IsPressed)
                        offset *= BvConfig.Current.block.floatMult.X;

                    if (BvBinds.ScrollUp.IsNewPressed)
                        value += offset;
                    else if (BvBinds.ScrollDown.IsNewPressed)
                        value -= offset;

                    if (absRange > floatPropLogThreshold)
                        floatMember.Value = (float)(Math.Pow(10d, value * logRange) - 1d + floatMember.MinValue);
                    else
                        floatMember.Value = (float)value;
                }
            }

            /// <summary>
            /// Handles input for color values in list
            /// </summary>
            private void HandleColorInput()
            {
                if (BindManager.IsChatOpen)
                {
                    HandleTextInput();
                }
                else
                {
                    IBlockMember blockMember = listBody[selectionIndex].AssocMember;
                    var colorMember = blockMember as IBlockNumericValue<byte>;
                    int offset = 1;

                    if (BvBinds.MultZ.IsPressed)
                        offset *= incrZ;
                    else if (BvBinds.MultY.IsPressed)
                        offset *= incrY;
                    else if (BvBinds.MultX.IsPressed)
                        offset *= incrX;

                    byte color = colorMember.Value;

                    if (BvBinds.ScrollUp.IsNewPressed)
                    {
                        colorMember.Value = (byte)MathHelper.Clamp(color + offset, 0, 255);
                    }
                    else if (BvBinds.ScrollDown.IsNewPressed)
                    {
                        colorMember.Value = (byte)MathHelper.Clamp(color - offset, 0, 255);
                    }
                }
            }

            /// <summary>
            /// Handles input for comboboxes
            /// </summary>
            private void HandleComboInput()
            {
                if (BvBinds.ScrollUp.IsNewPressed || BvBinds.ScrollDown.IsNewPressed)
                {
                    IBlockMember blockMember = listBody[selectionIndex].AssocMember;
                    var comboBox = blockMember as IBlockComboBox;
                    var entries = comboBox.ComboEntries as List<KeyValuePair<long, StringBuilder>>;
                    int index = (int)comboBox.Value;

                    if (BvBinds.ScrollUp.IsNewPressed)
                        index = MathHelper.Clamp(index + 1, 0, entries.Count - 1);
                    else if (BvBinds.ScrollDown.IsNewPressed)
                        index = MathHelper.Clamp(index - 1, 0, entries.Count - 1);

                    comboBox.Value = index;
                }
            }

            private void HandleTextInput()
            {
                PropertyListEntry selection = listBody[selectionIndex];

                if (BindManager.IsChatOpen)
                {
                    if (!selection.InputOpen)
                    {
                        IBlockMember blockMember = selection.AssocMember;
                        selection.ValueText.SetText(blockMember.ValueText, selectedFormat);
                        selection.OpenInput();
                        selection.WaitingForChatInput = false;
                    }
                }
                else
                {
                    selection.ValueText.SetText(textEntryWarning, selectedFormat);
                    selection.WaitingForChatInput = true;
                }
            }

            /// <summary>
            /// Updates the selection index
            /// </summary>
            private void OffsetSelectionIndex(int offset)
            {
                int min = GetFirstIndex(), max = GetLastIndex(), dir = (offset > 0) ? 1 : -1;
                offset = Math.Abs(offset);

                for (int i = 1; i <= offset; i++)
                {
                    selectionIndex += dir;

                    for (int j = selectionIndex; (j <= max && j >= min); j += dir)
                    {
                        if (listBody.Collection[j].Enabled)
                        {
                            selectionIndex = j;
                            break;
                        }
                    }
                }

                if (listWrapTimer.ElapsedMilliseconds > 300 && (selectionIndex > max || selectionIndex < min) 
                    && !BvBinds.MultX.IsPressed)
                {
                    if (selectionIndex < min)
                    {
                        selectionIndex = max;
                        listBody.End = selectionIndex;
                    }
                    else
                    {
                        selectionIndex = min;
                        listBody.Start = selectionIndex;
                    }
                }
                else
                {
                    selectionIndex = MathHelper.Clamp(selectionIndex, min, max);

                    if (selectionIndex < listBody.Start)
                        listBody.Start = selectionIndex;
                    else if (selectionIndex > listBody.End)
                        listBody.End = selectionIndex;
                }
            }

            /// <summary>
            /// Returns the index of the first enabled property.
            /// </summary>
            private int GetFirstIndex()
            {
                int first = 0;

                for (int n = 0; n < listBody.Collection.Count; n++)
                {
                    if (listBody.Collection[n].Enabled)
                    {
                        first = n;
                        break;
                    }
                }

                return first;
            }

            /// <summary>
            /// Retrieves the index of the last enabled property.
            /// </summary>
            private int GetLastIndex()
            {
                int last = 0;

                for (int n = listBody.Collection.Count - 1; n >= 0; n--)
                {
                    if (listBody.Collection[n].Enabled)
                    {
                        last = n;
                        break;
                    }
                }

                return last;
            }
        }
    }
}