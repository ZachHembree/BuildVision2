using Sandbox.ModAPI;
using System;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public class RotorData
        {
            /// <summary>
            /// Rotor angle in pi radians.
            /// </summary>
            public float Angle => rotor.Angle;

            /// <summary>
            /// Reverses the direction of the rotor.
            /// </summary>
            public readonly Action Reverse;

            private readonly IMyMotorStator rotor;

            public RotorData(IMyTerminalBlock tBlock)
            {
                rotor = tBlock as IMyMotorStator;
                Reverse = () => rotor.TargetVelocityRad = -rotor.TargetVelocityRad;
            }
        }
    }
}