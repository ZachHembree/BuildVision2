using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public BeaconAccessorBase Beacon { get; private set; }

        public class BeaconAccessorBase : SubtypeAccessor<IMyBeacon>
        {
            public float Radius { get { return subtype.Radius; } set { subtype.Radius = value; } }

            public string HudText { get { return subtype.HudText; } set { subtype.HudText = value; } }

            public BeaconAccessorBase(SuperBlock block) : base(block, TBlockSubtypes.Beacon)
            { }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertiesTitle_HudText)}: ", nameFormat);
                builder.Add($"{HudText}\n", valueFormat);

                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_BroadcastRadius)}: ", nameFormat);
                builder.Add($"{TerminalUtilities.GetDistanceDisplay(Radius)}\n", valueFormat);
            }
        }
    }
}