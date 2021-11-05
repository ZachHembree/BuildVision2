using RichHudFramework;
using RichHudFramework.UI;
using Sandbox.ModAPI;
using System.Text;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to radio antenna members, if defined.
        /// </summary>
        public RadioAntennaAccessor RadioAntenna  { get { return _radioAntenna; } private set { _radioAntenna = value; } }

        private RadioAntennaAccessor _radioAntenna;

        public class RadioAntennaAccessor : SubtypeAccessor<IMyRadioAntenna>
        {
            /// <summary>
            /// Controls the broadcast range of the antenna.
            /// </summary>
            public float Range { get { return subtype.Radius; } set { subtype.Radius = value; } }

            /// <summary>
            /// Indicates whether or not the antenna is broadcasting.
            /// </summary>
            public bool IsBroadcasting => subtype.IsBroadcasting;

            /// <summary>
            /// Controls the name broadcasted by the antenna.
            /// </summary>
            public string HudText { get { return subtype.HudText; } set { subtype.HudText = value; } }

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.RadioAntenna);
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                // Broadcast name
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesTitle_HudText), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(HudText, valueFormat);
                builder.Add("\n", valueFormat);

                // Broadcast Radius
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_BroadcastRadius), nameFormat);
                builder.Add(": ", nameFormat);

                block.textBuffer.Clear();
                TerminalUtilities.GetDistanceDisplay(Range, block.textBuffer);
                block.textBuffer.Append('\n');
                builder.Add(block.textBuffer, valueFormat);

                // Broadcasting enabled/disabled
                builder.Add(MyTexts.GetString(MySpaceTexts.HudInfoBroadcasting), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(IsBroadcasting ? MyTexts.GetString(MySpaceTexts.HudInfoOn) : MyTexts.GetString(MySpaceTexts.HudInfoOff), valueFormat);
                builder.Add("\n", valueFormat);
            }
        }
    }
}