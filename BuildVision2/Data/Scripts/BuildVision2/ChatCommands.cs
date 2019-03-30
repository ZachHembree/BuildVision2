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

        private static Binds Binds { get { return Binds.Instance; } }
        private static BvMain Main { get { return BvMain.Instance; } }
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
        private ChatCommands(string prefix)
        {
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
                    (string[] args) => Binds.TryUpdateBind(args[0], GetSubarray(args, 1))),
                new Command("resetBinds",
                    () => Binds.TryUpdateConfig(BindsConfig.Defaults)),
                new Command ("save",
                    () => Main.SaveConfig()),
                new Command ("load",
                    () => Main.LoadConfig()),
                new Command("resetConfig",
                    () => Main.ResetConfig()),
                new Command ("toggleApi",
                    () => Main.forceFallbackHud = !Main.forceFallbackHud),
                new Command ("toggleAutoclose",
                    () => Main.closeIfNotInView = !Main.closeIfNotInView),

                // Debug/Testing
                new Command ("open",
                    () => Main.TryOpenMenu()),
                new Command ("close", 
                    () => Main.TryCloseMenu())
            };

            controlList = Binds.GetControlListString();
            MyAPIGateway.Utilities.MessageEntered += MessageHandler;
        }

        /// <summary>
        /// Returns the current instance or creates one if necessary.
        /// </summary>
        public static ChatCommands GetInstance(string prefix)
        {
            if (Instance == null)
                Instance = new ChatCommands(prefix);

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
            IKeyBind[] keyBinds = Binds.KeyBinds;

            for (int n = 0; n < keyBinds.Length; n++)
                output += keyBinds[n].Name + ": [" + keyBinds[n].BindString + "]\n";

            MyAPIGateway.Utilities.ShowMessage("BV2 Keybinds", output);
        }

        private string GetHelpMessage()
        {
            string helpMessage =
                $"To open Build Vision press [{Binds.open.BindString}] while aiming at a block; to close the menu, press " +
                $"[{Binds.close.BindString}] or press the open bind again but without pointing at a valid block (like armor). " +
                $"The [{Binds.scrollUp.BindString}] and [{Binds.scrollDown.BindString}] binds can be used to scroll up and down " +
                $"in the menu and to change the terminal settings of the selected block. To select a setting in the menu press " +
                $"[{Binds.select.BindString}]. Pressing the select bind on an action will trigger it (a setting without a number " +
                $"or On/Off value). If you move more than 10 meters (4 large blocks) from your target block, the menu will " +
                $"automatically close. The rate at which a selected terminal value changes with each tick of the scroll wheel can " +
                $"be changed using the multiplier binds listed below.\n\n" +
                $"Key binds can be changed using the /bv2 bind chat command. Enter /bv2 bindHelp in chat for more information. Chat " +
                $"commands are not case sensitive and , ; | or spaces can be used to separate arguments. For information on chat " +
                $"commands see the Build Vision 2 mod page on the Steam Workshop.\n";

            helpMessage += GetPrintBindsMessage();
            return helpMessage;
        }

        private string GetBindHelpMessage()
        {
            string helpMessage = $"The syntax of the /bv2 bind command is as follows (without brackets): /bv2 bind [bindName] [control1] " +
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

        private string GetPrintBindsMessage()
        {
            string bindHelp = 
                "\n---Build Vision 2 Binds---\n" +
                $"Open: [{Binds.open.BindString}]\n" +
                $"Close: [{Binds.close.BindString}]\n" +
                $"Select [{Binds.select.BindString}]\n" +
                $"Scroll Up: [{Binds.scrollUp.BindString}]\n" +
                $"Scroll Down: [{Binds.scrollDown.BindString}]]\n" +
                "---Multipliers---\n" +
                $"MultX: [{Binds.multX.BindString}]\n" +
                $"MultY: [{Binds.multY.BindString}]\n" +
                $"MultZ: [{Binds.multZ.BindString}]";

            return bindHelp;
        }
    }
}