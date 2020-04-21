using Sandbox.ModAPI;
using System;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
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
                }

                return null;
            }
        }
    }
}