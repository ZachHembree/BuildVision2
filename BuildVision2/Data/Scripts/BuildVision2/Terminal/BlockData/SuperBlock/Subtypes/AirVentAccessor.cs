using VRage;
using System;
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

            public float OxygenLevel
            {
                get 
                {
                    BvServer.SendEntityActionToServer
                    (
                        ServerBlockActions.AirVent | ServerBlockActions.GetOxygen,
                        subtype.EntityId,
                        OxygenCallback
                    );

                    return _oxygenLevel;
                }
            }

            private float _oxygenLevel;
            private readonly Action<byte[]> OxygenCallback;

            public AirVentAccessor()
            {
                OxygenCallback = UpdateOxygenLevel;
            }

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.AirVent);
            }

            /// <summary>
            /// Returns vent status as a localized string.
            /// </summary>
            public string GetLocalizedVentStatus() =>
                (block.Power.Enabled != null && block.Power.Enabled.Value) ? MyTexts.TrySubstitute(Status.ToString()) : MyTexts.GetString(MySpaceTexts.SwitchText_Off);

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                if (block.Power.Enabled != null && block.Power.Enabled.Value)
                {
                    // Depressurize on/off
                    builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_Depressurize), nameFormat);
                    builder.Add(": ", nameFormat);

                    builder.Add(MyTexts.GetString(Depressurize ? MySpaceTexts.SwitchText_On : MySpaceTexts.SwitchText_Off), valueFormat);
                    builder.Add("\n", valueFormat);

                    // Oxy pct
                    builder.Add(MyTexts.GetString(MySpaceTexts.HudInfoOxygen), nameFormat);
                    builder.Add($"{(OxygenLevel * 100f):F2}%\n", valueFormat);
                }

                // Vent status
                builder.Add(MyTexts.GetString(MySpaceTexts.TerminalStatus), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(GetLocalizedVentStatus(), valueFormat);
                builder.Add("\n", valueFormat);
            }

            private void UpdateOxygenLevel(byte[] bin)
            {
                Utils.ProtoBuf.TryDeserialize(bin, out _oxygenLevel);
            }
        }
    }
}