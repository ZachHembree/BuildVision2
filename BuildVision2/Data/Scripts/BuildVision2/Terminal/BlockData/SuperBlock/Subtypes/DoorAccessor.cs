using Sandbox.ModAPI;
using System;
using VRage;
using RichHudFramework.UI;
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
        public DoorAccessor Door
        {
            get
            {
                return _door;
            }
            private set
            {
                _door = value;
            }
        }

        private DoorAccessor _door;

        public class DoorAccessor : SubtypeAccessor<IMyDoor>
        {
            /// <summary>
            /// Returns the status of the door (open/opening/closed).
            /// </summary>
            public DoorStatus Status => subtype.Status;

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.Door);

                if (subtype != null && block.TBlock is IMyParachute)
                    block.SubtypeId |= TBlockSubtypes.Parachute;
            }

            /// <summary>
            /// Toggles the door opened/closed.
            /// </summary>
            public void ToggleDoor() =>
                subtype.ToggleDoor();

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

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add(MyTexts.GetString(MySpaceTexts.TerminalStatus), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(GetLocalizedStatus(), valueFormat);
                builder.Add("\n", valueFormat);
            }
        }
    }
}