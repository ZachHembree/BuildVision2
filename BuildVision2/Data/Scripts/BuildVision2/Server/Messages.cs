using System;
using ProtoBuf;

namespace DarkHelmet.BuildVision2
{
    [Flags]
    public enum ServerBlockActions : ulong
    {
        None =                      0x00000000,
        MyMechanicalConnection =    0x00000001,
        MotorStator =               0x00000002  | MyMechanicalConnection,
        Warhead =                   0x00000004,
        AirVent =                   0x00000008,

        GetOrSendData =             0x04000000,
        RequireReply =              0x08000000  | GetOrSendData,
        AttachHead =                0x10000000  | GetOrSendData,
        DetachHead =                0x20000000  | GetOrSendData,
        GetAngle =                  0x40000000  | RequireReply,
        GetTime =                   0x80000000  | RequireReply,
        GetOxygen =                 0x100000000 | RequireReply,
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
            public int callbackID;

            [ProtoMember(2)]
            public long entID;

            [ProtoMember(3)]
            public ulong actionID;

            public ClientMessage(ServerBlockActions actionID, int callbackID, long entID)
            {
                this.actionID = (ulong)actionID;
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
