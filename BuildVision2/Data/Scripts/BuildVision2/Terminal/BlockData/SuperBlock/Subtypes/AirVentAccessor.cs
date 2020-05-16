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

        public class AirVentAccessor : SubtypeAccessor<IMyAirVent>
        {
            /// <summary>
            /// Indicates the vent's current status (depressurized/depressurizing/pressurizing/pressurized).
            /// </summary>
            public VentStatus Status => subtype.Status;

            public bool Depressurize => subtype.Depressurize;

            public bool CanPressurize => subtype.CanPressurize;

            public float OxygenLevel => subtype.GetOxygenLevel();

            public AirVentAccessor(SuperBlock block) : base(block, TBlockSubtypes.AirVent)
            { }

            /// <summary>
            /// Returns vent status as a localized string.
            /// </summary>
            public string GetLocalizedVentStatus() =>
                MyTexts.TrySubstitute(Status.ToString());

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                var summary = new RichText
                {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_Depressurize)}: ", nameFormat },
                    { $"{MyTexts.GetString(Depressurize ? MySpaceTexts.SwitchText_On : MySpaceTexts.SwitchText_Off)}\n", valueFormat },
                };

                if (block.Power.Enabled != null && block.Power.Enabled.Value)
                {
                    summary.Add(new RichText
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.HudInfoOxygen)}", nameFormat },
                        { $"{(OxygenLevel * 100f).Round(2)}%\n", valueFormat },
                    });
                }

                summary.Add(new RichText
                {
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", nameFormat },
                    { $"{GetLocalizedVentStatus()}\n", valueFormat },
                });

                return summary;
            }
        }
    }
}