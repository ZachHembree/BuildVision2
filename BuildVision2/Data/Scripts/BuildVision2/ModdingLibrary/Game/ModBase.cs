using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using System;
using System.Collections.Generic;
using DarkHelmet.IO;
using System.Runtime.CompilerServices;

namespace DarkHelmet.Game
{
    /// <summary>
    /// Base class for mods using this library. 
    /// </summary>
    public abstract class ModBase : MySessionComponentBase
    {
        public static string ModName { get; protected set; }
        public static string LogFileName { get; protected set; }
        public static bool RunOnServer { get; protected set; }
        protected static ModBase Instance { get; private set; }

        protected static readonly List<Action> closeActions, drawActions, inputActions, updateActions;

        private LogIO modLog;
        private bool crashed, isDedicated, isServer, closing;

        static ModBase()
        {
            ModName = "Mod Base";
            RunOnServer = false;
            TaskPool.MaxTasksRunning = 2;

            closeActions = new List<Action>();
            drawActions = new List<Action>();
            inputActions = new List<Action>();
            updateActions = new List<Action>();
        }

        public ModBase()
        {
            if (Instance != null)
                throw new Exception("Only one instance of type ModBase can exist at a given time.");

            modLog = new LogIO(LogFileName);
            crashed = false;
            isDedicated = false;
            isServer = false;
            closing = false;
        }

        public sealed override void Draw() =>
            RunUpdateActions(drawActions);

        public sealed override void HandleInput() =>
            RunUpdateActions(inputActions);

        public sealed override void UpdateBeforeSimulation() =>
            Update();

        public sealed override void Simulate() =>
            Update();

        public sealed override void UpdateAfterSimulation() =>
            Update();

        /// <summary>
        /// The update function used (Before/Sim/After) is determined by the settings used by
        /// the MySessionComponentDescriptorAttribute applied to the child class.
        /// </summary>
        private void Update()
        {
            if (Instance == null) // move mod init somewhere else
            {
                Instance = this;
                closing = false;
                isServer = MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer;
                isDedicated = (MyAPIGateway.Utilities.IsDedicated && isServer);

                RunSafeAction(() => AfterInit());
            }

            RunUpdateActions(updateActions);
        }

        protected virtual void AfterInit() { }

        private void RunUpdateActions(List<Action> updateActions)
        {
            RunSafeAction(() =>
            {
                for (int n = 0; n < updateActions.Count; n++)
                    updateActions[n]();
            });
        }

        /// <summary>
        /// Executes a given <see cref="Action"/> in a try-catch block. If an exception occurs, it will attempt
        /// to log it, display an error message to the user, then unload the mod and its components.
        /// </summary>
        public static void RunSafeAction(Action action)
        {
            if (Instance != null && !Instance.crashed && (!Instance.isDedicated || RunOnServer))
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Instance.crashed = true;
                    TryWriteToLog(ModName + " has crashed!\n" + e.ToString());

                    MyAPIGateway.Utilities.ShowMissionScreen
                    (
                        ModName, "Debug", "",
                        $"{ModName} has encountered a problem and will attempt to reload. Press the X in the upper right hand corner " +
                        "to cancel.\n" + e.ToString(),
                        Instance.AllowReload, "Reload"
                    );

                    Close();
                }
            }
        }

        private void AllowReload(ResultEnum response)
        {
            if (response == ResultEnum.OK)
                crashed = false;
            else
                crashed = true;
        }

        public static bool TryWriteToLog(string message) =>
            Instance.modLog.TryWriteToLog(message);

        public static void WriteToLogStart(string message) =>
            Instance.modLog.WriteToLogStart(message);

        /// <summary>
        /// Sends chat message using the mod name as the sender.
        /// </summary>
        public static void SendChatMessage(string message)
        {
            if (Instance != null && !Instance.closing)
                MyAPIGateway.Utilities.ShowMessage(ModName, message);
        }

        /// <summary>
        /// Creates a message window using the mod name, a given subheading and a message.
        /// </summary>
        public static void ShowMessageScreen(string subHeading, string message)
        {
            if (Instance != null && !Instance.closing)
                MyAPIGateway.Utilities.ShowMissionScreen(ModName, subHeading, null, message, null, "Close");
        }

        protected override void UnloadData()
        {
            closing = true;
            Close();
        }

        protected virtual void BeforeClose() { }

        public static void Close()
        {
            RunSafeAction(() =>
            {
                foreach (Action CloseAction in closeActions)
                    CloseAction();

                Instance.BeforeClose();
                Instance = null;
            });
        }

        /// <summary>
        /// Generic base for mod components that need to hook into the game's internal loops; singleton.
        /// </summary>
        public class Component<T> : Singleton<T> where T : Component<T>, new()
        {
            static Component()
            {
                drawActions.Add(() => Instance?.Draw());
                inputActions.Add(() => Instance?.HandleInput());
                updateActions.Add(() => Instance?.Update());
                closeActions.Add(Close);
            }

            protected virtual void Draw() { }

            protected virtual void HandleInput() { }

            protected virtual void Update() { }
        }
    }
}
