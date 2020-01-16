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

    /// <summary>
    /// Base class for mods making use of the Rich HUD Framework
    /// </summary>
    public abstract class RichHudClient : ModBase
    {
        private const long modID = 1965654081, queueID = 1314086443;
        private const int versionID = 1;

        public static bool Registered => Instance.registered;
        private static new RichHudClient Instance;

        private ClientData regMessage;
        private bool regFail, registered, inQueue;

        private Action UnregisterAction;
        private Func<int, object> GetApiDataFunc;

        public RichHudClient() : base(false, true)
        {
            regMessage = new ClientData(ModName, MessageHandler, () => RunSafeAction(Reload), versionID);
        }

        protected sealed override void AfterInit()
        {
            Instance = this;
            RequestRegistration();

            if (!Registered && !regFail)
                EnterQueue();
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
                    RunSafeAction(HudInit);
                }
                else if (msgType == MsgTypes.RegistrationFailed)
                {
                    if (message is string)
                        WriteToLogStart($"Rich HUD API registration failed. Message: {message as string}");
                    else
                        WriteToLogStart($"Rich HUD API registration failed.");

                    regFail = true;
                }
            }
        }

        /// <summary>
        /// Called immediately after the client registers with the API.
        /// </summary>
        protected virtual void HudInit()
        { }

        protected override void Update()
        {
            if (registered && inQueue)
            {
                ExitQueue();
                inQueue = false;
            }
        }

        protected sealed override void BeforeClose()
        {
            ExitQueue();
            HudClose();
            Unregister();
            Instance = null;
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
        /// Called immediately before mod close and before the client unregisters from the API.
        /// </summary>
        protected virtual void HudClose()
        { }

        /// <summary>
        /// Base class for types acting as modules for the API
        /// </summary>
        public abstract class ApiModule<T> : ComponentBase
        {
            protected readonly ApiModuleTypes componentType;

            public ApiModule(ApiModuleTypes componentType, bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient)
            {
                this.componentType = componentType;
            }

            protected T GetApiData() =>
                (T)Instance.GetApiDataFunc((int)componentType);
        }
    }
}