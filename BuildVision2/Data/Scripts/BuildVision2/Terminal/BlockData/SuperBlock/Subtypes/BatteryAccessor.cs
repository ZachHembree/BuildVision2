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
                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_StoredPower)}", nameFormat);
                builder.Add($"{TerminalUtilities.GetPowerDisplay(Charge)}", valueFormat);
                builder.Add($" ({((Charge / Capacity) * 100f):F1}%)\n", nameFormat);

                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_MaxStoredPower)}", nameFormat);
                builder.Add($"{TerminalUtilities.GetPowerDisplay(Capacity)}\n", valueFormat);

                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_ChargeMode)}: ", nameFormat);
                builder.Add($"{GetLocalizedChargeMode()}\n", valueFormat);
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