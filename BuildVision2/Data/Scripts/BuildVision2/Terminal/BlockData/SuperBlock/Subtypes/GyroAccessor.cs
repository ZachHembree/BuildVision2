using Sandbox.ModAPI;
using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public GyroAccessor Gyroscope { get; private set; }

        public class GyroAccessor : SubtypeAccessorBase
        {
            public float Power { get { return gyroscope.GyroPower; } set { gyroscope.GyroPower = value; } }

            public bool Override { get { return gyroscope.GyroOverride; } set { gyroscope.GyroOverride = value; } }

            public float Pitch { get { return gyroscope.Pitch; } set { gyroscope.Pitch = value; } }

            public float Roll { get { return gyroscope.Roll; } set { gyroscope.Roll = value; } }

            public float Yaw { get { return gyroscope.Yaw; } set { gyroscope.Yaw = value; } }

            private readonly IMyGyro gyroscope;

            public GyroAccessor(SuperBlock block) : base(block, TBlockSubtypes.Gyroscope)
            {
                gyroscope = block.TBlock as IMyGyro;
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                var summary = new RichText 
                {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GyroPower)}: ", nameFormat },
                    { $"{Power.Round(2)}\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GyroOverride)}: ", nameFormat },
                    { $"{(Override ? MyTexts.GetString(MySpaceTexts.HudInfoOn) : MyTexts.GetString(MySpaceTexts.HudInfoOff))}\n", valueFormat },
                };

                return summary;
            }
        }
    }
}