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
        public ConnectorAccessor Connector { get; private set; }

        public class ConnectorAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Returns the status of the connector (locked/ready/unlocked).
            /// </summary>
            public ConnectorStatus Status => connector.Status;

            private readonly IMyShipConnector connector;

            public ConnectorAccessor(SuperBlock block) : base(block, TBlockSubtypes.Connector)
            {
                connector = block.TBlock as IMyShipConnector;
            }

            /// <summary>
            /// Toggles connector lock.
            /// </summary>
            public void ToggleConnect() =>
                connector.ToggleConnect();

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

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText {
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", nameFormat },
                    { $"{GetLocalizedStatus()}\n", valueFormat },
                };
            }
        }
    }
}