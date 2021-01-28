using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using IMyAirVent = SpaceEngineers.Game.ModAPI.Ingame.IMyAirVent;
using VentStatus = SpaceEngineers.Game.ModAPI.Ingame.VentStatus;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to block vent members, if defined.
        /// </summary>
        public AirVentAccessor AirVent
        {
            get
            {
                return _airVent;
            }
            private set
            {
                _airVent = value;
            }
        }

        private AirVentAccessor _airVent;

        public class AirVentAccessor : SubtypeAccessor<IMyAirVent>
        {
            /// <summary>
            /// Indicates the vent's current status (depressurized/depressurizing/pressurizing/pressurized).
            /// </summary>
            public VentStatus Status => subtype.Status;

            public bool Depressurize => subtype.Depressurize;

            public bool CanPressurize => subtype.CanPressurize;

            public float OxygenLevel => subtype.GetOxygenLevel();

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.AirVent);
            }

            /// <summary>
            /// Returns vent status as a localized string.
            /// </summary>
            public string GetLocalizedVentStatus() =>
                MyTexts.TrySubstitute(Status.ToString());

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_Depressurize)}: ", nameFormat);
                builder.Add($"{MyTexts.GetString(Depressurize ? MySpaceTexts.SwitchText_On : MySpaceTexts.SwitchText_Off)}\n", valueFormat);

                if (block.Power.Enabled != null && block.Power.Enabled.Value)
                {
                    builder.Add($"{MyTexts.GetString(MySpaceTexts.HudInfoOxygen)}", nameFormat);
                    builder.Add($"{(OxygenLevel * 100f):F2}%\n", valueFormat);
                }

                builder.Add($"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", nameFormat);
                builder.Add($"{GetLocalizedVentStatus()}\n", valueFormat);
            }
        }
    }
}