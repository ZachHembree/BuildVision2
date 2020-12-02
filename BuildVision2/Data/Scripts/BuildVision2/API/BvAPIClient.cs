using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace DarkHelmet.BuildVision2
{
    using ClientData = MyTuple<string, Action<int, object>, Action, int>;
    using ServerData = MyTuple<Action, ApiMemberAccessor, int>;

    /// <summary>
    /// Registration states for Build Vision's API
    /// </summary>
    public enum BvApiStates : int
    {
        RegistrationRequest = 1,
        RegistrationSuccessful = 2,
        RegistrationFailed = 3
    }

    /// <summary>
    /// Build Vision menu modes
    /// </summary>
    public enum ScrollMenuModes : int
    {
        /// <summary>
        /// Menu currently set to peek
        /// </summary>
        Peek = 0,

        /// <summary>
        /// Menu currently controlling properties
        /// </summary>
        Control = 1,

        /// <summary>
        /// Menu currently copying/pasting properties
        /// </summary>
        Dupe = 2
    }

    /// <summary>
    /// Build Vision members accessible via the API
    /// </summary>
    public enum BvApiAccessors : int
    {
        /// <summary>
        /// out: bool
        /// </summary>
        Open = 1,

        /// <summary>
        /// out: int (ScrollMenuModes)
        /// </summary>
        MenuMode = 2,

        /// <summary>
        /// out: IMyTerminalBlock
        /// </summary>
        Target = 3,
    }

    /// <summary>
    /// Standalone client for querying client-side information from Build Vision
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, -1)]
    public sealed class BvApiClient : MySessionComponentBase
    {
        private const long modID = 1697184408, queueID = 1965658453;
        private const int versionID = 0;

        /// <summary>
        /// Returns true if Build Vision is currently open
        /// </summary>
        public static bool Open { get; private set; }

        /// <summary>
        /// Returns the menu's current mode (peek/control/copy)
        /// </summary>
        public static ScrollMenuModes MenuMode { get; private set; }

        /// <summary>
        /// Returns the targeted block. Null if there isn't one.
        /// </summary>
        public static IMyTerminalBlock TargetBlock { get; private set; }

        /// <summary>
        /// Returns true if the API client is registered
        /// </summary>
        public static bool Registered => instance != null ? instance.registered : false;

        private bool regFail, registered, inQueue;
        private ApiMemberAccessor GetOrSetMemberFunc;
        private Action UnregisterAction;
        private ClientData regMessage;
        private string modName;

        private static BvApiClient instance;

        /// <summary>
        /// Returns true if the player currently has the menu open
        /// </summary>
        public bool IsMenuOpen { get; }

        public BvApiClient()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
                throw new Exception("Only one instance of BvApiClient can exist at any given time.");
        }

        public static void Init(string modName)
        {
            if (!instance.registered && !MyAPIGateway.Utilities.IsDedicated)
            {
                instance.modName = modName;
                instance.regMessage = new ClientData(modName, instance.MessageHandler, () => Run(instance.Unregister), versionID);

                instance.RequestRegistration();

                if (!instance.registered && !instance.regFail)
                {
                    instance.EnterQueue();
                }
            }
        }

        /// <summary>
        /// Attempts to register the client with the API
        /// </summary>
        private void RequestRegistration() =>
            MyAPIUtilities.Static.SendModMessage(modID, regMessage);

        /// <summary>
        /// Enters queue to await client registration.
        /// </summary>
        private void EnterQueue()
        {
            MyAPIUtilities.Static.RegisterMessageHandler(queueID, QueueHandler);
            inQueue = true;
        }

        /// <summary>
        /// Unregisters callback for framework client queue.
        /// </summary>
        private void ExitQueue()
        {
            MyAPIUtilities.Static.UnregisterMessageHandler(queueID, QueueHandler);
            inQueue = false;
        }

        /// <summary>
        /// Resend registration request on queue invocation.
        /// </summary>
        private void QueueHandler(object message)
        {
            if (!(registered || regFail))
            {
                RequestRegistration();
            }
        }

        /// <summary>
        /// Handles registration response.
        /// </summary>
        private void MessageHandler(int typeValue, object message)
        {
            if (!Registered && !regFail)
            {
                BvApiStates msgType = (BvApiStates)typeValue;

                if ((msgType == BvApiStates.RegistrationSuccessful) && message is ServerData)
                {
                    var data = (ServerData)message;
                    UnregisterAction = data.Item1;
                    GetOrSetMemberFunc = data.Item2;

                    registered = true;
                }
                else if (msgType == BvApiStates.RegistrationFailed)
                {
                    if (message is string)
                        WriteToLog($"API registration failed. Message: {message as string}");
                    else
                        WriteToLog($"API registration failed.");

                    regFail = true;
                }
            }
        }

        public override void Draw()
        {
            if (registered && inQueue)
            {
                ExitQueue();
                inQueue = false;
            }

            if (registered)
            {
                Open = (bool)(GetOrSetMemberFunc(null, (int)BvApiAccessors.Open));
                MenuMode = (ScrollMenuModes)GetOrSetMemberFunc(null, (int)BvApiAccessors.MenuMode);
                TargetBlock = GetOrSetMemberFunc(null, (int)BvApiAccessors.Target) as IMyTerminalBlock;
            }
        }

        protected override void UnloadData()
        {
            Unregister();
            ExitQueue();
            instance = null;
        }

        /// <summary>
        /// Unregisters client from API
        /// </summary>
        private void Unregister()
        {
            if (registered)
            {
                registered = false;
                TargetBlock = null;
                Open = false;
                MenuMode = default(ScrollMenuModes);

                UnregisterAction();
                EnterQueue();
            }
        }

        /// <summary>
        /// Executes a given <see cref="Action"/> in a try-catch block. If an exception occurs, it will attempt
        /// to log it using MyLog.Default.WriteLineAndConsole(). Should not be used for anything that might run
        /// in a loop.
        /// </summary>
        private static void Run(Action Action)
        {
            try
            {
                Action();
            }
            catch (Exception e)
            {
                WriteToLog(e.ToString());
            }
        }

        private static void WriteToLog(string message)
        {
            MyLog.Default.WriteLineAndConsole($"[{instance.modName} (Build Vision)] {message}");
        }
    }
}