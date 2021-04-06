﻿using Sandbox.ModAPI;
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
        public RotorAccessor Rotor  { get { return _rotor; } private set { _rotor = value; } }

        private RotorAccessor _rotor;

        public class RotorAccessor : SubtypeAccessor<IMyMotorStator>
        {
            /// <summary>
            /// Rotor angle in pi radians.
            /// </summary>
            // Bug: Mod API not synchronizing this with DS, yet piston extension is fine
            public float Angle => subtype.Angle;

            public bool RotorLock { get { return subtype.RotorLock; } set { subtype.RotorLock = value; } }

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.Rotor, TBlockSubtypes.MechanicalConnection);
            }

            /// <summary>
            /// Reverses the rotors direction of rotation
            /// </summary>
            public void Reverse() =>
                subtype.TargetVelocityRad = -subtype.TargetVelocityRad;

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                if (subtype.IsAttached)
                {
                    builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_MotorLock)}: ", nameFormat);
                    builder.Add($"{(RotorLock ? MyTexts.GetString(MySpaceTexts.HudInfoOn) : MyTexts.GetString(MySpaceTexts.HudInfoOff))}\n", valueFormat);

                    builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_MotorCurrentAngle)}", nameFormat);
                    builder.Add($"{Angle.RadiansToDegrees():F2}°\n", valueFormat);
                }
            }
        }
    }
}