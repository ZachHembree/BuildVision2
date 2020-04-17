using Sandbox.ModAPI;
using System;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public class PistonData
        {
            /// <summary>
            /// Returns the current position of the piston.
            /// </summary>
            public float ExtensionDist => piston.CurrentPosition;

            /// <summary>
            /// Reverses the piston's velocity.
            /// </summary>
            public readonly Action Reverse;

            private readonly IMyPistonBase piston;

            public PistonData(IMyTerminalBlock tBlock)
            {
                piston = tBlock as IMyPistonBase;
                Reverse = piston.Reverse;
            }
        }
    }
}