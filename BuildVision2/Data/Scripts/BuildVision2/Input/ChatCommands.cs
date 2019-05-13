using System;
using Sandbox.ModAPI;
using System.Text.RegularExpressions;
using DarkHelmet.UI;
using DarkHelmet.Game;

namespace DarkHelmet.BuildVision2
{
    internal static class ChatCommands
    {
        private static BuildVision2 Main { get { return BuildVision2.Instance as BuildVision2; } }
        public static CmdManager CmdManager { get { return CmdManager.Instance; } }
        private static HudUtilities HudElements { get { return HudUtilities.Instance; } }

        private static readonly string controlList;

        static ChatCommands()
        {
            CmdManager.Prefix = "/bv2";

            CmdManager.AddCommands(new Command[]
            {
                new Command ("help",
                    () => ModBase.ShowMessageScreen("Help", GetHelpMessage())),
                new Command ("bindHelp",
                    () => ModBase.ShowMessageScreen("Bind Help", GetBindHelpMessage())),
                new Command ("printBinds",
                    () => ModBase.SendChatMessage(GetPrintBindsMessage())),
                new Command ("bind",
                    (string[] args) => KeyBinds.BindManager.TryUpdateBind(args[0], Utilities.GetSubarray(args, 1))),
                new Command("resetBinds",
                    () => KeyBinds.Cfg = BindsConfig.Defaults),
                new Command ("save",
                    () => Main.SaveConfig()),
                new Command ("load",
                    () => Main.LoadConfig()),
                new Command("resetConfig",
                    () => Main.ResetConfig()),
                new Command ("toggleApi",
                    () => PropertiesMenu.Cfg.forceFallbackHud = !PropertiesMenu.Cfg.forceFallbackHud),
                new Command ("toggleAutoclose",
                    () => Main.Cfg.general.closeIfNotInView = !Main.Cfg.general.closeIfNotInView),
                new Command ("toggleOpenWhileHolding",
                    () => Main.Cfg.general.canOpenIfHolding = !Main.Cfg.general.canOpenIfHolding),

                // Debug/Testing
                new Command ("open",
                    () => Main.TryOpenMenu()),
                new Command ("close",
                    () => Main.TryCloseMenu()),
                new Command ("toggleTestPattern",
                    () => HudElements.TestPattern.Toggle()),
                new Command ("reload",
                    () => Main.Close())
            });

            controlList = BindManager.GetControlListString();
        }

        /// <summary>
        /// Prints the names and bind strings of all keybinds.
        /// </summary>
        private static void PrintBinds()
        {
            string output = "Keybinds\n";

            for (int n = 0; n < KeyBinds.BindManager.Count; n++)
                output += KeyBinds.BindManager[n].Name + ": [" + KeyBinds.BindManager[n].BindString + "]\n";

            ModBase.SendChatMessage(output);
        }

        public static string GetHelpMessage()
        {
            string helpMessage =
                $"To open Build Vision press [{KeyBinds.Open.BindString}] while aiming at a block; to close the menu, press " +
                $"[{KeyBinds.Hide.BindString}] or press the open bind again but without pointing at a valid block (like armor). " +
                $"The [{KeyBinds.ScrollUp.BindString}] and [{KeyBinds.ScrollDown.BindString}] binds can be used to scroll up and down " +
                $"in the menu and to change the terminal settings of the selected block. To select a setting in the menu press " +
                $"[{KeyBinds.Select.BindString}]. Pressing the select bind on an action will trigger it (a setting without a number " +
                $"or On/Off value). If you move more than 10 meters (4 large blocks) from your target block, the menu will " +
                $"automatically close. The rate at which a selected terminal value changes with each tick of the scroll wheel can " +
                $"be changed using the multiplier binds listed below.\n\n" +
                $"Key binds can be changed using the /bv2 bind chat command or through the F2 mod menu. Enter /bv2 bindHelp in chat " +
                $"for more information. Chat commands are not case sensitive and , ; | or spaces can be used to separate arguments. " +
                $"For information on chat commands see the Build Vision 2 mod page on the Steam Workshop.\n";

            helpMessage += GetPrintBindsMessage();
            return helpMessage;
        }

        public static string GetBindHelpMessage()
        {
            string helpMessage =
                $"The syntax of the /bv2 bind command is as follows (without brackets): /bv2 bind [bindName] [control1] " +
                $"[control2] [control3]. To see your current bind settings use the command /bv2 printBinds. No more than three controls " +
                $"can be used for any one bind.\n\n" +
                $"Examples:\n" +
                $"/bv2 bind open control alt\n" +
                $"/bv2 bind scrollup pageup\n" +
                $"/bv2 bind scrolldown pagedown\n\n" +
                $"To reset your binds to the default settings type /bv2 resetBinds in chat.\n" +
                $"These are your current bind settings:\n" +
                $"{GetPrintBindsMessage()}\n\n" +
                $"The following controls can be used to create binds (probably):\n\n" +
                $"{controlList}";

            return helpMessage;
        }

        public static string GetPrintBindsMessage()
        {
            string bindHelp =
                "\n---Build Vision 2 Binds---\n" +
                $"Open: [{KeyBinds.Open.BindString}]\n" +
                $"Close: [{KeyBinds.Hide.BindString}]\n" +
                $"Select [{KeyBinds.Select.BindString}]\n" +
                $"Scroll Up: [{KeyBinds.ScrollUp.BindString}]\n" +
                $"Scroll Down: [{KeyBinds.ScrollDown.BindString}]]\n" +
                "---Multipliers---\n" +
                $"MultX: [{KeyBinds.MultX.BindString}]\n" +
                $"MultY: [{KeyBinds.MultY.BindString}]\n" +
                $"MultZ: [{KeyBinds.MultZ.BindString}]";

            return bindHelp;
        }
    }
}