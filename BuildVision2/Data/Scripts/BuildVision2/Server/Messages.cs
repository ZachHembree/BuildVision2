using System;
using ProtoBuf;

namespace DarkHelmet.BuildVision2
{
    [Flags]
    public enum BvServerActions : ulong
    {
        None =                      0,
        MyMechanicalConnection =    1 << 0,
        MotorStator =               1 << 1  | MyMechanicalConnection,
        Warhead =                   1 << 2,
        AirVent =                   1 << 3,

        GetOrSendData =             1 << 26,
        RequireReply =              1 << 27  | GetOrSendData,
        AttachHead =                1 << 28  | GetOrSendData,
        DetachHead =                1 << 29  | GetOrSendData,
        GetAngle =                  1 << 30  | RequireReply,
        GetTime =                   1ul << 31  | RequireReply,
        GetOxygen =                 1ul << 32 | RequireReply,
        GetAlive =                  1ul << 33 | RequireReply,
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

            public ClientMessage(BvServerActions actionID, int callbackID, long entID)
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
