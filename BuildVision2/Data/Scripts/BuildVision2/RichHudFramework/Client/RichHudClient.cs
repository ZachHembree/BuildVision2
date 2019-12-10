using DarkHelmet.Game;
using DarkHelmet.UI;
using Sandbox.ModAPI;
using System;
using VRage;
using DarkHelmet.UI.Rendering;
using VRage.Game.ModAPI;
using VRage.Game.Components;
using VRageMath;

namespace DarkHelmet.RichHudClient
{
    using ServerData = MyTuple<Action, Func<int, object>>;
    using ClientData = MyTuple<string, Action<int, object>, Action>;

    public abstract class RichHudClient : ModBase
    {
        private const long modID = 0112358132134, queueID = 1314086443;
        private static new RichHudClient Instance;
        public static bool Registered => Instance.registered;

        private bool regFail, registered;
        private ClientData regMessage;
        private Action UnregisterAction;
        private Func<int, object> GetApiDataFunc;

        public RichHudClient() : base(false, true)
        {
            regMessage = new ClientData(ModName, MessageHandler, Close);
        }

        protected sealed override void AfterInit()
        {
            Instance = this;
            SendChatMessage($"Sending registration request...");
            MyAPIUtilities.Static.SendModMessage(modID, regMessage);
            SendChatMessage($"No response. Entering queue...");

            if (!Registered && !regFail)
                MyAPIUtilities.Static.RegisterMessageHandler(queueID, QueueHandler);
        }

        protected virtual void HudInit()
        { }

        private void QueueHandler(object message)
        {
            if (!(registered || regFail) && message is long)
            {
                long ID = (long)message;

                if (ID == modID)
                {
                    SendChatMessage($"Queue handler invoked. Resending registration request...");
                    //MyAPIUtilities.Static.UnregisterMessageHandler(queueID, QueueHandler);
                    MyAPIUtilities.Static.SendModMessage(modID, regMessage);
                }
            }
        }

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
                    HudInit();

                    SendChatMessage($"Client registered.");
                }
                else if (msgType == MsgTypes.RegistrationFailed)
                {
                    WriteToLogStart($"Registration Failed.");
                    SendChatMessage($"Registration failed.");
                    regFail = true;
                }
            }
        }

        private void Unregister()
        {
            if (registered)
            {
                registered = false;
                UnregisterAction();
            }
        }

        protected sealed override void BeforeClose()
        {
            MyAPIUtilities.Static.UnregisterMessageHandler(queueID, QueueHandler);
            Unregister();
            Instance = null;
        }

        protected virtual void HudClose()
        { }

        public abstract class ApiComponentBase : ComponentBase
        {
            protected readonly ApiComponentTypes componentType;

            public ApiComponentBase(ApiComponentTypes componentType, bool runOnServer, bool runOnClient) : base(runOnServer, runOnClient)
            {
                this.componentType = componentType;
            }

            protected object GetApiData() =>
                Instance.GetApiDataFunc((int)componentType);
        }
    }
}