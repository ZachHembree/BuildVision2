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
        public JumpDriveAccessor JumpDrive  { get { return _jumpDrive; } private set { _jumpDrive = value; } }

        private JumpDriveAccessor _jumpDrive;

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

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.JumpDrive);
            }

            /// <summary>
            /// Returns the jump drive's status as a localized string.
            /// </summary>
            public string GetLocalizedDriveStatus() =>
                (Charge == Capacity && Status == MyJumpDriveStatus.Charging) ? MyTexts.TrySubstitute(MyJumpDriveStatus.Ready.ToString()) : MyTexts.TrySubstitute(Status.ToString());

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add($"{MyTexts.GetString(MySpaceTexts.TerminalStatus)}: ", nameFormat);
                builder.Add($"{GetLocalizedDriveStatus()}\n", valueFormat);

                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_StoredPower)}", nameFormat);
                builder.Add($"{TerminalUtilities.GetPowerDisplay(Charge)}h", valueFormat);
                builder.Add($" ({((Charge / Capacity) * 100f).Round(1)}%)\n", nameFormat);

                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_MaxStoredPower)}", nameFormat);
                builder.Add($"{TerminalUtilities.GetPowerDisplay(Capacity)}h\n", valueFormat);
            }
        }
    }
}