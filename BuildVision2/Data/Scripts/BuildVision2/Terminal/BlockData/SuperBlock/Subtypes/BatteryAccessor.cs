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
        /// Provides access to block battery members, if defined.
        /// </summary>
        public BatteryAccessor Battery { get; private set; }

        public class BatteryAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Returns the amount of power currently stored in the battery.
            /// </summary>
            public float Charge => battery.CurrentStoredPower;

            /// <summary>
            /// Returns the maximum capacity of the battery.
            /// </summary>
            public float Capacity => battery.MaxStoredPower;

            private readonly IMyBatteryBlock battery;

            public BatteryAccessor(SuperBlock block) : base(block, TBlockSubtypes.Battery)
            {
                battery = block.TBlock as IMyBatteryBlock;
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_StoredPower)}", nameFormat },
                    { $"{TerminalUtilities.GetPowerDisplay(Charge)}", valueFormat },
                    { $" ({((Charge / Capacity) * 100f).Round(1)}%)\n", nameFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_MaxStoredPower)}", nameFormat },
                    { $"{TerminalUtilities.GetPowerDisplay(Capacity)}\n", valueFormat },
                };
            }
        }
    }
}