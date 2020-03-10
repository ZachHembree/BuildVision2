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
        public PropertyData[] terminalProperties;

        public BlockData(string blockTypeID, PropertyData[] terminalProperties)
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

        [ProtoMember(2)]
        public int id;

        [ProtoMember(3)]
        public string valueData;

        public PropertyData(string propertyID, int ID, string valueData)
        {
            this.name = propertyID;
            this.id = ID;
            this.valueData = valueData;
        }
    }
}