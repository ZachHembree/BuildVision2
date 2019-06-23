using DarkHelmet.Game;
using DarkHelmet.UI;
using System;

namespace DarkHelmet.BuildVision2
{
    internal sealed partial class BvMain
    {
        private static string controlList;

        private CmdManager.Command[] GetChatCommands()
        {
            controlList = BindManager.GetControlListString();

            return new CmdManager.Command[]
            {
                new CmdManager.Command ("help",
                    () => ModBase.ShowMessageScreen("Help", GetHelpMessage())),
                new CmdManager.Command ("bindHelp",
                    () => ModBase.ShowMessageScreen("Bind Help", GetBindHelpMessage())),
                new CmdManager.Command ("printBinds",
                    () => ModBase.SendChatMessage(GetPrintBindsMessage())),
                new CmdManager.Command ("bind",
                    (string[] args) => KeyBinds.BindGroup.TryUpdateBind(args[0], args.GetSubarray(1))),
                new CmdManager.Command("resetBinds",
                    () => KeyBinds.Cfg = BindsConfig.Defaults),
                new CmdManager.Command ("save",
                    () => BvConfig.SaveStart()),
                new CmdManager.Command ("load",
                    () => BvConfig.LoadStart()),
                new CmdManager.Command("resetConfig",
                    () => BvConfig.ResetConfig()),
                new CmdManager.Command ("toggleApi",
                    () => PropertiesMenu.Cfg.forceFallbackHud = !PropertiesMenu.Cfg.forceFallbackHud),
                new CmdManager.Command ("toggleAutoclose",
                    () => Cfg.general.closeIfNotInView = !Cfg.general.closeIfNotInView),
                new CmdManager.Command ("toggleOpenWhileHolding",
                    () => Cfg.general.canOpenIfHolding = !Cfg.general.canOpenIfHolding),

                // Debug/Testing
                new CmdManager.Command ("open",
                    () => TryOpenMenu()),
                new CmdManager.Command ("close",
                    () => TryCloseMenu()),
                new CmdManager.Command ("toggleTestPattern",
                    () => HudUtilities.TestPattern.Toggle()),
                new CmdManager.Command ("reload",
                    () => ModBase.Instance.Close()),
                new CmdManager.Command("crash", Crash)
            };
        }

        private static void Crash()
        {
            throw new Exception("/bv2 crash was called.");
        }

        private static string GetHelpMessage()
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
                $"for more information. Chat command are not case sensitive and , ; | or spaces can be used to separate arguments. " +
                $"For information on chat command see the Build Vision 2 mod page on the Steam Workshop.\n";

            helpMessage += GetPrintBindsMessage();
            return helpMessage;
        }

        private static string GetBindHelpMessage()
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

        private static string GetPrintBindsMessage()
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