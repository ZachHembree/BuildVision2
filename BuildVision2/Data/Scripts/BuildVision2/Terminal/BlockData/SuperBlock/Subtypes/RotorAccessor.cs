using Sandbox.ModAPI;
using System;
using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to rotor members, if defined.
        /// </summary>
        public RotorAccessor Rotor { get; private set; }

        public class RotorAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Rotor angle in pi radians.
            /// </summary>
            public float Angle => rotor.Angle;

            private readonly IMyMotorStator rotor;

            public RotorAccessor(SuperBlock block) : base(block, TBlockSubtypes.Rotor)
            {
                rotor = block.TBlock as IMyMotorStator;
            }

            /// <summary>
            /// Reverses the rotors direction of rotation
            /// </summary>
            public void Reverse() =>
                rotor.TargetVelocityRad = -rotor.TargetVelocityRad;

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                if (rotor.IsAttached)
                {
                    return new RichText {
                        { MyTexts.GetString(MySpaceTexts.BlockPropertiesText_MotorCurrentAngle), nameFormat },
                        { $"{Angle.RadiansToDegrees().Round(2)}\n", valueFormat },
                    };
                }
                else
                    return null;
            }
        }
    }
}