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
        public List<PropertyData> propertyList;

        public BlockData(string blockTypeID, List<PropertyData> terminalProperties)
        {
            this.blockTypeID = blockTypeID;
            this.propertyList = terminalProperties;
        }
    }

    [ProtoContract]
    public struct PropertyData
    {
        [ProtoMember(1)]
        public string propName;

        [ProtoMember(2)]
        public byte[] valueData;

        [ProtoMember(3)]
        public bool enabled;

        [ProtoMember(4)]
        public BlockMemberValueTypes valueType;

        public PropertyData(string propertyID, byte[] valueData, bool enabled, BlockMemberValueTypes valueType)
        {
            this.propName = propertyID;
            this.valueData = valueData;
            this.enabled = enabled;
            this.valueType = valueType;
        }
    }
}