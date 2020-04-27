using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class BvScrollMenu : HudElementBase
    {
        private void HandlePropertyInput()
        {
            if (BvBinds.Select.IsNewPressed)
                ToggleSelect();

            if (SharedBinds.Enter.IsNewPressed)
                ToggleTextBox();

            if (Selection != null && Selection.BlockMember is IBlockTextMember)
            {
                if (MyAPIGateway.Gui.ChatEntryVisible)
                {
                    if (waitingForChat && !Selection.value.InputOpen)
                    {
                        OpenProp();
                        waitingForChat = false;
                    }
                }
                else if (!waitingForChat && Selection.value.InputOpen)
                    CloseProp();
            }
        }

        /// <summary>
        /// Toggles property selection.
        /// </summary>
        private void ToggleSelect()
        {
            if (!PropOpen)
                OpenProp();
            else
                CloseProp();
        }

        /// <summary>
        /// Toggles the textbox of the selected property open/closed if the property supports text
        /// input.
        /// </summary>
        private void ToggleTextBox()
        {
            if (Selection.BlockMember is IBlockTextMember)
            {
                if (!PropOpen)
                {
                    if (!Selection.value.InputOpen)
                        waitingForChat = true;
                }
                else
                {
                    if (Selection.value.InputOpen)
                        waitingForChat = false;
                }
            }
        }

        /// <summary>
        /// Opens the currently highlighted property.
        /// </summary>
        private void OpenProp()
        {
            var blockAction = Selection.BlockMember as IBlockAction;

            if (blockAction == null)
            {
                PropOpen = true;

                if (Selection.BlockMember is IBlockTextMember)
                {
                    if (MyAPIGateway.Gui.ChatEntryVisible)
                        OpenTextInput();
                    else
                    {
                        waitingForChat = true;

                        if (!(Selection.BlockMember is IBlockScrollable))
                        {
                            Selection.value.TextBoard.Format = selectedText;
                            Selection.value.Text = "Open chat to continue";
                        }
                    }
                }
            }
            else
            {
                blockAction.Action();
            }
        }

        private void OpenTextInput()
        {
            waitingForChat = false;

            Selection.value.OpenInput();
            Selection.value.TextBoard.Format = selectedText;
            Selection.value.Text = Selection.BlockMember.Value;
        }

        /// <summary>
        /// Closes the currently selected property.
        /// </summary>
        private void CloseProp()
        {
            CloseTextInput();
            PropOpen = false;
        }

        private void CloseTextInput()
        {
            if (Selection != null)
            {
                var textMember = Selection.BlockMember as IBlockTextMember;

                if (textMember != null && Selection.value.InputOpen)
                    textMember.SetValueText(Selection.value.Text.ToString());

                waitingForChat = false;
                Selection.value.CloseInput();
            }
        }

    }
}
