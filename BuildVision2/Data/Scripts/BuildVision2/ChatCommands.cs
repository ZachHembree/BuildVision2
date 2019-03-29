using System;
using Sandbox.ModAPI;
using System.Text.RegularExpressions;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Manages chat commands; singleton
    /// </summary>
    internal sealed class ChatCommands
    {
        public static ChatCommands Instance { get; private set; }

        private readonly BvMain main;
        private readonly Binds binds;
        private readonly string prefix, controlList;
        private readonly Regex cmdParser;
        private readonly Command[] commands;
        private delegate bool CmdAction(string[] args);

        /// <summary>
        /// Stores chat command name and action
        /// </summary>
        private class Command
        {
            public readonly string cmdName;
            public readonly CmdAction action;
            public readonly bool takesArgs;

            public Command(string cmdName, CmdAction argAction)
            {
                this.cmdName = cmdName.ToLower();
                action = argAction;
                takesArgs = true;
            }

            public Command(string cmdName, Action action)
            {
                this.cmdName = cmdName.ToLower();
                this.action = (string[] args) => { action(); return true; };
                takesArgs = false;
            }
        }

        /// <summary>
        /// Instantiates commands and regex
        /// </summary>
        private ChatCommands(Binds binds, string prefix)
        {
            main = BvMain.GetInstance();
            this.binds = binds;
            this.prefix = prefix;
            cmdParser = new Regex(@"((\s*?[\s,;|]\s*?)(\w+))+");

            commands = new Command[]
            {
                new Command ("help",
                    () => MyAPIGateway.Utilities.ShowMissionScreen("Build Vision 2", "Help", "", GetHelpMessage())),
                new Command ("bindHelp",
                    () => MyAPIGateway.Utilities.ShowMissionScreen("Build Vision 2", "Bind Help", "", GetBindHelpMessage())),
                new Command ("printBinds",
                    () => MyAPIGateway.Utilities.ShowMessage("", GetPrintBindsMessage())),
                new Command ("bind",
                    (string[] args) => binds.TryUpdateBind(args[0], GetSubarray(args, 1))),
                new Command("resetBinds",
                    () => binds.UpdateConfig(BindsConfig.Defaults)),
                new Command ("save",
                    () => main.SaveConfig()),
                new Command ("load",
                    () => main.LoadConfig()),
                new Command("resetConfig",
                    () => main.ResetConfig()),
                new Command ("toggleApi",
                    () => main.forceFallbackHud = !main.forceFallbackHud),
                new Command ("toggleAutoclose",
                    () => main.closeIfNotInView = !main.closeIfNotInView),

                // Debug/Testing
                new Command ("open",
                    () => main.TryOpenMenu()),
                new Command ("close", 
                    () => main.TryCloseMenu())
            };

            controlList = binds.GetControlListString();
            MyAPIGateway.Utilities.MessageEntered += MessageHandler;
        }

        /// <summary>
        /// Returns the current instance or creates one if necessary.
        /// </summary>
        public static ChatCommands GetInstance(Binds binds, string prefix)
        {
            if (Instance == null && binds != null)
                Instance = new ChatCommands(binds, prefix);

            return Instance;
        }

        public void Close()
        {
            MyAPIGateway.Utilities.MessageEntered -= MessageHandler;
            Instance = null;
        }

        /// <summary>
        /// Recieves chat commands and attempts to execute them.
        /// </summary>
        private void MessageHandler(string message, ref bool sendToOthers)
        {
            string cmdName;
            string[] matches;
            bool cmdFound = false;
            message = message.ToLower();

            if (message.StartsWith(prefix))
            {
                sendToOthers = false;

                if (TryParseCommand(message, out matches))
                {
                    cmdName = matches[0];

                    foreach (Command cmd in commands)
                        if (cmd.cmdName == cmdName)
                        {
                            cmdFound = true;

                            if (cmd.takesArgs)
                            {
                                if (matches.Length > 1)
                                    cmd.action(GetSubarray(matches, 1));
                                else
                                    MyAPIGateway.Utilities.ShowMessage("Build Vision 2", "Invalid Command. This command requires an argument.");
                            }
                            else
                                cmd.action(null);

                            break;
                        }
                }
                
                if (!cmdFound)
                    MyAPIGateway.Utilities.ShowMessage("Build Vision 2", "Command not recognised.");
            }
        }

        private static T[] GetSubarray<T>(T[] arr, int i)
        {
            T[] trimmed = new T[arr.Length - i];

            for (int n = i; n < arr.Length; n++)
                trimmed[n - i] = arr[n];

            return trimmed;
        }

        /// <summary>
        /// Parses list of arguments and their associated command name.
        /// </summary>
        private bool TryParseCommand(string cmd, out string[] matches)
        {
            Match match = cmdParser.Match(cmd);
            CaptureCollection captures = match.Groups[3].Captures;
            matches = new string[captures.Count];

            for (int n = 0; n < captures.Count; n++)
                matches[n] = captures[n].Value;

            for (int a = 2; a < matches.Length; a++)
                for (int b = 2; b < matches.Length; b++)
                {
                    if (a != b && matches[a] == matches[b])
                    {
                        MyAPIGateway.Utilities.ShowMessage("Build Vision 2", "Invalid Command. The same argument cannot be used more than once.");
                        return false;
                    }
                }

            return matches.Length > 0; 
        }

        /// <summary>
        /// Prints the names and bind strings of all keybinds.
        /// </summary>
        private void PrintBinds()
        {
            string output = "\n";
            IKeyBind[] keyBinds = binds.KeyBinds;

            for (int n = 0; n < keyBinds.Length; n++)
                output += keyBinds[n].Name + ": [" + keyBinds[n].BindString + "]\n";

            MyAPIGateway.Utilities.ShowMessage("BV2 Keybinds", output);
        }

        private string GetHelpMessage()
        {
            string helpMessage =
                $"To open Build Vision press [{binds.open.BindString}] while aiming at a block; to close the menu, press [{binds.close.BindString}] " +
                $"or press the open bind again but without pointing at a valid block (like armor). To select a setting in the menu press " +
                $"[{binds.select.BindString}]. Once a setting is selected, the [{binds.scrollUp.BindString}] and [{binds.scrollDown.BindString}] binds " +
                "can be used to scroll up and down in the menu and to change the terminal settings of the selected block.\n" +
                "Pressing the select bind on an action will trigger it (a setting without a number or On/Off value, like Attach/Detach head or Reverse). " +
                "Key binds can be changed using the /bv2 binds chat command. Enter /bv2 bindHelp in chat for more information. Chat commands are not case " +
                "sensitive and , ; | or spaces can be used to separate arguments. For information on chat commands see the Build Vision 2 mod page on " +
                "the Steam Workshop.\n" +
                "Note: If you move more than 10 meters(4 large blocks) from your target block, the menu will automatically close.\n" +
                "The rate at which a selected terminal value changes with each tick of the scroll wheel can be changed using the multiplier binds " +
                "listed below.\n";

            helpMessage += GetPrintBindsMessage();
            return helpMessage;
        }

        private string GetBindHelpMessage()
        {
            string helpMessage = $"The syntax of the /bv2 bind command is as follows (without brackets): /bv2 bind [bindName] [controlOne] " +
                $"[controlTwo] [controlThree]. To see your current bind settings use the command /bv2 printBinds. No more than three binds " +
                $"can be used for any one control; frankly, any more than three would be stupid.\n" +
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

        private string GetPrintBindsMessage()
        {
            string bindHelp = 
                "\n---Build Vision 2 Binds---\n" +
                $"Open: [{binds.open.BindString}]\n" +
                $"Close: [{binds.close.BindString}]\n" +
                $"Select [{binds.select.BindString}]\n" +
                $"Scroll Up: [{binds.scrollUp.BindString}]\n" +
                $"Scroll Down: [{binds.scrollDown.BindString}]]\n" +
                "---Multipliers---\n" +
                $"MultX: [{binds.multX.BindString}]\n" +
                $"MultY: [{binds.multY.BindString}]\n" +
                $"MultZ: [{binds.multZ.BindString}]";

            return bindHelp;
        }
    }
}