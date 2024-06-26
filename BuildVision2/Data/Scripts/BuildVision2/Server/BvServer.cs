﻿using RichHudFramework;
using RichHudFramework.Internal;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using SecureMsgHandler = System.Action<ushort, byte[], ulong, bool>;
using IMyAirVent = SpaceEngineers.Game.ModAPI.Ingame.IMyAirVent;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Used to get/send information from/to the server, dedicated or not.
    /// </summary>
    public sealed partial class BvServer : BvComponentBase
    {
        public static bool IsAlive { get; private set; }

#if PLUGIN_LOADER
        public const bool IsPlugin = true;
#else
        public const bool IsPlugin = false;
#endif

        private const ushort serverHandlerID = 16971;
        private static BvServer instance;

        private readonly List<SecureMessage> incomingMessages;
        private readonly List<MyTuple<ulong, ClientMessage>> receivedClientMessages;
        private readonly List<ServerReplyMessage> receivedServerMessages;

        private readonly List<MyTuple<ulong, List<ServerReplyMessage>>> serverOutgoing;
        private readonly ObjectPool<List<ServerReplyMessage>> replyListPool;

        private readonly List<ClientMessage> clientOutgoing;
        private readonly CallbackManager callbackManager;

        private SecureMsgHandler messageHandler;

        private BvServer() : base(true, true)
        {
            clientOutgoing = new List<ClientMessage>();

            incomingMessages = new List<SecureMessage>();
            receivedClientMessages = new List<MyTuple<ulong, ClientMessage>>();
            receivedServerMessages = new List<ServerReplyMessage>();

            serverOutgoing = new List<MyTuple<ulong, List<ServerReplyMessage>>>();
            replyListPool = new ObjectPool<List<ServerReplyMessage>>(GetNewReplyList, ResetReplyList);

            if (ExceptionHandler.IsClient)
                callbackManager = new CallbackManager();
            else
                callbackManager = null;
        }

        public static void Init()
        {
            if (instance == null)
            {
                instance = new BvServer();
            }

            instance.messageHandler = new SecureMsgHandler(instance.NetworkMessageHandler);
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(serverHandlerID, instance.messageHandler);

            if (ExceptionHandler.IsServer)
            {
                IsAlive = true;
            }
            else
            {
                IsAlive = false;
                SendEntityActionToServerInternal(BvServerActions.GetAlive, -1, x => IsAlive = true);
            }
        }

        /// <summary>
        /// Release resources
        /// </summary>
        public override void Close()
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(serverHandlerID, messageHandler);
            IsAlive = false;
            instance = null;
        }

        public static void SendEntityActionToServer(BvServerActions actionID, long entID, Action<byte[]> callback = null, bool uniqueCallback = true)
        {
            if (IsAlive)
            {
                SendEntityActionToServerInternal(actionID, entID, callback, uniqueCallback);
            }
        }

        /// <summary>
        /// Sends an entity action to be executed on the server
        /// </summary>
        private static void SendEntityActionToServerInternal(BvServerActions actionID, long entID, Action<byte[]> callback = null, bool uniqueCallback = true)
        {
            int callbackID = -1;

            if ((actionID & BvServerActions.RequireReply) == BvServerActions.RequireReply)
            {
                if (callback != null)
                {
                    callbackID = instance.callbackManager.RegisterCallback(callback, uniqueCallback);

                    if (callbackID == -1)
                        return;
                }
                else
                    throw new Exception($"Callback missing for {actionID}");
            }
            
            instance.clientOutgoing.Add(new ClientMessage(actionID, callbackID, entID));
        }

        /// <summary>
        /// Receives serialized data sent over the network
        /// </summary>
        private void NetworkMessageHandler(ushort id, byte[] message, ulong plyID, bool sentFromServer)
        {
            if (ExceptionHandler.IsServer || (ExceptionHandler.IsClient && sentFromServer))
            {
                incomingMessages.Add(new SecureMessage(sentFromServer, id, plyID, message));
            }
        }

        public override void Update()
        {
            if (ExceptionHandler.IsClient)
                SendMessagesToServer();

            if (ExceptionHandler.IsServer)
                SendMessagesToClient();

            ParseIncommingMessages();

            if (ExceptionHandler.IsServer)
                ProcessMessagesFromClient();

            if (ExceptionHandler.IsClient)
                ProcessMessagesFromServer();
        }

        /// <summary>
        /// Serializes client messages and sends them to the server
        /// </summary>
        private void SendMessagesToServer()
        {
            if (clientOutgoing.Count > 0)
            {
                ExceptionHandler.WriteToLogAndConsole($"Sending {clientOutgoing.Count} message(s) to server.", true);

                byte[] bin;
                KnownException exception = Utils.ProtoBuf.TrySerialize(clientOutgoing, out bin);

                if (exception == null)
                    exception = Utils.ProtoBuf.TrySerialize(new MessageContainer(false, bin), out bin);

                if (exception == null)
                    MyAPIGateway.Multiplayer.SendMessageToServer(serverHandlerID, bin);
                else
                    ExceptionHandler.WriteToLogAndConsole($"Unable to serialize server message: {exception}");

                clientOutgoing.Clear();
            }
        }

        /// <summary>
        /// Serializes server replies and sends them to the appropriate clients
        /// </summary>
        private void SendMessagesToClient()
        {
            if (serverOutgoing.Count > 0)
            {
                ExceptionHandler.WriteToLogAndConsole($"Sending {serverOutgoing.Count} message(s) to clients.", true);

                foreach (var clientMessages in serverOutgoing)
                {
                    byte[] bin;
                    KnownException exception = Utils.ProtoBuf.TrySerialize(clientMessages.Item2, out bin);

                    if (exception == null)
                        exception = Utils.ProtoBuf.TrySerialize(new MessageContainer(true, bin), out bin);

                    if (exception == null)
                        MyAPIGateway.Multiplayer.SendMessageTo(serverHandlerID, bin, clientMessages.Item1);
                    else
                        ExceptionHandler.WriteToLogAndConsole($"Unable to serialize client message: {exception}");
                }

                // Reuse reply lists
                foreach (var list in serverOutgoing)
                    replyListPool.Return(list.Item2);

                serverOutgoing.Clear();
            }
        }

        /// <summary>
        /// Parses messages recieved into separate client and server message lists
        /// </summary>
        private void ParseIncommingMessages()
        {
            int errCount = 0;
            receivedServerMessages.Clear();
            receivedClientMessages.Clear();

            // Deserialize client messages and keep a running count of errors
            for (int i = 0; i < incomingMessages.Count; i++)
            {
                MessageContainer container;
                KnownException exception = Utils.ProtoBuf.TryDeserialize(incomingMessages[i].message, out container);

                if (exception != null)
                    errCount++;
                else if (container.isFromServer)
                {
                    ServerReplyMessage[] serverReplies;
                    exception = Utils.ProtoBuf.TryDeserialize(container.message, out serverReplies);

                    receivedServerMessages.AddRange(serverReplies);
                }
                else
                {
                    ClientMessage[] clientMessages;
                    exception = Utils.ProtoBuf.TryDeserialize(container.message, out clientMessages);

                    receivedClientMessages.EnsureCapacity(receivedClientMessages.Count + clientMessages.Length);

                    for (int j = 0; j < clientMessages.Length; j++)
                        receivedClientMessages.Add(new MyTuple<ulong, ClientMessage>(incomingMessages[i].plyID, clientMessages[j]));
                }

                if (exception != null)
                    errCount++;
            }

            if (errCount > 0)
                ExceptionHandler.WriteToLogAndConsole($"Unable to parse {errCount} of {incomingMessages.Count} message(s).");

            incomingMessages.Clear();
        }

        /// <summary>
        /// Executes block actions specified in the parsed client message list
        /// </summary>
        private void ProcessMessagesFromClient()
        {
            receivedClientMessages.Sort((a, b) => 
            {
                if (a.Item2.entID > b.Item2.entID)
                    return 1;
                if (a.Item2.entID < b.Item2.entID)
                    return -1;
                else
                    return 0;
            });

            IMyEntity entity = null;
            long entID = 0;
            ulong? currentClient = null;

            foreach (var message in receivedClientMessages)
            {
                var actionID = (BvServerActions)message.Item2.actionID;

                if (message.Item2.entID != -1)
                {
                    if (entID != message.Item2.entID)
                        entity = MyAPIGateway.Entities.GetEntityById(message.Item2.entID);

                    entID = message.Item2.entID;

                    if ((actionID & BvServerActions.MyMechanicalConnection) == BvServerActions.MyMechanicalConnection)
                    {
                        HandleMechBlockMessages(entity, message, ref currentClient);
                    }
                    else if ((actionID & BvServerActions.Warhead) == BvServerActions.Warhead)
                    {
                        HandleWarheadMessages(entity, message, ref currentClient);
                    }
                    else if ((actionID & BvServerActions.AirVent) == BvServerActions.AirVent)
                    {
                        HandleAirVentMessages(entity, message, ref currentClient);
                    }
                }
                else if ((actionID & BvServerActions.GetAlive) == BvServerActions.GetAlive)
                {
                    AddServerReply(message, true, ref currentClient);
                }
            }
        }

        /// <summary>
        /// Processes replies from the server
        /// </summary>
        private void ProcessMessagesFromServer()
        {
            callbackManager.InvokeCallbacks(receivedServerMessages);
        }

        /// <summary>
        /// Handles client messages for mechanical connection blocks
        /// </summary>
        private void HandleMechBlockMessages(IMyEntity entity, MyTuple<ulong, ClientMessage> message, ref ulong? currentClient)
        {
            var mechBlock = entity as IMyMechanicalConnectionBlock;
            var actionID = (BvServerActions)message.Item2.actionID;

            if ((actionID & BvServerActions.AttachHead) == BvServerActions.AttachHead)
            {
                mechBlock.Attach();
            }
            else if ((actionID & BvServerActions.DetachHead) == BvServerActions.DetachHead)
            {
                mechBlock.Detach();
            }
            else if ((actionID & BvServerActions.MotorStator) == BvServerActions.MotorStator)
            {
                if ((actionID & BvServerActions.GetAngle) == BvServerActions.GetAngle)
                {
                    var rotor = entity as IMyMotorStator;
                    AddServerReply(message, rotor.Angle, ref currentClient);
                }
            }
        }

        private void HandleWarheadMessages(IMyEntity entity, MyTuple<ulong, ClientMessage> message, ref ulong? currentClient)
        {
            var actionID = (BvServerActions)message.Item2.actionID;

            if ((actionID & BvServerActions.GetTime) == BvServerActions.GetTime)
            {
                var warhead = entity as IMyWarhead;
                AddServerReply(message, warhead.DetonationTime, ref currentClient);
            }
        }

        private void HandleAirVentMessages(IMyEntity entity, MyTuple<ulong, ClientMessage> message, ref ulong? currentClient)
        {
            var actionID = (BvServerActions)message.Item2.actionID;

            if ((actionID & BvServerActions.GetOxygen) == BvServerActions.GetOxygen)
            {
                var vent = entity as IMyAirVent;
                AddServerReply(message, vent.GetOxygenLevel(), ref currentClient);
            }
        }

        /// <summary>
        /// Adds server reply for a given client message
        /// </summary>
        private void AddServerReply<T>(MyTuple<ulong, ClientMessage> message, T dataIn, ref ulong? currentClient)
        {
            ulong clientID = message.Item1;
            ClientMessage clientMessage = message.Item2;
            var actionID = clientMessage.actionID;

            byte[] bin;
            KnownException exception = Utils.ProtoBuf.TrySerialize(dataIn, out bin);

            if (exception == null)
            {
                if (currentClient != clientID || currentClient == null)
                {
                    currentClient = clientID;
                    serverOutgoing.Add(new MyTuple<ulong, List<ServerReplyMessage>>(clientID, replyListPool.Get()));
                }

                var list = serverOutgoing[serverOutgoing.Count - 1].Item2;
                list.Add(new ServerReplyMessage(clientMessage.callbackID, bin));
            }
            else
                ExceptionHandler.WriteToLogAndConsole($"Failed to serialize client reply: {exception}");
        }

        private static List<ServerReplyMessage> GetNewReplyList()
        {
            return new List<ServerReplyMessage>();
        }

        private static void ResetReplyList(List<ServerReplyMessage> list)
        {
            list.Clear();
        }
    }
}
