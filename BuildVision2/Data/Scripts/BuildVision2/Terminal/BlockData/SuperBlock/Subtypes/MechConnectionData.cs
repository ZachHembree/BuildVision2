using Sandbox.ModAPI;
using System;

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
        }
    }
}