using Sandbox.ModAPI;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to block battery members, if defined.
        /// </summary>
        public BatteryAccessor Battery { get; private set; }

        public class BatteryAccessor : SubtypeAccessorBase
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

            public BatteryAccessor(SuperBlock block) : base(block, TBlockSubtypes.Battery)
            {
                battery = block.TBlock as IMyBatteryBlock;
            }
        }
    }
}