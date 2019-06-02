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
        public static LogIO Log { get { return LogIO.Instance; } }
        public static string ModName { get; protected set; }
        public static bool RunOnServer { get; protected set; }
        protected static ModBase Instance { get; private set; }
        protected static List<Action> DrawActions { get { return drawActions; } }
        protected static List<Action> InputActions { get { return inputActions; } }
        protected static List<Action> UpdateActions { get { return updateActions; } }

        private static readonly List<Action> initActions, closeActions, drawActions, inputActions, updateActions;

        private bool crashed, isDedicated, isServer, closing;

        static ModBase()
        {
            ModName = "Mod Base";
            RunOnServer = false;

            initActions = new List<Action>();
            closeActions = new List<Action>();
            drawActions = new List<Action>();
            inputActions = new List<Action>();
            updateActions = new List<Action>();
        }

        public ModBase()
        {
            if (Instance != null)
                throw new Exception("Only one instance of type ModBase can exist at a given time.");

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
        /// The update function used Before/Sim/After is determined by the settings used by
        /// the MySessionComponentDescriptorAttribute applied to the child class.
        /// </summary>
        private void Update()
        {
            if (Instance == null)
            {
                Instance = this;
                closing = false;
                isServer = MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer;
                isDedicated = (MyAPIGateway.Utilities.IsDedicated && isServer);

                if (!isDedicated || RunOnServer) AfterInit();
            }

            RunUpdateActions(initActions);
            RunUpdateActions(updateActions);
        }

        protected virtual void AfterInit() { }

        private void RunUpdateActions(List<Action> updateActions)
        {
            if (!crashed && (!isDedicated || RunOnServer))
            {
                RunSafeAction(() =>
                {
                    for (int n = 0; n < updateActions.Count; n++)
                        updateActions[n]();
                });
            }
        }

        /// <summary>
        /// Executes a given <see cref="Action"/> in a try-catch block. If an exception occurs, it will attempt
        /// to log it, display an error message to the user, and unload the mod and its components.
        /// </summary>
        protected void RunSafeAction(Action action)
        {
            if (!crashed && (!isDedicated || RunOnServer))
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    crashed = true;
                    Log?.TryWriteToLog($"{ModName} has crashed!\n" + e.ToString());

                    MyAPIGateway.Utilities.ShowMissionScreen
                    (
                        ModName, "Debug", "",
                        $"{ModName} has crashed! Press the X in the upper right hand corner if you don't want " +
                        "it to reload.\n" + e.ToString(),
                        AllowReload, "Reload"
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
            Instance?.RunSafeAction(() =>
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
            protected static List<Action> DrawActions { get { return ModBase.drawActions; } }
            protected static List<Action> InputActions { get { return ModBase.inputActions; } }
            protected static List<Action> UpdateActions { get { return ModBase.updateActions; } }

            static Component()
            {
                initActions.Add(Init);
                closeActions.Add(Close);
            }
        }
    }

    public abstract class ConfigurableMod : ModBase
    {

    }

    public class ConfigurableMod<ConfigT> : ConfigurableMod where ConfigT : ConfigRoot<ConfigT>, new()
    {
        public static ConfigT Cfg { get; set; }

        private static ConfigIO<ConfigT> ConfigIO { get { return ConfigIO<ConfigT>.Instance; } }
        protected bool initStarted, initFinished;

        public ConfigurableMod()
        {
            initStarted = false;
            initFinished = false;
        }

        protected sealed override void AfterInit()
        {
            initStarted = true;
            initFinished = false;
            ConfigIO.LoadStart(BeforeInitFinish, true);
        }

        private void BeforeInitFinish(ConfigT cfg)
        {
            if (!initFinished && initStarted)
            {
                initFinished = true;
                Cfg = cfg;
                InitFinish();
            }
        }

        protected virtual void InitFinish() { }

        protected sealed override void BeforeClose()
        {
            if (initFinished)
            {
                ConfigIO?.Save(Cfg);
                BeforeUnload();
            }
        }

        protected virtual void BeforeUnload() { }

        /// <summary>
        /// Loads config from file and applies it. Runs in parallel.
        /// </summary>
        public void LoadConfig(bool silent = false)
        {
            if (Instance != null)
                ConfigIO.LoadStart((ConfigT value) => Cfg = value, silent);
        }

        /// <summary>
        /// Gets the current configuration and writes it to the config file. 
        /// Runs in parallel.
        /// </summary>
        public void SaveConfig(bool silent = false)
        {
            if (Instance != null)
                ConfigIO.SaveStart(Cfg, silent);
        }

        /// <summary>
        /// Resets the current configuration to the default settings and saves them.
        /// </summary>
        public void ResetConfig(bool silent = false)
        {
            if (Instance != null)
                ConfigIO.SaveStart(ConfigRoot<ConfigT>.Defaults, silent);

            Cfg = ConfigRoot<ConfigT>.Defaults;
        }
    }
}
