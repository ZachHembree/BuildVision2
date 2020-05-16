using RichHudFramework;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Sandbox.ModAPI;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class BvMain
    {
        private static string controlList;

        private void AddChatCommands()
        {
            controlList = GetControlList();
            CmdManager.GetOrCreateGroup("/bv2", new CmdGroupInitializer 
            {
                { "help", x => ExceptionHandler.ShowMessageScreen("Help", GetHelpMessage()) },
                { "bindHelp", x => ExceptionHandler.ShowMessageScreen("Bind Help", GetBindHelpMessage()) },
                { "printBinds", x => ExceptionHandler.SendChatMessage(GetPrintBindsMessage()) },
                { "bind", x => UpdateBind(x[0], x.GetSubarray(1)), 2 },
                { "resetBinds", x => BvBinds.Cfg = BindsConfig.Defaults },
                { "save", x => BvConfig.SaveStart() },
                { "load", x => BvConfig.LoadStart() },
                { "resetConfig", x => BvConfig.ResetConfig() },
                { "toggleAutoclose", x => Cfg.general.closeIfNotInView = !Cfg.general.closeIfNotInView },
                { "toggleOpenWhileHolding", x => Cfg.general.canOpenIfHolding = !Cfg.general.canOpenIfHolding },

                // Debug/Testing
                { "open", x => PropertiesMenu.TryOpenMenu() },
                { "close", x => PropertiesMenu.HideMenu() },
                { "reload", x => Instance.Reload() },
                { "crash", x => Crash() },
                { "printControlsToLog", x => LogIO.WriteToLogStart($"Control List:\n{GetControlList()}") },
                { "export", x => ExportBlockData() },
                { "import", x=> TryImportBlockData() },
                { "checkType", x => ExceptionHandler.SendChatMessage($"Block Type: {(PropertiesMenu.Target?.SubtypeId.ToString() ?? "No Target")}") },
                { "targetBench", TargetBench, 1 },
                { "echo", x => ExceptionHandler.SendChatMessage($"echo: {x[0]}") },
            });
        }

        private static void UpdateBind(string bindName, string[] controls)
        {
            IBind bind = BvBinds.OpenGroup.GetBind(bindName);

            if (bind == null)
                bind = BvBinds.MainGroup.GetBind(bindName);

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
                $"press [{GetBindString(BvBinds.Hide)}]. Alternatively, pressing and holding [{GetBindString(BvBinds.Peek)}] will allow you to peek " +
                $"at a block's current status.\n\n" +

                $"[{GetBindString(BvBinds.ScrollUp)}] and [{GetBindString(BvBinds.ScrollDown)}] can be used to scroll up and down " +
                $"the list and to increment/decrement numerical properties when selected. To select a property in the menu press " +
                $"[{GetBindString(BvBinds.Select)}].\n\n" +

                $"By default, the menu will close if you move more than 10 meters (4 large blocks) from your target block. " +
                $"The exact distance can be customized in the settings menu.\n\n" +

                $"Main Binds:\n" +
                $"    Open Menu: [{GetBindString(BvBinds.Open)}]\n" +
                $"    Close Menu: [{GetBindString(BvBinds.Hide)}]\n" +
                $"    Select Property/Trigger Action: [{GetBindString(BvBinds.Select)}]\n" +
                $"    Scroll Up: [{GetBindString(BvBinds.ScrollUp)}]\n" +
                $"    Scroll Down: {GetBindString(BvBinds.ScrollDown)}\n\n" +

                $"Multiplier Binds:\n" +
                $"    MultX (x{BvConfig.Current.block.floatMult.X}): {GetBindString(BvBinds.MultX)}\n" +
                $"    MultY (x{BvConfig.Current.block.floatMult.Y}): {GetBindString(BvBinds.MultY)}\n" +
                $"    MultZ (x{BvConfig.Current.block.floatMult.Z}): {GetBindString(BvBinds.MultZ)}\n\n" +

                $"The multiplier (mult) binds can be used to increase/decrease the rate at which numerical properties change with " +
                $"each tick of the scroll wheel.\n\n" +

                $"Copy/Paste Binds:\n" +
                $"    Toggle Copy Mode: [{GetBindString(BvBinds.ToggleSelectMode)}]\n" +
                $"    Select All Properties: [{GetBindString(BvBinds.SelectAll)}]\n" +
                $"    Copy Selected Properties: [{GetBindString(BvBinds.CopySelection)}]\n" +
                $"    Paste Copied Properties: [{GetBindString(BvBinds.PasteProperties)}]\n" +
                $"    Undo Paste: [{GetBindString(BvBinds.UndoPaste)}]\n\n" +

                $"The copy/paste binds are used to copy properties between compatible block types. When in copy mode, you'll be " +
                $"able to select/deselect properties one at a time using the scroll and select binds or you can select them all at " +
                $"once using the select all bind.\n\n" +

                $"Settings Menu:\n" +
                $"The settings menu can be accessed by pressing F1 while having chat open. From there, you can configure block " +
                $"targeting, change UI settings and configure your keybinds.\n\n" +

                $"Chat Commands:\n" +
                $"Chat commands are not case sensitive and , ; | or spaces can be used to separate arguments.\n\n" +

                $"• help -- You are here.\n" +
                $"• bindHelp -- Help menu for changing keybinds\n" +
                $"• printBinds -- Prints current key bind configuration to chat.\n" +
                $"• bind [bindName] [control1] [control2] [control3] (see bindHelp for more info)\n" +
                $"• save -– Saves the current configuration\n" +
                $"• load -- Loads configuration from the config file\n" +
                $"• resetBinds -- Resets all keybinds\n" +
                $"• resetConfig -- Resets all settings to default\n\n" +

                $"For more information, see the Build Vision 2 workshop page.";

            return helpMessage;
        }

        private static string GetBindHelpMessage()
        {
            string helpMessage =
                $"Key binds can be changed using either the Rich Hud Terminal or the /bv2 bind chat command. To access the terminal, open chat " +
                $"and press F1.\n\n" +

                $"The /bv2 printBinds command can be used to print your current bind cfg to chat. No more than three controls " +
                $"can be used for any one bind.\n\n" +

                $"Command Syntax:\n" +
                $"bind: /bv2 bind [bindName] [control1] [control2] [control3].\n\n " +

                $"Examples:\n" +
                $"/bv2 bind scrollup pageup\n" +
                $"/bv2 bind scrolldown pagedown\n\n" +

                $"You can reset your binds by either pressing the defaults button in the terminal or by " +
                $"using the chat command /bv2 resetBinds.\n\n" +
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
                $"Select: [{GetBindString(BvBinds.Select)}]\n" +
                $"Scroll Up: [{GetBindString(BvBinds.ScrollUp)}]\n" +
                $"Scroll Down: [{GetBindString(BvBinds.ScrollDown)}]\n" +
                $"---Multipliers---\n" +
                $"MultX: [{GetBindString(BvBinds.MultX)}]\n" +
                $"MultY: [{GetBindString(BvBinds.MultY)}]\n" +
                $"MultZ: [{GetBindString(BvBinds.MultZ)}]\n" +
                $"---Copy/Paste---\n" +
                $"ToggleSelectMode: [{GetBindString(BvBinds.ToggleSelectMode)}]\n" +
                $"SelectAll: [{GetBindString(BvBinds.SelectAll)}]\n" +
                $"CopySelection: [{GetBindString(BvBinds.CopySelection)}]\n" +
                $"PasteProperties: [{GetBindString(BvBinds.PasteProperties)}]\n" +
                $"UndoPaste: [{GetBindString(BvBinds.UndoPaste)}]";

            return bindHelp;
        }

        private void TryImportBlockData()
        {
            LocalFileIO blockIO = new LocalFileIO($"{PropertiesMenu.Target?.TypeID}.bin");
            byte[] byteData;

            if (blockIO.FileExists && blockIO.TryRead(out byteData) == null)
            {
                BlockData data;

                if (Utils.ProtoBuf.TryDeserialize(byteData, out data) == null)
                    PropertiesMenu.Target.ImportSettings(data);
            }
        }

        private void ExportBlockData()
        {
            LocalFileIO blockIO = new LocalFileIO($"{PropertiesMenu.Target?.TypeID}.bin");
            byte[] byteData;

            if (Utils.ProtoBuf.TrySerialize(PropertiesMenu.Target?.ExportSettings(), out byteData) == null)
                blockIO.TryWrite(byteData);
        }

        private void TargetBench(string[] args)
        {
            IMyTerminalBlock tblock;

            if (PropertiesMenu.TryGetTargetedBlock(100d, out tblock))
            {
                int iterations;
                bool getProperties = false;

                int.TryParse(args[0], out iterations);

                if (args.Length > 1)
                    bool.TryParse(args[1], out getProperties);

                Utils.Stopwatch timer = new Utils.Stopwatch();
                timer.Start();

                TerminalGrid grid = new TerminalGrid();
                grid.SetGrid(tblock.CubeGrid);

                for (int n = 0; n < iterations; n++)
                {
                    IMyTerminalBlock temp;
                    PropertiesMenu.TryGetTargetedBlock(100d, out temp);
                    PropertyBlock pBlock = new PropertyBlock(grid, tblock);

                    if (getProperties)
                        pBlock.GetEnabledElementCount();
                }

                timer.Stop();
                ExceptionHandler.SendChatMessage
                (
                    $"Target Bench:\n" +
                    $"\tGetProperties: {getProperties}\n" +
                    $"\tTime: {(timer.ElapsedTicks / (double)TimeSpan.TicksPerMillisecond):G6} ms\n" +
                    $"\tIterations: {iterations}"
                );
            }
            else
                ExceptionHandler.SendChatMessage($"Cant start target bench. No target found.");
        }

        private static void Crash()
        {
            throw new Exception($"Crash chat command was called.");
        }
    }
}