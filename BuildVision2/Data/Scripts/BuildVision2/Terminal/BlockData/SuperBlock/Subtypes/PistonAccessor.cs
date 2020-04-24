using Sandbox.ModAPI;
using System;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to piston members, if defined.
        /// </summary>
        public PistonAccessor Piston { get; private set; }

        public class PistonAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Returns the current position of the piston.
            /// </summary>
            public float ExtensionDist => piston.CurrentPosition;

            private readonly IMyPistonBase piston;

            public PistonAccessor(SuperBlock block) : base(block, TBlockSubtypes.Piston)
            {
                piston = block.TBlock as IMyPistonBase;
            }

            /// <summary>
            /// Reverses the direction of the piston.
            /// </summary>
            public void Reverse() =>
                piston.Reverse();
        }
    }
}