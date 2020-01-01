using System;
using VRage;

namespace RichHudFramework.Server
{
    using UI;
    using UI.Server;
    using UI.Rendering.Server;
    using ClientData = MyTuple<string, Action<int, object>, Action>;
    using ServerData = MyTuple<Action, Func<int, object>>;

    internal sealed partial class RichHudMaster
    {
        private class RichHudClient
        {
            public readonly string debugName;

            private readonly IBindClient bindClient;

            private readonly Action<int, object> SendMsgAction;
            private readonly Action UnregisterAction;
            private MyTuple<object, IHudElement> menuData;
            private bool registered;

            public RichHudClient(ClientData data)
            {
                debugName = data.Item1;
                SendMsgAction = data.Item2;
                UnregisterAction = data.Item3;

                bindClient = BindManager.GetNewBindClient();
                menuData = ModMenu.GetClientData(debugName);
                registered = true;

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
                else if (id == ApiComponentTypes.SettingsMenu)
                    return menuData.Item1;
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

                    Instance.clients.Remove(this);
                    bindClient.Unload();
                    menuData.Item2.Unregister();
                    UnregisterAction();
                }
            }
        }
    }
}