using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using System;
using System.Collections.Generic;
using DarkHelmet.IO;
using DarkHelmet.UI;

namespace DarkHelmet.Game
{
    /// <summary>
    /// Entry point for any mod using this library.
    /// </summary>
    public class ModBase : MySessionComponentBase // try sealing this
    {
        public static LogIO Log { get { return LogIO.Instance; } }
        public static string ModName { get; set; }
        public static bool RunOnServer { get; set; }

        private static ModBase Instance { get; set; }
        private static readonly List<Action> initActions, closeActions, 
            drawActions, updateBeforeSimActions, updateAfterSimActions;

        private bool crashed = false, isDedicated = false, isServer = false;

        static ModBase()
        {
            ModName = "Mod Base";
            RunOnServer = false;

            initActions = new List<Action>();
            closeActions = new List<Action>();
            drawActions = new List<Action>();
            updateBeforeSimActions = new List<Action>();
            updateAfterSimActions = new List<Action>();
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);

            if (Instance == null)
            {
                Instance = this;
                isServer = MyAPIGateway.Session.OnlineMode == MyOnlineModeEnum.OFFLINE || MyAPIGateway.Multiplayer.IsServer;
                isDedicated = (MyAPIGateway.Utilities.IsDedicated && isServer);

                HudUtilities.Init();

                foreach (Action initAction in initActions)
                    initAction();
            }
        }

        public override void Draw() =>
            RunLoopActions(drawActions);

        public override void UpdateBeforeSimulation() =>
            RunLoopActions(updateBeforeSimActions);

        public override void UpdateAfterSimulation() =>
            RunLoopActions(updateAfterSimActions);

        private void RunLoopActions(List<Action> updateActions)
        {
            if (!crashed && (!isDedicated || RunOnServer))
            {
                try
                {
                    foreach (Action action in updateActions)
                        action();
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
                foreach (Action closeAction in closeActions)
                    closeAction();

                Log.Close();
                Instance = null;
            }
            catch
            {
                // you're kinda screwed at this point
            }
        }

        /// <summary>
        /// Generic base for mod components that need to hook into the game's internal loops; singleton.
        /// </summary>
        public class Component<T> : Singleton<T> where T : Component<T>, new()
        {
            protected static List<Action> DrawActions { get { return ModBase.drawActions; } }
            protected static List<Action> UpdateBeforeSimActions { get { return ModBase.updateBeforeSimActions; } }
            protected static List<Action> UpdateAfterSimActions { get { return ModBase.updateAfterSimActions; } }

            static Component()
            {
                initActions.Add(Init);
                closeActions.Add(() => Instance?.Close());
            }
        }
    }
}
