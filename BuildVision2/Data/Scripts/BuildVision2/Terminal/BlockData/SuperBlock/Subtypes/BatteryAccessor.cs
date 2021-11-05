using Sandbox.ModAPI;
using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using ChargeMode = Sandbox.ModAPI.Ingame.ChargeMode;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to block battery members, if defined.
        /// </summary>
        public BatteryAccessor Battery
        {
            get
            {
                return _battery;
            }
            private set
            {
                _battery = value;
            }
        }

        private BatteryAccessor _battery;

        public class BatteryAccessor : SubtypeAccessor<IMyBatteryBlock>
        {
            /// <summary>
            /// Returns the amount of power currently stored in the battery.
            /// </summary>
            public float Charge => subtype.CurrentStoredPower;

            /// <summary>
            /// Returns the maximum capacity of the battery.
            /// </summary>
            public float Capacity => subtype.MaxStoredPower;

            public ChargeMode ChargeMode { get { return subtype.ChargeMode; } set { subtype.ChargeMode = value; } }

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.Battery);
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                var buf = block.textBuffer;

                // Current charge
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesText_StoredPower), nameFormat);

                buf.Clear();
                TerminalUtilities.GetPowerDisplay(Charge, buf);
                buf.Append('h');
                builder.Add(buf, valueFormat);

                // pct
                buf.Clear();
                buf.Append(" (");
                buf.AppendFormat("{0:F1}", (Charge / Capacity) * 100f);
                buf.Append("%)\n");
                builder.Add(buf, nameFormat);

                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesText_MaxStoredPower), nameFormat);

                buf.Clear();
                TerminalUtilities.GetPowerDisplay(Capacity, buf);
                buf.Append("h\n");
                builder.Add(buf, valueFormat);

                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_ChargeMode), nameFormat);
                builder.Add(": ", nameFormat);

                buf.Clear();
                buf.Append(GetLocalizedChargeMode());
                buf.Append('\n');
                builder.Add(buf, valueFormat);
            }

            private string GetLocalizedChargeMode()
            {
                switch (ChargeMode)
                {
                    case ChargeMode.Auto:
                        return MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_Auto);
                    case ChargeMode.Recharge:
                        return MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_Recharge);
                    case ChargeMode.Discharge:
                        return MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_Discharge);
                }

                return "";
            }
        }
    }
}