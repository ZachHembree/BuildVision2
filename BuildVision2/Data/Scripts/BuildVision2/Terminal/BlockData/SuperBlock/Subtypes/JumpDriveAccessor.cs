using Sandbox.ModAPI;
using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using MyJumpDriveStatus = Sandbox.ModAPI.Ingame.MyJumpDriveStatus;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to jump drive members, if defined.
        /// </summary>
        public JumpDriveAccessor JumpDrive { get; private set; }

        public class JumpDriveAccessor : SubtypeAccessor<IMyJumpDrive>
        {
            /// <summary>
            /// Returns the amount of power currently stored in the jump drive.
            /// </summary>
            public float Charge => subtype.CurrentStoredPower;

            /// <summary>
            /// Returns the amount of power the jump drive can store.
            /// </summary>
            public float Capacity => subtype.MaxStoredPower;

            /// <summary>
            /// Returns the jump drive's status (charging/jumping/ready).
            /// </summary>
            public MyJumpDriveStatus Status => subtype.Status;

            public JumpDriveAccessor(SuperBlock block) : base(block, TBlockSubtypes.JumpDrive)
            { }

            /// <summary>
            /// Returns the jump drive's status as a localized string.
            /// </summary>
            public string GetLocalizedDriveStatus() =>
                (Charge == Capacity && Status == MyJumpDriveStatus.Charging) ? MyTexts.TrySubstitute(MyJumpDriveStatus.Ready.ToString()) : MyTexts.TrySubstitute(Status.ToString());

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText 
                {
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", nameFormat },
                    { $"{GetLocalizedDriveStatus()}\n", valueFormat },

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