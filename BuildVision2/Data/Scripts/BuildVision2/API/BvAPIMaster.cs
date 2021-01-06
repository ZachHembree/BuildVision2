using RichHudFramework.Client;
using RichHudFramework.Internal;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using Sandbox.ModAPI;
using VRage.Game.Components;
using RichHudFramework;
using ProtoBuf;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace DarkHelmet.BuildVision2
{
    using ClientData = MyTuple<string, Action<int, object>, Action, int>;
    using ServerData = MyTuple<Action, ApiMemberAccessor, int>;

    public class BvApiMaster : BvComponentBase
    {
        private const long modID = 1697184408, queueID = 1965658453;
        private const int versionID = 0;

        private readonly List<Client> clients;
        private static BvApiMaster instance;

        private BvApiMaster() : base(false, true)
        {
            clients = new List<Client>();
        }

        public static void Init()
        {
            if (instance == null)
            {
                instance = new BvApiMaster();
                instance.RegisterClientHandler();
                instance.CheckClientQueue();
            }
        }

        /// <summary>
        /// Registers the callback method for client registration.
        /// </summary>
        private void RegisterClientHandler() =>
            MyAPIUtilities.Static.RegisterMessageHandler(modID, ClientHandler);

        /// <summary>
        /// Unegisters the callback method for client registration.
        /// </summary>
        private void UnregisterClientHandler() =>
            MyAPIUtilities.Static.UnregisterMessageHandler(modID, ClientHandler);

        /// <summary>
        /// Queries the client queue for any clients awaiting registration.
        /// </summary>
        private void CheckClientQueue() =>
            MyAPIUtilities.Static.SendModMessage(queueID, modID);

        /// <summary>
        /// Processes registration requests from API clients.
        /// </summary>
        private void ClientHandler(object message)
        {
            if (message is ClientData)
            {
                var clientData = (ClientData)message;
                Client client = clients.Find(x => (x.debugName == clientData.Item1));

                if (client == null && clientData.Item4 == versionID)
                {
                    clients.Add(new Client(clientData));
                }
                else
                {
                    Action<int, object> SendMsgAction = clientData.Item2;

                    if (clientData.Item4 != versionID)
                    {
                        string error = $" [BV2] Error: Client version for {clientData.Item1} does not match. API vID: {versionID}, Client vID: {clientData.Item4}";

                        SendMsgAction((int)BvApiStates.RegistrationFailed, error);
                        ExceptionHandler.WriteToLogAndConsole(error);
                    }
                    else
                        SendMsgAction((int)BvApiStates.RegistrationFailed, "Client already registered.");
                }
            }
        }

        private static object GetOrSetMembers(object data, int memberEnum)
        {
            bool running = BvMain.Instance?.CanUpdate ?? false;

            switch ((BvApiAccessors)memberEnum)
            {
                case BvApiAccessors.Open:
                    return running ? PropertiesMenu.Open : false;
                case BvApiAccessors.MenuMode:
                    return running ? PropertiesMenu.MenuMode : default(ScrollMenuModes);
                case BvApiAccessors.Target:
                    return running ? PropertiesMenu.Target?.TBlock : null;
            }

            return null;
        }

        public override void Close()
        {
            UnregisterClientHandler();

            if (ExceptionHandler.Reloading)
            {
                for (int n = clients.Count - 1; n >= 0; n--)
                    clients[n].Unregister();

                clients.Clear();
            }
            else if (ExceptionHandler.Unloading)
            {
                instance = null;
            }
        }

        private class Client
        {
            public readonly string debugName;
            public readonly int versionID;

            private readonly Action<int, object> SendMsgAction;
            private readonly Action ReloadAction;
            private bool registered;

            public Client(ClientData data)
            {
                debugName = data.Item1;
                SendMsgAction = data.Item2;
                ReloadAction = data.Item3;
                versionID = data.Item4;

                registered = true;

                ExceptionHandler.WriteToLogAndConsole($"{debugName} successfully registered with the API.");
                SendData(BvApiStates.RegistrationSuccessful, new ServerData(Unregister, GetOrSetMembers, versionID));
            }

            /// <summary>
            /// Sends a message to the client.
            /// </summary>
            public void SendData(BvApiStates msgType, object data) =>
                SendMsgAction((int)msgType, data);

            public void Unregister()
            {
                if (registered && !ExceptionHandler.Unloading)
                {
                    ExceptionHandler.Run(() => 
                    {
                        registered = false;
                        instance.clients.Remove(this);
                        ReloadAction();
                    });
                }
            }
        }
    }
}