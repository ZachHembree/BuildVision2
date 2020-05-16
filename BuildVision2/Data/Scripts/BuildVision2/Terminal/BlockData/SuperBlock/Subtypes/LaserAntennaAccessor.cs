using Sandbox.ModAPI;
using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using MyLaserAntennaStatus = Sandbox.ModAPI.Ingame.MyLaserAntennaStatus;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to laser antenna members, if defined.
        /// </summary>
        public LaserAntennaAccessor LaserAntenna { get; private set; }

        public class LaserAntennaAccessor : SubtypeAccessor<IMyLaserAntenna>
        {
            /// <summary>
            /// Contorls the maximum range of the antenna.
            /// </summary>
            public float Range { get { return subtype.Range; } set { subtype.Range = value; } }

            /// <summary>
            /// Indicates the antenna's current status.
            /// </summary>
            public MyLaserAntennaStatus Status => subtype.Status;

            public LaserAntennaAccessor(SuperBlock block) : base(block, TBlockSubtypes.LaserAntenna)
            { }

            /// <summary>
            /// Returns antenna status as a localized string.
            /// </summary>
            public string GetLocalizedAntennaStatus() =>
                MyTexts.TrySubstitute(Status.ToString());

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText 
                {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_LaserRange)}: ", nameFormat },
                    { $"{((Range < 1E8) ? TerminalUtilities.GetDistanceDisplay(Range) : MyTexts.GetString(MySpaceTexts.ScreenTerminal_Infinite))}\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", nameFormat },
                    { $"{GetLocalizedAntennaStatus()}\n", valueFormat }
                };
            }
        }
    }
}