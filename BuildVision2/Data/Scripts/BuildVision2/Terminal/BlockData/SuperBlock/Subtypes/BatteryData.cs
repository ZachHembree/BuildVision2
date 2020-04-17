using Sandbox.ModAPI;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public class BatteryData
        {
            /// <summary>
            /// Returns the amount of power currently stored in the battery.
            /// </summary>
            public float PowerStored => battery.CurrentStoredPower;

            /// <summary>
            /// Returns the maximum capacity of the battery.
            /// </summary>
            public float Capacity => battery.MaxStoredPower;

            private readonly IMyBatteryBlock battery;

            public BatteryData(IMyTerminalBlock tBlock)
            {
                battery = tBlock as IMyBatteryBlock;
            }
        }
    }
}