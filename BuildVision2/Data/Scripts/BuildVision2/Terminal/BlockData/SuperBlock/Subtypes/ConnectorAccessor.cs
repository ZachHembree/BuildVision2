using Sandbox.ModAPI;
using System;
using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using ConnectorStatus = Sandbox.ModAPI.Ingame.MyShipConnectorStatus;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to block connector members, if defined.
        /// </summary>
        public ConnectorAccessor Connector
        {
            get
            {
                return _connector;
            }
            private set
            {
                _connector = value;
            }
        }

        private ConnectorAccessor _connector;

        public class ConnectorAccessor : SubtypeAccessor<IMyShipConnector>
        {
            /// <summary>
            /// Returns the status of the connector (locked/ready/unlocked).
            /// </summary>
            public ConnectorStatus Status => subtype.Status;

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.Connector);
            }

            /// <summary>
            /// Toggles connector lock.
            /// </summary>
            public void ToggleConnect() =>
                subtype.ToggleConnect();

            /// <summary>
            /// Returns localized string representing the connector's status.
            /// </summary>
            public string GetLocalizedStatus()
            {
                switch (Status)
                {
                    case ConnectorStatus.Unconnected:
                        return MyTexts.GetString(MySpaceTexts.BlockPropertyValue_Unlocked);
                    case ConnectorStatus.Connectable:
                        return MyTexts.GetString(MySpaceTexts.BlockPropertyValue_ReadyToLock);
                    case ConnectorStatus.Connected:
                        return MyTexts.GetString(MySpaceTexts.BlockPropertyValue_Locked);
                    default:
                        return null;
                }
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add(MyTexts.GetString(MySpaceTexts.TerminalStatus), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(GetLocalizedStatus(), valueFormat);
                builder.Add("\n", valueFormat);
            }
        }
    }
}