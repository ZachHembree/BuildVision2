using RichHudFramework;
using RichHudFramework.UI;
using Sandbox.ModAPI;
using System;
using VRage;
using MyJumpDriveStatus = Sandbox.ModAPI.Ingame.MyJumpDriveStatus;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

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
                var buf = block.textBuffer;

                // Jump status
                builder.Add(MyTexts.GetString(MySpaceTexts.TerminalStatus), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(GetLocalizedDriveStatus(), valueFormat);
                builder.Add("\n", valueFormat);

                // Stored power
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesText_StoredPower), nameFormat);

                buf.Clear();
                TerminalUtilities.GetPowerDisplay(Charge, buf);
                buf.Append('h');
                builder.Add(buf, valueFormat);

                // pct
                buf.Clear();
                buf.Append(" (");
                buf.Append(Math.Round(Charge / Capacity * 100f, 1));
                buf.Append("%)\n");
                builder.Add(buf, nameFormat);

                // Power capacity
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesText_MaxStoredPower), nameFormat);

                buf.Clear();
                TerminalUtilities.GetPowerDisplay(Capacity, buf);
                buf.Append("h\n");
                builder.Add(buf, valueFormat);
            }
        }
    }
}