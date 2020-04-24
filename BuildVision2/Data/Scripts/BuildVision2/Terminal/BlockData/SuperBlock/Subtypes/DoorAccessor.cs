using Sandbox.ModAPI;
using System;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using DoorStatus = Sandbox.ModAPI.Ingame.DoorStatus;
using IMyParachute = SpaceEngineers.Game.ModAPI.Ingame.IMyParachute;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to block door members, if defined.
        /// </summary>
        public DoorAccessor Door { get; private set; }

        public class DoorAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Returns the status of the door (open/opening/closed).
            /// </summary>
            public DoorStatus Status => door.Status;

            private readonly IMyDoor door;

            public DoorAccessor(SuperBlock block) : base(block, TBlockSubtypes.Door)
            {
                door = block.TBlock as IMyDoor;

                if (block.TBlock is IMyParachute)
                    block.SubtypeId |= TBlockSubtypes.Parachute;
            }

            /// <summary>
            /// Toggles the door opened/closed.
            /// </summary>
            public void ToggleDoor() =>
                door.ToggleDoor();

            /// <summary>
            /// Returns localized string representing the door's status.
            /// </summary>
            public string GetLocalizedStatus()
            {
                if (Status != DoorStatus.Open)
                    return MyTexts.GetString(MySpaceTexts.BlockAction_DoorClosed);
                else
                    return MyTexts.GetString(MySpaceTexts.BlockAction_DoorOpen);
            }
        }
    }
}