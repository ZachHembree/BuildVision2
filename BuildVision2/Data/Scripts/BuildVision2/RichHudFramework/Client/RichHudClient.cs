using RichHudFramework;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using Sandbox.ModAPI;
using System;
using VRage;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework.Client
{
    using ClientData = MyTuple<string, Action<int, object>, Action, int>;
    using ServerData = MyTuple<Action, Func<int, object>, int>;

    /// <summary>
    /// API Client for the Rich HUD Framework 
    /// </summary>
    public sealed class RichHudClient : RichHudComponentBase
    {
        private const long modID = 1965654081, queueID = 1314086443;
        private const int versionID = 7;

        public static bool Registered => Instance != null ? Instance.registered : false;
        private static RichHudClient Instance { get; set; }

        private readonly ClientData regMessage;
        private readonly Action InitAction, ReloadAction;

        private bool regFail, registered, inQueue;
        private Func<int, object> GetApiDataFunc;
        private Action UnregisterAction;

        private RichHudClient(string modName, Action InitCallback, Action ReloadCallback) : base(false, true)
        {
            InitAction = InitCallback;
            ReloadAction = ReloadCallback;

            ExceptionHandler.ModName = modName;

            if (LogIO.FileName == null || LogIO.FileName == "modLog.txt")
                LogIO.FileName = $"richHudLog.txt";

            regMessage = new ClientData(modName, MessageHandler, () => ExceptionHandler.Run(RemoteReload), versionID);
        }

        /// <summary>
        /// Initialzes and registers the client with the API if it is not already registered.
        /// </summary>
        /// <param name="modName">Name of the mod as it appears in the settings menu and in diagnostics</param>
        /// <param name="InitCallback">Invoked upon successfully registering with the API.</param>
        /// <param name="ReloadCallback">Invoked on client reload.</param>
        public static void Init(string modName, Action InitCallback, Action ReloadCallback)
        {
            if (Instance == null)
            {
                Instance = new RichHudClient(modName, InitCallback, ReloadCallback);
                Instance.RequestRegistration();

                if (!Registered && !Instance.regFail)
                {
                    Instance.EnterQueue();
                }
            }
        }

        /// <summary>
        /// Unregisters the client and resets all framework modules.
        /// </summary>
        public static void Reset()
        {
            if (Registered)
                ExceptionHandler.ReloadClients();
        }

        /// <summary>
        /// Attempts to register the client with the API
        /// </summary>
        private void RequestRegistration() =>
            MyAPIUtilities.Static.SendModMessage(modID, regMessage);

        /// <summary>
        /// Enters queue to await client registration.
        /// </summary>
        private void EnterQueue() =>
            MyAPIUtilities.Static.RegisterMessageHandler(queueID, QueueHandler);

        /// <summary>
        /// Unregisters callback for framework client queue.
        /// </summary>
        private void ExitQueue() =>
            MyAPIUtilities.Static.UnregisterMessageHandler(queueID, QueueHandler);

        /// <summary>
        /// Resend registration request on queue invocation.
        /// </summary>
        private void QueueHandler(object message)
        {
            if (!(registered || regFail))
            {
                inQueue = true;
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
                MsgTypes msgType = (MsgTypes)typeValue;

                if ((msgType == MsgTypes.RegistrationSuccessful) && message is ServerData)
                {
                    var data = (ServerData)message;
                    UnregisterAction = data.Item1;
                    GetApiDataFunc = data.Item2;

                    registered = true;

                    ExceptionHandler.Run(InitAction);
                }
                else if (msgType == MsgTypes.RegistrationFailed)
                {
                    if (message is string)
                        LogIO.WriteToLogStart($"Rich HUD API registration failed. Message: {message as string}");
                    else
                        LogIO.WriteToLogStart($"Rich HUD API registration failed.");

                    regFail = true;
                }
            }
        }

        public override void Update()
        {
            if (registered && inQueue)
            {
                ExitQueue();
                inQueue = false;
            }
        }

        public override void Close()
        {
            ExitQueue();
            Unregister();
            Instance = null;
        }

        private void RemoteReload()
        {
            if (registered)
            {
                ExceptionHandler.ReloadClients();
                ReloadAction();
            }
        }

        /// <summary>
        /// Unregisters client from API
        /// </summary>
        private void Unregister()
        {
            if (registered)
            {
                registered = false;
                UnregisterAction();
            }
        }

        /// <summary>
        /// Base class for types acting as modules for the API
        /// </summary>
        public abstract class ApiModule<T> : RichHudComponentBase
        {
            protected readonly ApiModuleTypes componentType;

            public ApiModule(ApiModuleTypes componentType, bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient)
            {
                if (!Registered)
                    throw new Exception("Types of ApiModule cannot be instantiated before RichHudClient is initialized.");

                this.componentType = componentType;
            }

            protected T GetApiData()
            {
                object data = Instance?.GetApiDataFunc((int)componentType);

                return (T)data;
            }
        }
    }
}