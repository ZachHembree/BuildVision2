using System;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System.Xml.Serialization;
using ProtoBuf;

namespace DarkHelmet.BuildVision2
{
    [ProtoContract]
    public struct BlockData
    {
        [ProtoMember(1)]
        public string blockTypeID;

        [ProtoMember(2)]
        public IList<PropertyData> terminalProperties;

        public BlockData(string blockTypeID, IList<PropertyData> terminalProperties)
        {
            this.blockTypeID = blockTypeID;
            this.terminalProperties = terminalProperties;
        }
    }

    [ProtoContract]
    public struct PropertyData
    {
        [ProtoMember(1)]
        public string name;

        [ProtoMember(3)]
        public byte[] valueData;

        public PropertyData(string propertyID, byte[] valueData)
        {
            this.name = propertyID;
            this.valueData = valueData;
        }
    }
}