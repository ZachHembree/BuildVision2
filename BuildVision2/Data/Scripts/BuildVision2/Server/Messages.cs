using System;
using ProtoBuf;

namespace DarkHelmet.BuildVision2
{
    [Flags]
    public enum ServerBlockActions : ulong
    {
        None =                      0x0,
        MyMechanicalConnection =    0x1,
        MyMotorStator =             0x2         | MyMechanicalConnection,

        GetOrSendData =             0x4000000,
        RequireReply =              0x8000000   | GetOrSendData,
        AttachHead =                0x10000000  | GetOrSendData,
        DetachHead =                0x20000000  | GetOrSendData,
        GetAngle =                  0x40000000  | RequireReply
    }

    public sealed partial class BvServer
    {
        [ProtoContract]
        private struct SecureMessage
        {
            [ProtoMember(1)]
            public bool sentFromServer;

            [ProtoMember(2)]
            public ushort id;

            [ProtoMember(3)]
            public ulong plyID;

            [ProtoMember(4)]
            public byte[] message;

            public SecureMessage(bool sentFromServer, ushort id, ulong plyID, byte[] message)
            {
                this.sentFromServer = sentFromServer;
                this.id = id;
                this.plyID = plyID;
                this.message = message;
            }
        }

        /// <summary>
        /// Shared message type for servers and clients
        /// </summary>
        [ProtoContract]
        private struct MessageContainer
        {
            [ProtoMember(1)]
            public bool isFromServer;

            [ProtoMember(2)]
            public byte[] message;

            public MessageContainer(bool isFromServer, byte[] message)
            {
                this.isFromServer = isFromServer;
                this.message = message;
            }
        }

        /// <summary>
        /// Messages sent from clients to the server
        /// </summary>
        [ProtoContract]
        private struct ClientMessage
        {
            [ProtoMember(1)]
            public ServerBlockActions actionID;

            [ProtoMember(2)]
            public int callbackID;

            [ProtoMember(3)]
            public long entID;

            public ClientMessage(ServerBlockActions actionID, int callbackID, long entID)
            {
                this.actionID = actionID;
                this.callbackID = callbackID;
                this.entID = entID;
            }
        }

        /// <summary>
        /// Replies sent from servers to clients
        /// </summary>
        [ProtoContract]
        private struct ServerReplyMessage
        {
            [ProtoMember(1)]
            public int callbackID;

            [ProtoMember(2)]
            public byte[] data;

            public ServerReplyMessage(int callbackID, byte[] data)
            {
                this.callbackID = callbackID;
                this.data = data;
            }
        }
    }
}
