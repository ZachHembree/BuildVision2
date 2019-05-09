using System;
using Sandbox.ModAPI;
using System.Text.RegularExpressions;

namespace DarkHelmet.UI
{
    /// <summary>
    /// Stores chat command name and action
    /// </summary>
    public class Command
    {
        public readonly string cmdName;
        public readonly Func<string[], bool> action;
        public readonly bool takesArgs;

        public Command(string cmdName, Func<string[], bool> argAction)
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
    /// Manages chat commands; singleton
    /// </summary>
    internal sealed class CmdManager
    {
        public static CmdManager Instance { get; private set; }

        private readonly string prefix;
        private readonly Action<string> SendMessage;
        private readonly Regex cmdParser;
        private readonly Command[] commands;

        /// <summary>
        /// Instantiates commands and regex
        /// </summary>
        private CmdManager(Action<string> SendMessage, string prefix, Command[] commands)
        {
            this.SendMessage = SendMessage;
            this.prefix = prefix;
            this.commands = commands;
            cmdParser = new Regex(@"((\s*?[\s,;|]\s*?)(\w+))+");

            MyAPIGateway.Utilities.MessageEntered += MessageHandler;
        }

        /// <summary>
        /// Returns the current instance or creates one if necessary.
        /// </summary>
        public static void Init(Action<string> SendMessage, string prefix, Command[] commands)
        {
            if (Instance == null)
                Instance = new CmdManager(SendMessage, prefix, commands);
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
                                    cmd.action(Utilities.GetSubarray(matches, 1));
                                else
                                    SendMessage("Invalid Command. This command requires an argument.");
                            }
                            else
                                cmd.action(null);

                            break;
                        }
                }
                
                if (!cmdFound)
                    SendMessage("Command not recognised.");
            }
        }

        /// <summary>
        /// Parses list of arguments and their associated command name.
        /// </summary>
        public bool TryParseCommand(string cmd, out string[] matches)
        {
            Match match = cmdParser.Match(cmd);
            CaptureCollection captures = match.Groups[3].Captures;
            matches = new string[captures.Count];

            for (int n = 0; n < captures.Count; n++)
                matches[n] = captures[n].Value;

            return matches.Length > 0; 
        }       
    }
}