using Sandbox.ModAPI;
using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

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
            /// Returns the maximum ore detection range in meters.
            /// </summary>
            public float Range => oreDetector.Range * (oreDetector.IsLargeGrid() ? 1.5f : .5f);
            
            /// <summary>
            /// Determines whether or not the ore detector will broadcast ore locations via antenna.
            /// </summary>
            public bool BroadcastUsingAntennas { get { return oreDetector.BroadcastUsingAntennas; } set { oreDetector.BroadcastUsingAntennas = value; } }

            private readonly IMyOreDetector oreDetector;

            public OreDetectorAccessor(SuperBlock block) : base(block, TBlockSubtypes.OreDetector)
            {
                oreDetector = block.TBlock as IMyOreDetector;
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText 
                {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_OreDetectorRange)}: ", nameFormat },
                    { $"{TerminalUtilities.GetDistanceDisplay(Range)}\n", valueFormat },
                };
            }
        }
    }
}