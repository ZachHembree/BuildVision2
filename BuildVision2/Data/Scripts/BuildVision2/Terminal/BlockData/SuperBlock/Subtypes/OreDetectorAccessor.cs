using Sandbox.ModAPI;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to ore detector members, if defined.
        /// </summary>
        public OreDetectorAccessor OreDetector { get; private set; }

        public class OreDetectorAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Returns the maximum ore detection range.
            /// </summary>
            public float Range => oreDetector.Range;
            
            /// <summary>
            /// Determines whether or not the ore detector will broadcast ore locations via antenna.
            /// </summary>
            public bool BroadcastUsingAntennas { get { return oreDetector.BroadcastUsingAntennas; } set { oreDetector.BroadcastUsingAntennas = value; } }

            private readonly IMyOreDetector oreDetector;

            public OreDetectorAccessor(SuperBlock block) : base(block, TBlockSubtypes.OreDetector)
            {
                oreDetector = block.TBlock as IMyOreDetector;
            }
        }
    }
}