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

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText 
                {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertiesTitle_HudText)}: ", nameFormat },
                    { $"{HudText}\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_BroadcastRadius)}: ", nameFormat },
                    { $"{TerminalUtilities.GetDistanceDisplay(Radius)}\n", valueFormat },
                };
            }
        }
    }
}