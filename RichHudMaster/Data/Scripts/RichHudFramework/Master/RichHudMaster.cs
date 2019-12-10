using DarkHelmet.Game;
using DarkHelmet.UI;
using DarkHelmet.UI.FontData;
using DarkHelmet.UI.Rendering;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRageMath;

namespace DarkHelmet.RichHudMaster
{
    using UI.Server;
    using UI.Rendering.Server;
    using ClientData = MyTuple<string, Action<int, object>, Action>;
    using ServerData = MyTuple<Action, Func<int, object>>;

    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 1)]
    internal sealed class RichHudMaster : ModBase
    {
        private const long modID = 0112358132134, queueID = 1314086443; // replace this with the real mod ID when you're done
        private static new RichHudMaster Instance { get; set; }
        private readonly List<RichHudClient> clients;

        static RichHudMaster()
        {
            ModName = "Rich HUD Master";
            LogFileName = "RichHudMasterLog.txt";
        }

        public RichHudMaster() : base(false, true)
        {
            clients = new List<RichHudClient>();
        }

        protected override void AfterInit()
        {
            Instance = this;
            InitializeFonts();
            BindManager.Init();
            HudMain.Init();

            SendChatMessage($"Server Init");
            MyAPIUtilities.Static.RegisterMessageHandler(modID, MessageHandler);
            SendChatMessage($"Checking client queue...");
            MyAPIUtilities.Static.SendModMessage(queueID, modID);

            //Material wallpaperMat = new Material("HudLibTestTexture", new Vector2(1024f, 1024f));
            //TexturedBox wallpaper = new TexturedBox(HudMain.Root) { Material = wallpaperMat, Size = new Vector2(1024f, 1024f) };

            TextEditor textEditor = new TextEditor(false, HudMain.Root) { Width = 500f, Height = 300f, BodyColor = new Color(0, 0, 0, 125), Offset = new Vector2(500f, 0) };
            textEditor.Title.Append(new RichText() { { new GlyphFormat(Color.Black, alignment: TextAlignment.Center), "I'm a text editor without word wrapping!" } });

            TextEditor textEditor2 = new TextEditor(true, HudMain.Root) { Width = 500f, Height = 300f, BodyColor = new Color(0, 0, 0, 125), Offset = new Vector2(500f, 0) };
            textEditor2.Title.Append(new RichText() { { new GlyphFormat(Color.Black, alignment: TextAlignment.Center), "I'm a text editor with word wrapping!" } });

            SettingsMenu settingsMenu = new SettingsMenu(HudMain.Root);
        }

        private void InitializeFonts()
        {
            FontManager.TryAddFont(SeFont.fontData);
            FontManager.TryAddFont(SeFontShadowed.fontData);
            FontManager.TryAddFont(MonoFont.fontData);
            FontManager.TryAddFont(TimesNewRoman.fontData);

            foreach (IFontMin font in FontManager.Fonts)
                SendChatMessage($"Font {font.Index}: {font.Name}, Size: {font.PtSize}");
        }

        private void MessageHandler(object message)
        {
            if (message is ClientData)
            {
                var clientData = (ClientData)message;
                Utils.Debug.AssertNotNull(clientData.Item1);
                RichHudClient client = clients.Find(x => (x.debugName == clientData.Item1));
                SendChatMessage($"Recieved registration request from {clientData.Item1}");

                if (client == null)
                {
                    clients.Add(new RichHudClient(clientData, clients.Count));
                }
                else
                    client.SendData(MsgTypes.RegistrationFailed, "Client already registered.");
            }
        }

        protected override void BeforeClose()
        {
            MyAPIUtilities.Static.UnregisterMessageHandler(modID, MessageHandler);

            for (int n = clients.Count - 1; n >= 0; n--)
                clients[n].Unregister();

            Instance = null;
        }

        private class RichHudClient
        {
            public readonly string debugName;
            public readonly int index;

            private readonly IBindClient bindClient;
            private readonly Action<int, object> SendMsgAction;
            private readonly Action UnregisterAction;
            private bool registered;

            public RichHudClient(ClientData data, int index)
            {
                debugName = data.Item1;
                SendMsgAction = data.Item2;
                UnregisterAction = data.Item3;

                this.index = index;
                registered = true;

                bindClient = BindManager.GetNewBindClient();
                SendData(MsgTypes.RegistrationSuccessful, new ServerData(Unregister, GetApiData));
                SendChatMessage($"{debugName} registered successfully.");
            }

            public object GetApiData(int typeID)
            {
                ApiComponentTypes id = (ApiComponentTypes)typeID;

                if (id == ApiComponentTypes.BindManager)
                    return bindClient.GetApiData();
                else if (id == ApiComponentTypes.HudMain)
                    return HudMain.GetApiData();
                else if (id == ApiComponentTypes.FontManager)
                    return FontManager.GetApiData();
                else
                    return null;
            }

            public void SendData(MsgTypes msgType, object data) =>
                SendMsgAction((int)msgType, data);

            public void Unregister()
            {
                if (registered)
                {
                    registered = false;

                    Instance.clients.RemoveAt(index);
                    bindClient.Unload();
                    UnregisterAction();
                }
            }
        }
    }
}