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
using SecureMessageHandler = System.Action<ushort, byte[], ulong, bool>;
using SecureClientMessage = VRage.MyTuple<ushort, byte[], ulong, bool>;

namespace DarkHelmet.BuildVision2
{
    public enum EntitySubtypes : long
    {
        None = 0,
        MyMechanicalConnection = 1
    }

    public enum MechBlockActions
    {
        Attach = 1,
        Detach = 2
    }

    [ProtoContract]
    public struct BlockActionMsg
    {
        [ProtoMember(1)]
        public long entID;

        [ProtoMember(2)]
        public EntitySubtypes subtypeID;

        [ProtoMember(3)]
        public MechBlockActions actionID;

        public BlockActionMsg(long entID, EntitySubtypes subtypeID, MechBlockActions actionID)
        {
            this.entID = entID;
            this.subtypeID = subtypeID;
            this.actionID = actionID;
        }
    }

    /// <summary>
    /// Used to get/send information from/to the server, dedicated or not.
    /// </summary>
    public sealed class BvServer : BvComponentBase
    {
        private const ushort serverHandlerID = 16971;
        private readonly List<BlockActionMsg> clientOutgoing, clientParsed;
        private readonly List<SecureClientMessage> serverIncoming;

        private static BvServer instance;
        private static SecureMessageHandler messageHandler;

        private BvServer() : base(true, true)
        {
            clientOutgoing = new List<BlockActionMsg>();
            clientParsed = new List<BlockActionMsg>();
            serverIncoming = new List<SecureClientMessage>();
        }

        public static void Init()
        {
            if (instance == null)
            {
                instance = new BvServer();
            }

            if (ExceptionHandler.IsServer)
            {
                messageHandler = new SecureMessageHandler(instance.ServerMessageHandler);
                MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(serverHandlerID, messageHandler);
            }
        }

        /// <summary>
        /// Runs block action on the server as defined by the <see cref="BlockActionMsg"/>
        /// </summary>
        public static void SendBlockActionToServer(BlockActionMsg message)
        {
            instance.clientOutgoing.Add(message);
        }

        /// <summary>
        /// Release resources
        /// </summary>
        public override void Close()
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(serverHandlerID, messageHandler);
            instance = null;
        }

        public override void Update()
        {
            // Send messages in queue
            if (ExceptionHandler.IsClient)
            {
                SendServerMessages();
            }

            // Process messages from clients
            if (ExceptionHandler.IsServer)
            {
                ProcessServerMessages();
            }
        }

        /// <summary>
        /// Receives serialized data sent to the server
        /// </summary>
        private void ServerMessageHandler(ushort id, byte[] message, ulong plyID, bool sentFromServer)
        {
            serverIncoming.Add(new SecureClientMessage(id, message, plyID, sentFromServer));
        }

        /// <summary>
        /// Serializes client messages and sends them to the server.
        /// </summary>
        private void SendServerMessages()
        {
            if (clientOutgoing.Count > 0)
            {
                ExceptionHandler.WriteLineAndConsole($"Sending {clientOutgoing.Count} message(s) to server.");

                byte[] protoMessages;
                KnownException exception = Utils.ProtoBuf.TrySerialize(clientOutgoing, out protoMessages);

                if (exception == null)
                    MyAPIGateway.Multiplayer.SendMessageToServer(serverHandlerID, protoMessages);
                else
                    ExceptionHandler.WriteLineAndConsole($"Unable to serialize server message: {exception}");

                clientOutgoing.Clear();
            }
        }

        /// <summary>
        /// Processes serialized messages from clients.
        /// </summary>
        private void ProcessServerMessages()
        {
            int errCount = 0;

            // Deserialize client messages and keep a running count of errors
            for (int n = 0; n < serverIncoming.Count; n++)
            {
                List<BlockActionMsg> clientActions;
                KnownException exception = Utils.ProtoBuf.TryDeserialize(serverIncoming[n].Item2, out clientActions);

                if (exception != null)
                    errCount++;
                else
                    clientParsed.AddRange(clientActions);
            }

            // Process successfully parsed client messages
            if (errCount < clientParsed.Count)
            {
                ExceptionHandler.WriteLineAndConsole($"Recieved {clientParsed.Count} message(s) from client(s).");

                for (int n = 0; n < clientParsed.Count; n++)
                {
                    IMyEntity entity = MyAPIGateway.Entities.GetEntityById(clientParsed[n].entID);

                    switch (clientParsed[n].subtypeID)
                    {
                        case EntitySubtypes.None:
                            break;
                        case EntitySubtypes.MyMechanicalConnection:
                            {
                                var mechBlock = entity as IMyMechanicalConnectionBlock;

                                if (mechBlock != null)
                                {
                                    switch (clientParsed[n].actionID)
                                    {
                                        case MechBlockActions.Attach:
                                            mechBlock.Attach(); break;
                                        case MechBlockActions.Detach:
                                            mechBlock.Detach(); break;
                                    }
                                }

                                break;
                            }
                    }
                }
            }

            if (errCount > 0)
                ExceptionHandler.WriteLineAndConsole($"Unable to parse {errCount} of {serverIncoming.Count} client message(s).");

            serverIncoming.Clear();
            clientParsed.Clear();
        }
    }
}
