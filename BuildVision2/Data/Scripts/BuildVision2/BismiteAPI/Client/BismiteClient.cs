using DarkHelmet.Game;
using DarkHelmet.UI;
using Sandbox.ModAPI;
using System;
using VRage;
using VRage.Game.ModAPI;
using VRage.Game.Components;
using VRageMath;

namespace DarkHelmet.BismiteClient
{
    using ServerData = MyTuple<Action, Func<int, object>>;
    using ClientData = MyTuple<string, Action<int, object>, Action>;

    public abstract class BismiteClient : ModBase
    {
        private const long modID = 0112358132134, queueID = 011235813318532110;
        private static new BismiteClient Instance;
        public static bool Registered => Instance.registered;

        private bool regFail, registered;
        private ClientData regMessage;
        private Action UnregisterAction;
        private Func<int, object> GetApiDataFunc;

        public BismiteClient() : base(false, true)
        {
            regMessage = new ClientData(DebugName, MessageHandler, Close);
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
        {
            /*TextHudMessage test = new TextHudMessage(HudMain.Root)
            {
                Offset = new Vector2(0f, 200f),
                Text = "Old: AV AW Wa Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                Scale = .96f
            };

            RichTextElement textBoard = new RichTextElement(HudMain.Root) { };
            textBoard.Offset = new Vector2(0f, -200f);
            textBoard.Text.Append("New: AV AW Wa Lorem ipsum dolor sit amet, consectetur adipiscing elit.");

            TextButton
                button1 = new TextButton(HudMain.Root)
                { BgColor = new Color(0, 0, 0, 125), Padding = new Vector2(16f, 14f) };

            button1.Text.Append("I'm a client button!");
            button1.MouseInput.OnLeftClick += () => SendChatMessage("Client button clicked");

            Window
                window1 = new Window(HudMain.Root) { Width = 500f, Height = 300f, BgColor = new Color(0, 0, 0, 125), Origin = new Vector2(-500f, 0) };

            window1.Title.Append(new RichText() { { new GlyphFormat(Color.Black), "I'm a client window!" } });*/
        }

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