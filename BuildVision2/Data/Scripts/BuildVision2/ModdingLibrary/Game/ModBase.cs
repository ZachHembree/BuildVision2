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
    public class ModMainAttribute : MySessionComponentDescriptor
    {
        public ModMainAttribute() : base(MyUpdateOrder.NoUpdate)
        { }
    }

    /// <summary>
    /// Entry point for any mod using this library.
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public sealed class ModBase : MySessionComponentBase 
    {
        public static LogIO Log { get { return LogIO.Instance; } }
        public static string ModName { get; set; }
        public static bool RunOnServer { get; set; }

        private static ModBase Instance { get; set; }
        private static readonly List<Action> initActions, closeActions, drawActions, updateActions;

        private bool crashed = false, isDedicated = false, isServer = false;

        static ModBase()
        {
            ModName = "Mod Base";
            RunOnServer = false;

            initActions = new List<Action>();
            closeActions = new List<Action>();
            drawActions = new List<Action>();
            updateActions = new List<Action>();
        }

        public override void Draw() =>
            RunUpdateActions(drawActions);

        public override void UpdateAfterSimulation()
        {
            if (Instance == null)
            {
                Instance = this;
                isServer = MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer;
                isDedicated = (MyAPIGateway.Utilities.IsDedicated && isServer);

                SendChatMessage($"Base Init. Init action count: {initActions.Count}");                
            }

            RunUpdateActions(initActions);
            RunUpdateActions(updateActions);
        }

        private void RunUpdateActions(List<Action> updateActions)
        {
            if (!crashed)
            {
                try
                {
                    for (int n = 0; n < updateActions.Count; n++)
                        updateActions[n]();
                }
                catch (Exception e)
                {
                    crashed = true;
                    Log?.TryWriteToLog($"{ModName} has crashed!\n" + e.ToString());

                    MyAPIGateway.Utilities.ShowMissionScreen
                    (
                        ModName,
                        "Debug", "",
                        $"{ModName} has crashed! Press the X in the upper right hand corner if you don't want " +
                        "it to reload.\n" + e.ToString(),
                        AllowReload,
                        "Reload"
                    );

                    UnloadData();
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
        /// Sends chat message using predetermined sender name.
        /// </summary>
        public static void SendChatMessage(string message)
        {
            if (Instance != null)
                MyAPIGateway.Utilities.ShowMessage(ModName, message);
        }

        /// <summary>
        /// Creates a message window using the mod name, a given subheading and a message.
        /// </summary>
        public static void ShowMessageScreen(string subHeading, string message)
        {
            if (Instance != null)
                MyAPIGateway.Utilities.ShowMissionScreen(ModName, subHeading, null, message, null, "Close");
        }

        protected override void UnloadData()
        {
            try
            {
                foreach (Action CloseAction in closeActions)
                    CloseAction();

                Instance = null;
            }
            catch
            {
                // you're kinda screwed at this point
            }
        }

        public static void Close()
        {
            if (Instance != null)
                Instance.UnloadData();
        }

        /// <summary>
        /// Generic base for mod components that need to hook into the game's internal loops; singleton.
        /// </summary>
        public class Component<T> : Singleton<T> where T : Component<T>, new()
        {
            protected static List<Action> DrawActions { get { return ModBase.drawActions; } }
            protected static List<Action> UpdateActions { get { return ModBase.updateActions; } }

            static Component()
            {
                initActions.Add(Init);
                closeActions.Add(Close);
            }
        }
    }
}
