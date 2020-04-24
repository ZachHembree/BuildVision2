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

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                if (piston.IsAttached)
                {
                    return new RichText {
                        { $"{MyTexts.GetString(MySpaceTexts.TerminalDistance)}: ", nameFormat },
                        { $"{ExtensionDist.Round(2)}m\n", valueFormat },
                    };
                }
                else
                    return null;
            }
        }
    }
}