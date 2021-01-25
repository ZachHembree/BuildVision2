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

        public class OreDetectorAccessor : SubtypeAccessor<IMyOreDetector>
        {
            /// <summary>
            /// Returns the maximum ore detection range in meters.
            /// </summary>
            public float Range => subtype.Range * (subtype.IsLargeGrid() ? 1.5f : .5f);
            
            /// <summary>
            /// Determines whether or not the ore detector will broadcast ore locations via antenna.
            /// </summary>
            public bool BroadcastUsingAntennas { get { return subtype.BroadcastUsingAntennas; } set { subtype.BroadcastUsingAntennas = value; } }

            public OreDetectorAccessor(SuperBlock block) : base(block, TBlockSubtypes.OreDetector)
            { }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_OreDetectorRange)}: ", nameFormat);
                builder.Add($"{TerminalUtilities.GetDistanceDisplay(Range)}\n", valueFormat);
            }
        }
    }
}