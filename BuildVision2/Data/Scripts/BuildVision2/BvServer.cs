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
        private readonly List<BlockActionMsg> serverMessages;
        private readonly List<byte[]> messageData;

        private static BvServer instance;
        private static Action<byte[]> messageHandler;

        private BvServer() : base(true, true)
        {
            serverMessages = new List<BlockActionMsg>();
            messageData = new List<byte[]>();
        }

        public static void Init()
        {
            if (instance == null)
            {
                instance = new BvServer();
            }

            if (!ExceptionHandler.IsClient)
            {
                messageHandler = new Action<byte[]>(instance.ServerMessageHandler);
                MyAPIGateway.Multiplayer.RegisterMessageHandler(serverHandlerID, messageHandler);
            }
        }

        /// <summary>
        /// Runs block action on the server as defined by the <see cref="BlockActionMsg"/>
        /// </summary>
        public static void SendBlockActionToServer(BlockActionMsg message)
        {
            instance.serverMessages.Add(message);
        }

        /// <summary>
        /// Release resources
        /// </summary>
        public override void Close()
        {
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(serverHandlerID, messageHandler);
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
                ReceiveServerMessages();
            }
        }

        /// <summary>
        /// Sends data to the server as a protobuf serialized byte array
        /// </summary>
        private void ServerMessageHandler(byte[] message)
        {
            messageData.Add(message);
        }

        private void SendServerMessages()
        {
            if (serverMessages.Count > 0)
            {
                ExceptionHandler.WriteLineAndConsole($"Sending {serverMessages.Count} message(s) to server.");

                byte[] protoMessages;
                KnownException exception = Utils.ProtoBuf.TrySerialize(serverMessages, out protoMessages);

                if (exception == null)
                    MyAPIGateway.Multiplayer.SendMessageToServer(serverHandlerID, protoMessages);
                else
                    ExceptionHandler.WriteLineAndConsole($"Unable to serialize server message: {exception.ToString()}");
            }

            if (!ExceptionHandler.IsServer)
                serverMessages.Clear();
        }

        /// <summary>
        /// Processes serialized messages from clients
        /// </summary>
        private void ReceiveServerMessages()
        {
            int errCount = 0;

            // Deserialize client messages and keep a running count of errors
            for (int n = 0; n < messageData.Count; n++)
            {
                var clientActions = new List<BlockActionMsg>();
                KnownException exception = Utils.ProtoBuf.TryDeserialize(messageData[n], out clientActions);

                if (exception != null)
                    errCount++;
                else
                {
                    serverMessages.AddRange(clientActions);
                }
            }

            // Process successfully parsed client messages
            if (errCount < serverMessages.Count)
            {
                ExceptionHandler.WriteLineAndConsole($"Recieved {serverMessages.Count} message(s) from client(s).");

                for (int n = 0; n < serverMessages.Count; n++)
                {
                    IMyEntity entity = MyAPIGateway.Entities.GetEntityById(serverMessages[n].entID);

                    switch ((EntitySubtypes)serverMessages[n].subtypeID)
                    {
                        case EntitySubtypes.None:
                            break;
                        case EntitySubtypes.MyMechanicalConnection:
                            {
                                var mechBlock = entity as IMyMechanicalConnectionBlock;

                                if (mechBlock != null)
                                {
                                    switch (serverMessages[n].actionID)
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
            {
                ExceptionHandler.WriteLineAndConsole($"Unable to parse {errCount} of {messageData.Count} client message(s).");
            }

            messageData.Clear();
            serverMessages.Clear();
        }
    }
}
