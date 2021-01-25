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
        /// Provides access to piston members, if defined.
        /// </summary>
        public PistonAccessor Piston { get; private set; }

        public class PistonAccessor : SubtypeAccessor<IMyPistonBase>
        {
            /// <summary>
            /// Returns the current position of the piston.
            /// </summary>
            public float ExtensionDist => subtype.CurrentPosition;

            public PistonAccessor(SuperBlock block) : base(block, TBlockSubtypes.Piston, TBlockSubtypes.MechanicalConnection)
            { }

            /// <summary>
            /// Reverses the direction of the piston.
            /// </summary>
            public void Reverse() =>
                subtype.Reverse();

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                if (subtype.IsAttached)
                {
                    builder.Add($"{MyTexts.GetString(MySpaceTexts.TerminalDistance)}: ", nameFormat);
                    builder.Add($"{TerminalUtilities.GetDistanceDisplay(ExtensionDist)}\n", valueFormat);
                }
            }
        }
    }
}