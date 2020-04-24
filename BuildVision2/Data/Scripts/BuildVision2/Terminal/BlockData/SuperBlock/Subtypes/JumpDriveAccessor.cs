using Sandbox.ModAPI;
using VRage;
using MyJumpDriveStatus = Sandbox.ModAPI.Ingame.MyJumpDriveStatus;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to jump drive members, if defined.
        /// </summary>
        public JumpDriveAccessor JumpDrive { get; private set; }

        public class JumpDriveAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Returns the amount of power currently stored in the jump drive.
            /// </summary>
            public float Charge => jumpDrive.CurrentStoredPower;

            /// <summary>
            /// Returns the amount of power the jump drive can store.
            /// </summary>
            public float Capacity => jumpDrive.MaxStoredPower;

            /// <summary>
            /// Returns the jump drive's status (charging/jumping/ready).
            /// </summary>
            public MyJumpDriveStatus Status => jumpDrive.Status;

            private readonly IMyJumpDrive jumpDrive;

            public JumpDriveAccessor(SuperBlock block) : base(block, TBlockSubtypes.JumpDrive)
            {
                jumpDrive = block.TBlock as IMyJumpDrive;
            }

            /// <summary>
            /// Returns the jump drive's status as a localized string.
            /// </summary>
            public string GetLocalizedDriveStatus() =>
                MyTexts.TrySubstitute(Status.ToString());
        }
    }
}