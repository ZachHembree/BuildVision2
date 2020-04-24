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
        /// Provides access to radio antenna members, if defined.
        /// </summary>
        public RadioAntennaAccessor RadioAntenna { get; private set; }

        public class RadioAntennaAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Controls the broadcast range of the antenna.
            /// </summary>
            public float Range { get { return radioAntenna.Radius; } set { radioAntenna.Radius = value; } }

            /// <summary>
            /// Indicates whether or not the antenna is broadcasting.
            /// </summary>
            public bool IsBroadcasting => radioAntenna.IsBroadcasting;

            /// <summary>
            /// Controls the name broadcasted by the antenna.
            /// </summary>
            public string HudText { get { return radioAntenna.HudText; } set { radioAntenna.HudText = value; } } 

            private readonly IMyRadioAntenna radioAntenna;

            public RadioAntennaAccessor(SuperBlock block) : base(block, TBlockSubtypes.RadioAntenna)
            {
                radioAntenna = block.TBlock as IMyRadioAntenna;
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText 
                {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertiesTitle_HudText)}: ", nameFormat },
                    { $"{HudText}\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_BroadcastRadius)}: ", nameFormat },
                    { $"{Range.Round(2)}m\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.HudInfoBroadcasting)}: ", nameFormat },
                    { $"{(IsBroadcasting ? MyTexts.GetString(MySpaceTexts.Beacon_SafeZone_Info_Enabled) : MyTexts.GetString(MySpaceTexts.Beacon_SafeZone_Info_Disabled))}\n", valueFormat },
                };
            }
        }
    }
}