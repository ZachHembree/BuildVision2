using Sandbox.ModAPI;
using System;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public class MechConnectionData
        {
            /// <summary>
            /// Indicates whether or not a head has been attached to the mech block.
            /// </summary>
            public bool PartAttached => mechBlock.IsAttached;

            /// <summary>
            /// Attempts to attach a nearby head to the block.
            /// </summary>
            public readonly Action AttachHead;

            /// <summary>
            /// Detaches the block head.
            /// </summary>
            public readonly Action DetachHead;

            private readonly IMyMechanicalConnectionBlock mechBlock;

            public MechConnectionData(IMyTerminalBlock tBlock)
            {
                mechBlock = tBlock as IMyMechanicalConnectionBlock;
                AttachHead = mechBlock.Attach;
                DetachHead = mechBlock.Detach;
            }

            /// <summary>
            /// Returns head attachment status as a localized string.
            /// </summary>
            public string GetLocalizedStatus()
            {
                if (PartAttached)
                    return MyTexts.GetString(MySpaceTexts.BlockPropertiesText_MotorAttached);
                else
                    return MyTexts.GetString(MySpaceTexts.BlockPropertiesText_MotorDetached);
            }
        }
    }
}