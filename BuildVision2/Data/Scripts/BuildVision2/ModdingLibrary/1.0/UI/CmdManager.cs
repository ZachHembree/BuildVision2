﻿using DarkHelmet.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DarkHelmet.UI
{
    /// <summary>
    /// Manages chat commands; singleton
    /// </summary>
    public sealed class CmdManager : ModBase.ComponentBase
    {
        private static CmdManager Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }

        private static CmdManager instance;
        private static readonly Regex cmdParser;
        private readonly List<Group> commandGroups;

        static CmdManager()
        {
            cmdParser = new Regex(@"((\s*?[\s,;|]\s*?)(\w+))+");
        }

        private CmdManager()
        {
            commandGroups = new List<Group>();
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

        /// <summary>
        /// Adds a <see cref="Group"/> with a given prefix and returns it. If a group already exists with the same prefix
        /// that group will be returned instead.
        /// </summary>
        public static Group AddOrGetCmdGroup(string prefix, IEnumerable<Command> commands = null)
        {
            prefix = prefix.ToLower();
            Group group = GetCmdGroup(prefix);

            if (group == null)
            {
                group = new Group(prefix, new List<Command>(commands));
                Instance.commandGroups.Add(group);
            }
            else if (commands != null)
                group.commands.AddRange(commands);

            return group;
        }

        /// <summary>
        /// Returns the command group using the given prefix. Returns null if the group doesn't exist.
        /// </summary>
        public static Group GetCmdGroup(string prefix)
        {
            prefix = prefix.ToLower();

            foreach (Group group in Instance.commandGroups)
                if (group.prefix == prefix)
                    return group;

            return null;
        }

        public static void AddCommand(string prefix, Command newCommand)
        {
            prefix = prefix.ToLower();
            Group group = GetCmdGroup(prefix);

            if (group != null)
                group.commands.Add(newCommand);
            else
                throw new Exception($"Could not add chat command. No group uses the prefix {prefix}.");
        }

        public static void AddCommands(string prefix, IEnumerable<Command> newCommands)
        {
            prefix = prefix.ToLower();
            Group group = GetCmdGroup(prefix);

            if (group != null)
                group.commands.AddRange(newCommands);
            else
                throw new Exception($"Could not add chat commands. No group uses the prefix {prefix}.");
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

            foreach (Group group in instance.commandGroups)
            {
                if (message.StartsWith(group.prefix))
                {
                    sendToOthers = false;

                    ModBase.RunSafeAction(() =>
                    {
                        if (TryParseCommand(message, out matches))
                        {
                            cmdName = matches[0];

                            foreach (Command cmd in group.commands)
                                if (cmd.cmdName == cmdName)
                                {
                                    cmdFound = true;

                                    if (cmd.needsArgs)
                                    {
                                        if (matches.Length > 1)
                                            cmd.action(matches.GetSubarray(1));
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
                    });
                }
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

        /// <summary>
        /// Stores a group of chat commands associated with a given prefix.
        /// </summary>
        public class Group
        {
            public readonly string prefix;
            public readonly List<Command> commands;

            public Group(string prefix, List<Command> commands = null)
            {
                this.prefix = prefix;

                if (commands != null)
                    this.commands = commands;
                else
                    this.commands = new List<Command>();
            }
        }

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
    }
}