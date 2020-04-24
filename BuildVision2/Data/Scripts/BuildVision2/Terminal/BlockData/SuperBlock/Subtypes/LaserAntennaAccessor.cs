using Sandbox.ModAPI;
using VRage;
using MyLaserAntennaStatus = Sandbox.ModAPI.Ingame.MyLaserAntennaStatus;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to laser antenna members, if defined.
        /// </summary>
        public LaserAntennaAccessor LaserAntenna { get; private set; }

        public class LaserAntennaAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Contorls the maximum range of the antenna.
            /// </summary>
            public float Range { get { return laserAntenna.Range; } set { laserAntenna.Range = value; } }

            /// <summary>
            /// Indicates the antenna's current status.
            /// </summary>
            public MyLaserAntennaStatus Status => laserAntenna.Status;

            private readonly IMyLaserAntenna laserAntenna;

            public LaserAntennaAccessor(SuperBlock block) : base(block, TBlockSubtypes.LaserAntenna)
            {
                laserAntenna = block.TBlock as IMyLaserAntenna;
            }

            /// <summary>
            /// Returns antenna status as a localized string.
            /// </summary>
            public string GetLocalizedAntennaStatus() =>
                MyTexts.TrySubstitute(Status.ToString());
        }
    }
}