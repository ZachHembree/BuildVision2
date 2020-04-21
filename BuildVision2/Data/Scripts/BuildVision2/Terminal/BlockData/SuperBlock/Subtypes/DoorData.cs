using Sandbox.ModAPI;
using System;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using DoorStatus = Sandbox.ModAPI.Ingame.DoorStatus;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public class DoorData
        {
            /// <summary>
            /// Returns the status of the door (open/opening/closed).
            /// </summary>
            public DoorStatus Status => door.Status;

            /// <summary>
            /// Toggles the door open/closed.
            /// </summary>
            public readonly Action ToggleDoor;

            private readonly IMyDoor door;

            public DoorData(IMyTerminalBlock tBlock)
            {
                door = tBlock as IMyDoor;
                ToggleDoor = door.ToggleDoor;
            }

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