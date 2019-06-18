using DarkHelmet.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
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
        public readonly bool needsArgs;

        public Command(string cmdName, Func<string[], bool> argAction)
        {
            this.cmdName = cmdName.ToLower();
            action = argAction;
            needsArgs = true;
        }

        public Command(string cmdName, Action action)
        {
            this.cmdName = cmdName.ToLower();
            this.action = (string[] args) => { action(); return true; };
            needsArgs = false;
        }
    }

    /// <summary>
    /// Manages chat commands; singleton
    /// </summary>
    public sealed class CmdManager : ModBase.ComponentBase
    {
        public static string Prefix { get { return prefix; } set { if (value != null && value.Length > 0) prefix = value; } }

        private static CmdManager Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }

        private static CmdManager instance;
        private static string prefix = "/cmd";
        private static readonly Regex cmdParser;
        private readonly List<Command> commands;

        static CmdManager()
        {
            cmdParser = new Regex(@"((\s*?[\s,;|]\s*?)(\w+))+");
        }

        private CmdManager()
        {
            commands = new List<Command>();
            MyAPIGateway.Utilities.MessageEntered += MessageHandler;
        }

        private static void Init()
        {
            if (instance == null)
                instance = new CmdManager();
        }

        public override void Close()
        {
            MyAPIGateway.Utilities.MessageEntered -= MessageHandler;
            Instance = null;
        }

        public static void AddCommands(IEnumerable<Command> newCommands)
        {
            Instance.commands.AddRange(newCommands);
        }

        /// <summary>
        /// Recieves chat commands and attempts to execute them.
        /// </summary>
        private static void MessageHandler(string message, ref bool sendToOthers)
        {
            string cmdName;
            string[] matches;
            bool cmdFound = false;
            message = message.ToLower();

            if (message.StartsWith(Prefix))
            {
                sendToOthers = false;

                if (TryParseCommand(message, out matches))
                {
                    cmdName = matches[0];

                    foreach (Command cmd in Instance.commands)
                        if (cmd.cmdName == cmdName)
                        {
                            cmdFound = true;

                            if (cmd.needsArgs)
                            {
                                if (matches.Length > 1)
                                    ModBase.RunSafeAction(() => cmd.action(matches.GetSubarray(1)));
                                else
                                    ModBase.SendChatMessage("Invalid Command. This command requires an argument.");
                            }
                            else
                                cmd.action(null);

                            break;
                        }
                }

                if (!cmdFound)
                    ModBase.SendChatMessage("Command not recognised.");
            }
        }

        /// <summary>
        /// Parses list of arguments and their associated command name.
        /// </summary>
        public static bool TryParseCommand(string cmd, out string[] matches)
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