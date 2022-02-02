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
            private const string textEntryWarning = "Open Chat to Enable Input";

            protected override void HandleInput(Vector2 cursorPos)
            {
                if (body.EntryList.Count > 0)
                {
                    // Highlight selection
                    if (body.Selection == null || !body.Selection.PropertyOpen)
                    {
                        if (BvBinds.ScrollUp.IsNewPressed)
                        {
                            body.OffsetSelectionIndex(-1);
                        }
                        else if (BvBinds.ScrollDown.IsNewPressed)
                        {
                            body.OffsetSelectionIndex(1);
                        }
                    }

                    // Select highlighted entry
                    if (body.Selection != null)
                    {
                        if(isDuplicatingProperties)
                            HandleDuplicationInput();
                        else
                            HandlePropertySelectionInput();
                    }
                }
            }

            private void HandleDuplicationInput()
            {
                var selection = body.Selection;

                if (BvBinds.Select.IsReleased)
                    selection.IsSelectedForDuplication = !selection.IsSelectedForDuplication;
            }

            private void HandlePropertySelectionInput()
            {
                var selection = body.Selection;
                IBlockMember blockMember = selection.AssocMember;

                if (BvBinds.Select.IsReleased)
                    selection.PropertyOpen = !selection.PropertyOpen;
                else if (!selection.PropertyOpen && BindManager.IsChatOpen && blockMember is IBlockTextMember)
                    selection.PropertyOpen = true;

                // Close text input on chat close or property deselect
                if (!BindManager.IsChatOpen && selection.InputOpen ||
                    !selection.PropertyOpen && selection.WaitingForChatInput)
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
            }

            /// <summary>
            /// Handles input for actions in list
            /// </summary>
            private void HandleActionInput()
            {
                IBlockMember blockMember = body.Selection.AssocMember;
                var member = blockMember as IBlockAction;

                member.Action();
                body.Selection.PropertyOpen = false;
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
                    IBlockMember blockMember = body.Selection.AssocMember;
                    var floatMember = blockMember as IBlockNumericValue<float>;
                    float offset = floatMember.Increment,
                        value = floatMember.Value;

                    if (BvBinds.MultZ.IsPressed)
                        offset *= BvConfig.Current.block.floatMult.Z;
                    else if (BvBinds.MultY.IsPressed)
                        offset *= BvConfig.Current.block.floatMult.Y;
                    else if (BvBinds.MultX.IsPressed)
                        offset *= BvConfig.Current.block.floatMult.X;

                    if (BvBinds.ScrollUp.IsNewPressed)
                    {
                        floatMember.Value = MathHelper.Clamp(value + offset, floatMember.MinValue, floatMember.MaxValue);
                    }
                    else if (BvBinds.ScrollDown.IsNewPressed)
                    {
                        floatMember.Value = MathHelper.Clamp(value - offset, floatMember.MinValue, floatMember.MaxValue);
                    }
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
                    IBlockMember blockMember = body.Selection.AssocMember;
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

            private void HandleComboInput()
            {
                IBlockMember blockMember = body.Selection.AssocMember;
                var comboBox = blockMember as IBlockComboBox;
                var entries = comboBox.ComboEntries as List<KeyValuePair<long, StringBuilder>>;
                int index = entries.FindIndex(x => x.Key == comboBox.Value);

                if (BvBinds.ScrollUp.IsNewPressed)
                    index++;
                else if (BvBinds.ScrollDown.IsNewPressed)
                    index--;

                index = MathHelper.Clamp(index, 0, entries.Count - 1);
                comboBox.Value = entries[index].Key;
            }

            private void HandleTextInput()
            {
                PropertyListEntry selection = body.Selection;

                if (BindManager.IsChatOpen)
                {
                    if (!selection.InputOpen)
                    {
                        IBlockMember blockMember = selection.AssocMember;
                        selection.ValueText.SetText(blockMember.ValueText);
                        selection.OpenInput();
                        selection.WaitingForChatInput = false;
                    }
                }
                else
                {
                    selection.ValueText.SetText(textEntryWarning);
                    selection.WaitingForChatInput = true;
                }
            }
        }
    }
}