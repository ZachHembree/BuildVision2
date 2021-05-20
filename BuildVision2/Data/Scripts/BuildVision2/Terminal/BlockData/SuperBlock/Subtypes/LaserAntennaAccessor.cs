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
        public LaserAntennaAccessor LaserAntenna  { get { return _laserAntenna; } private set { _laserAntenna = value; } }

        private LaserAntennaAccessor _laserAntenna;

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

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.LaserAntenna);
            }

            /// <summary>
            /// Returns antenna status as a localized string.
            /// </summary>
            public string GetLocalizedAntennaStatus() =>
                MyTexts.TrySubstitute(Status.ToString());

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                // Laser range
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_LaserRange), nameFormat);
                builder.Add(": ", nameFormat);

                var buf = block.textBuffer;
                buf.Clear();

                if (Range < 1E8)
                    TerminalUtilities.GetDistanceDisplay(Range, buf);
                else
                    buf.Append(MyTexts.GetString(MySpaceTexts.ScreenTerminal_Infinite));

                buf.Append('\n');
                builder.Add(buf, valueFormat);

                // Antenna status
                builder.Add(MyTexts.GetString(MySpaceTexts.TerminalStatus), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(GetLocalizedAntennaStatus(), valueFormat);
                builder.Add("\n", valueFormat);
            }
        }
    }
}