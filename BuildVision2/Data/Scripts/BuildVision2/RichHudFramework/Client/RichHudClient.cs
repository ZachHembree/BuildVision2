using RichHudFramework.Game;
using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using Sandbox.ModAPI;
using System;
using VRage;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;

namespace RichHudFramework.Client
{
    using ClientData = MyTuple<string, Action<int, object>, Action, int>;
    using ServerData = MyTuple<Action, Func<int, object>, int>;

    public sealed class RichHudClient : ModBase.ComponentBase
    {
        private const long modID = 1965654081, queueID = 1314086443;
        private const int versionID = 2;

        public static bool Registered => Instance != null ? Instance.registered : false;
        private static RichHudClient Instance { get; set; }

        private ClientData regMessage;
        private bool regFail, registered, inQueue;
        private Action UnregisterAction;
        private Func<int, object> GetApiDataFunc;
        private readonly ModBase modInstance;
        private readonly Action InitCallbackAction;

        private RichHudClient(ModBase mod, Action InitCallback = null) : base(false, true)
        {
            InitCallbackAction = InitCallback;
            modInstance = mod;

            regMessage = new ClientData(ModBase.ModName, MessageHandler, () => ModBase.RunSafeAction(RemoteReload), versionID);
        }

        public static void Init(ModBase mod, Action InitCallback = null)
        {
            if (Instance == null)
            {
                Instance = new RichHudClient(mod, InitCallback);
                Instance.RequestRegistration();

                if (!Registered && !Instance.regFail)
                    Instance.EnterQueue();
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
        private void EnterQueue() =>
            MyAPIUtilities.Static.RegisterMessageHandler(queueID, QueueHandler);

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

                    if (InitCallbackAction != null)
                        ModBase.RunSafeAction(InitCallbackAction);
                }
                else if (msgType == MsgTypes.RegistrationFailed)
                {
                    if (message is string)
                        ModBase.WriteToLogStart($"Rich HUD API registration failed. Message: {message as string}");
                    else
                        ModBase.WriteToLogStart($"Rich HUD API registration failed.");

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
                Unregister();
                modInstance.Reload();
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
        public abstract class ApiModule<T> : ModBase.ComponentBase
        {
            protected readonly ApiModuleTypes componentType;

            public ApiModule(ApiModuleTypes componentType, bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient)
            {
                if (!Registered)
                    throw new Exception("Types of ApiModule cannot be instantiated before RichHudClient is registered.");

                this.componentType = componentType;
            }

            protected T GetApiData() =>
                (T)Instance.GetApiDataFunc((int)componentType);
        }
    }
}