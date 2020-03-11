﻿using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RichHudFramework.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class BvMain
    {
        private static string controlList;

        private List<CmdManager.Command> GetChatCommands()
        {
            controlList = GetControlList();

            return new List <CmdManager.Command>
            {
                new CmdManager.Command ("help",
                    () => ExceptionHandler.ShowMessageScreen("Help", GetHelpMessage())),
                new CmdManager.Command ("bindHelp",
                    () => ExceptionHandler.ShowMessageScreen("Bind Help", GetBindHelpMessage())),
                new CmdManager.Command ("printBinds",
                    () => ExceptionHandler.SendChatMessage(GetPrintBindsMessage())),
                new CmdManager.Command ("bind",
                    (string[] args) => UpdateBind(args[0], args.GetSubarray(1))),
                new CmdManager.Command("resetBinds",
                    () => BvBinds.Cfg = BindsConfig.Defaults),
                new CmdManager.Command ("save",
                    () => BvConfig.SaveStart()),
                new CmdManager.Command ("load",
                    () => BvConfig.LoadStart()),
                new CmdManager.Command("resetConfig",
                    () => BvConfig.ResetConfig()),
                new CmdManager.Command ("toggleAutoclose",
                    () => Cfg.general.closeIfNotInView = !Cfg.general.closeIfNotInView),
                new CmdManager.Command ("toggleOpenWhileHolding",
                    () => Cfg.general.canOpenIfHolding = !Cfg.general.canOpenIfHolding),

                // Debug/Testing
                new CmdManager.Command ("open",
                    () => TryOpenMenu()),
                new CmdManager.Command ("close",
                    () => TryCloseMenu()),
                new CmdManager.Command ("reload",
                    () => Instance.Reload()),
                new CmdManager.Command("crash", 
                    Crash),
                new CmdManager.Command("printControlsToLog",
                    () => LogIO.WriteToLogStart($"Control List:\n{GetControlList()}")),
                new CmdManager.Command("export", 
                    ExportBlockData),
                new CmdManager.Command("import",
                    TryImportBlockData),
                new CmdManager.Command("echo",
                    x => ExceptionHandler.SendChatMessage($"echo: {x[0]}")),
            };
        }

        private void TryImportBlockData()
        {
            LocalFileIO blockIO = new LocalFileIO($"{target?.TypeID}.bin");
            byte[] byteData;

            if (blockIO.FileExists && blockIO.TryRead(out byteData) == null)
            {
                BlockData data;

                if (Utils.ProtoBuf.TryDeserialize(byteData, out data) == null)
                    target.ImportSettings(data);
            }
        }

        private void ExportBlockData()
        {
            LocalFileIO blockIO = new LocalFileIO($"{target?.TypeID}.bin");
            byte[] byteData;

            if (Utils.ProtoBuf.TrySerialize(target?.ExportSettings(), out byteData) == null)
                blockIO.TryWrite(byteData);
        }

        private static void Crash()
        {
            throw new Exception($"Crash chat command was called.");
        }

        private static void UpdateBind(string bindName, string[] controls)
        {
            IBind bind = BvBinds.BindGroup.GetBind(bindName);

            if (bind == null)
                ExceptionHandler.SendChatMessage("Error: The bind specified could not be found.");
            else
                bind.TrySetCombo(controls);
        }

        private static string GetControlList()
        {
            StringBuilder text = new StringBuilder(BindManager.Controls.Count * 10);

            for (int n = 0; n < BindManager.Controls.Count; n++)
            {
                var control = BindManager.Controls[n];

                if (control != null)
                {
                    if (control.DisplayName != control.Name)
                        text.AppendLine($"{control.DisplayName} ({control.Name})");
                    else
                        text.AppendLine(control.Name);
                }
            }
            return text.ToString();
        }

        private static string GetBindString(IBind bind)
        {
            IList<IControl> combo = bind.GetCombo();
            string bindString = "";

            for (int n = 0; n < combo.Count; n++)
            {
                if (n != combo.Count - 1)
                    bindString += combo[n].DisplayName + " + ";
                else
                    bindString += combo[n].DisplayName;
            }

            return bindString;
        }

        private static string GetHelpMessage()
        {
            string helpMessage =
                $"To open Build Vision, aim at the block you want to control and press press [{GetBindString(BvBinds.Open)}]. To close the menu, " +
                $"press [{GetBindString(BvBinds.Hide)}].\n\n" +

                $"[{GetBindString(BvBinds.ScrollUp)}] and [{GetBindString(BvBinds.ScrollDown)}] can be used to scroll up and down " +
                $"the list and to increment/decrement numerical properties when selected. You can scroll faster by pressing [{GetBindString(BvBinds.MultX)}]. " +
                $"To select a property in the menu press [{GetBindString(BvBinds.Select)}].\n\n" +

                $"By default, the menu will close if you move more than 10 meters (4 large blocks) from your target block." +
                $"The exact distance can be customized in the settings menu.\n\n" +     
                
                $"Settings Menu:\n" +
                $"The settings menu can be accessed by pressing ~ (tilde) while having chat open.\n\n" +

                $"The multiplier (mult) binds can be used to increase/decrease the rate at which numerical properties change with " +
                $"each tick of the scroll wheel.\n\n" +

                $"These are your current key binds:\n" +
                $"{GetPrintBindsMessage()}\n\n" +

                $"Chat Commands:\n" +
                $"Chat commands are not case sensitive and , ; | or spaces can be used to separate arguments.\n\n" +

                $"help -- You are here.\n" +
                $"bindHelp -- Help menu for changing keybinds\n" +
                $"printBinds -- Prints current key bind configuration to chat.\n" +
                $"bind [bindName] [control1] [control2] [control3] (see bindHelp for more info)\n" +
                $"save -– Saves the current configuration\n" +
                $"load -- Loads configuration from the config file\n" +
                $"resetConfig -- Resets all settings to default\n" +
                $"resetBinds -- Resets all keybinds\n\n" +

                $"For more information, see the Build Vision workshop page.";

            return helpMessage;
        }

        private static string GetBindHelpMessage()
        {
            string helpMessage =
                $"Key binds can be changed using either the Rich Hud Terminal or the /bv2 bind chat command. To access the terminal, open chat " +
                $"and press ~ (tilde).\n\n" +

                $"The /bv2 printBinds command can be used to print your current bind cfg to chat. No more than three controls " +
                $"can be used for any one bind.\n\n" +

                $"Syntax:\n" +
                $"bind: /bv2 bind [bindName] [control1] [control2] [control3].\n\n " +

                $"Examples:\n" +
                $"/bv2 bind scrollup pageup\n" +
                $"/bv2 bind scrolldown pagedown\n\n" +

                $"You cna reset your binds by either pressing the defaults button in the terminal or by" +
                $"using the chat command /bv2 resetBinds.\n" +
                $"These are your current key binds:\n" +
                $"{GetPrintBindsMessage()}\n\n" +

                $"The following controls can be used to create binds (probably):\n\n" +

                $"{controlList}";

            return helpMessage;
        }

        private static string GetPrintBindsMessage()
        {
            string bindHelp =
                "\n---Build Vision 2 Binds---\n" +
                $"Open: [{GetBindString(BvBinds.Open)}]\n" +
                $"Close: [{GetBindString(BvBinds.Hide)}]\n" +
                $"Select [{GetBindString(BvBinds.Select)}]\n" +
                $"Scroll Up: [{GetBindString(BvBinds.ScrollUp)}]\n" +
                $"Scroll Down: [{GetBindString(BvBinds.ScrollDown)}]]\n" +
                "---Multipliers---\n" +
                $"MultX: [{GetBindString(BvBinds.MultX)}]\n" +
                $"MultY: [{GetBindString(BvBinds.MultY)}]\n" +
                $"MultZ: [{GetBindString(BvBinds.MultZ)}]";

            return bindHelp;
        }
    }
}