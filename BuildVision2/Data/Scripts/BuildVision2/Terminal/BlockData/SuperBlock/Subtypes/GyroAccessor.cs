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

        public class GyroAccessor : SubtypeAccessor<IMyGyro>
        {
            public float Power { get { return subtype.GyroPower; } set { subtype.GyroPower = value; } }

            public bool Override { get { return subtype.GyroOverride; } set { subtype.GyroOverride = value; } }

            public float Pitch { get { return subtype.Pitch; } set { subtype.Pitch = value; } }

            public float Roll { get { return subtype.Roll; } set { subtype.Roll = value; } }

            public float Yaw { get { return subtype.Yaw; } set { subtype.Yaw = value; } }

            public GyroAccessor(SuperBlock block) : base(block, TBlockSubtypes.Gyroscope)
            { }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GyroPower)}: ", nameFormat);
                builder.Add($"{(Power * 100f).Round(2)}%\n", valueFormat);

                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_GyroOverride)}: ", nameFormat);
                builder.Add($"{(Override ? MyTexts.GetString(MySpaceTexts.HudInfoOn) : MyTexts.GetString(MySpaceTexts.HudInfoOff))}\n", valueFormat);
            }
        }
    }
}