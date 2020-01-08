using RichHudFramework;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace DarkHelmet.BuildVision2
{
    internal sealed partial class BvMain
    {
        private static string controlList;

        private List<CmdManager.Command> GetChatCommands()
        {
            controlList = GetControlList();

            return new List <CmdManager.Command>
            {
                new CmdManager.Command ("help",
                    () => ShowMessageScreen("Help", GetHelpMessage())),
                new CmdManager.Command ("bindHelp",
                    () => ShowMessageScreen("Bind Help", GetBindHelpMessage())),
                new CmdManager.Command ("printBinds",
                    () => SendChatMessage(GetPrintBindsMessage())),
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
                    () => Instance.Close()),
                new CmdManager.Command("crash", 
                    Crash),
                new CmdManager.Command("printControlsToLog",
                    () => WriteToLogStart($"Control List:\n{GetControlList()}"))
            };
        }

        private static void Crash()
        {
            throw new Exception($"Crash chat command was called.");
        }

        private static void UpdateBind(string bindName, string[] controls)
        {
            IBind bind = BvBinds.BindGroup.GetBind(bindName);

            if (bind == null)
                SendChatMessage("Error: The bind specified could not be found.");
            else
                bind.TrySetCombo(controls);
        }

        private static string GetControlList()
        {
            StringBuilder text = new StringBuilder(BindManager.Controls.Count * 10);

            foreach (IControl control in BindManager.Controls)
            {
                if (control.DisplayName != control.Name)
                    text.AppendLine($"{control.DisplayName} ({control.Name})");
                else
                    text.AppendLine(control.Name);
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
                $"To open Build Vision, press [{GetBindString(BvBinds.Open)}] while aiming at a block; to close the menu, press " +
                $"[{GetBindString(BvBinds.Hide)}] or press the open bind again but without pointing at a valid block (like armor).\n\n" +
                $"The [{GetBindString(BvBinds.ScrollUp)}] and [{GetBindString(BvBinds.ScrollDown)}] binds can be used to scroll up and down " +
                $"in the menu and to change the terminal settings of the selected block. To select a setting in the menu press " +
                $"[{GetBindString(BvBinds.Select)}]. Pressing the select bind on an action will trigger it (a setting without a number " +
                $"or On/Off value).\n\n" +
                $"If you move more than 10 meters (4 large blocks) from your target block, the menu will " +
                $"automatically close. The rate at which a selected terminal value changes with each tick of the scroll wheel can " +
                $"be changed using the multiplier binds listed below.\n\n" +
                $"Chat Commands:\n" +
                $"Chat commands are not case sensitive and , ; | or spaces can be used to separate arguments.\n\n" +
                $"help -- You are here.\n" +
                $"bindHelp -- Help menu for changing keybinds\n" +
                $"printBinds -- Prints current key bind configuration to chat.\n" +
                $"rebind [bindName] -- Opens the rebind menu\n" +
                $"bind [bindName] [control1] [control2] [control3] (see bindHelp for more info)\n" +
                $"save -– Saves the current configuration\n" +
                $"load -- Loads configuration from the config file\n" +
                $"resetConfig -- Resets all settings to default\n" +
                $"resetBinds -- Resets all keybinds\n\n" +
                $"For more information on chat commands, see the Build Vision workshop page.";

            return helpMessage;
        }

        private static string GetBindHelpMessage()
        {
            string helpMessage =
                $"Key binds can be changed using either the rebind menu or the /bv2 bind chat command. The rebind menu can be accessed either " +
                $"through the mod menu or the /bv2 rebind command.\n\n" +
                $"To see your current bind settings use /bv2 printBinds. No more than three controls " +
                $"can be used for any one bind.\n\n" +
                $"Syntax:\n" +
                $"rebind: /bv2 rebind [bindName]\n" +
                $"bind: /bv2 bind [bindName] [control1] [control2] [control3].\n\n " +
                $"Examples:\n" +
                $"/bv2 rebind open\n" +
                $"/bv2 bind scrollup pageup\n" +
                $"/bv2 bind scrolldown pagedown\n\n" +
                $"To reset your binds to the default settings type /bv2 resetBinds in chat.\n" +
                $"These are your current bind settings:\n" +
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