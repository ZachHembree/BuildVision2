using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public BeaconAccessorBase Beacon
        {
            get
            {
                return _beacon;
            }
            private set
            {
                _beacon = value;
            }
        }

        private BeaconAccessorBase _beacon;

        public class BeaconAccessorBase : SubtypeAccessor<IMyBeacon>
        {
            public float Radius { get { return subtype.Radius; } set { subtype.Radius = value; } }

            public string HudText { get { return subtype.HudText; } set { subtype.HudText = value; } }

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.Beacon);
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                var buf = block.textBuffer;

                // Broadcast text
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesTitle_HudText), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(HudText, valueFormat);
                builder.Add('\n', valueFormat);

                // Broadcast radius
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_BroadcastRadius), nameFormat);
                builder.Add(": ", nameFormat);

                buf.Clear();
                TerminalUtilities.GetDistanceDisplay(Radius, buf);
                buf.Append('\n');
                builder.Add(buf, valueFormat);
            }
        }
    }
}