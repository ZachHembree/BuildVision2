using Sandbox.ModAPI;
using System;
using ConnectorStatus = Sandbox.ModAPI.Ingame.MyShipConnectorStatus;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public class ConnectorData
        {
            /// <summary>
            /// Returns the status of the connector (locked/ready/unlocked).
            /// </summary>
            public ConnectorStatus Status => connector.Status;

            /// <summary>
            /// Toggles the connector lock.
            /// </summary>
            public readonly Action ToggleConnect;

            private readonly IMyShipConnector connector;

            public ConnectorData(IMyTerminalBlock tBlock)
            {
                connector = tBlock as IMyShipConnector;
                ToggleConnect = connector.ToggleConnect;
            }
        }
    }
}