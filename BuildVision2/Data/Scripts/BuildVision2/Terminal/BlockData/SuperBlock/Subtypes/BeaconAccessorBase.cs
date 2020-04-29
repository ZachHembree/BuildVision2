using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public BeaconAccessorBase Beacon { get; private set; }

        public class BeaconAccessorBase : SubtypeAccessorBase
        {
            public float Radius { get { return beacon.Radius; } set { beacon.Radius = value; } }

            public string HudText { get { return beacon.HudText; } set { beacon.HudText = value; } }

            private readonly IMyBeacon beacon;

            public BeaconAccessorBase(SuperBlock block) : base(block, TBlockSubtypes.Beacon)
            {
                beacon = block.TBlock as IMyBeacon;
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText 
                {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertiesTitle_HudText)}: ", nameFormat },
                    { $"{HudText}\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_BroadcastRadius)}: ", nameFormat },
                    { $"{TerminalExtensions.GetDistanceDisplay(Radius)}\n", valueFormat },
                };
            }
        }
    }
}