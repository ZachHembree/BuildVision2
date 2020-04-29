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
        public AirVentAccessor AirVent { get; private set; }

        public class AirVentAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Indicates the vent's current status (depressurized/depressurizing/pressurizing/pressurized).
            /// </summary>
            public VentStatus Status => vent.Status;

            public bool Depressurize => vent.Depressurize;

            public bool CanPressurize => vent.CanPressurize;

            public float OxygenLevel => vent.GetOxygenLevel();

            private readonly IMyAirVent vent;

            public AirVentAccessor(SuperBlock block) : base(block, TBlockSubtypes.AirVent)
            {
                vent = block.TBlock as IMyAirVent;
            }

            /// <summary>
            /// Returns vent status as a localized string.
            /// </summary>
            public string GetLocalizedVentStatus() =>
                MyTexts.TrySubstitute(Status.ToString());

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText
                {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_Depressurize)}: ", nameFormat },
                    { $"{MyTexts.GetString(Depressurize ? MySpaceTexts.SwitchText_On : MySpaceTexts.SwitchText_Off)}\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.HudInfoOxygen)}", nameFormat },
                    { $"{(OxygenLevel * 100f).Round(2)}%\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", nameFormat },
                    { $"{GetLocalizedVentStatus()}\n", valueFormat },
                };
            }
        }
    }
}