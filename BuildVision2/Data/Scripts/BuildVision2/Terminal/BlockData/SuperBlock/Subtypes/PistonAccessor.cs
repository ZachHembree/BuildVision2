using Sandbox.ModAPI;
using System;
using System.Text;
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
        public PistonAccessor Piston  { get { return _piston; } private set { _piston = value; } }

        private PistonAccessor _piston;

        public class PistonAccessor : SubtypeAccessor<IMyPistonBase>
        {
            /// <summary>
            /// Returns the current position of the piston.
            /// </summary>
            public float ExtensionDist => subtype.CurrentPosition;

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.Piston, TBlockSubtypes.MechanicalConnection);
            }

            /// <summary>
            /// Reverses the direction of the piston.
            /// </summary>
            public void Reverse() =>
                subtype.Reverse();

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                if (subtype.IsAttached)
                {
                    // Piston extension dist
                    builder.Add(MyTexts.GetString(MySpaceTexts.TerminalDistance), nameFormat);
                    builder.Add(": ", nameFormat);

                    block.textBuffer.Clear();
                    TerminalUtilities.GetDistanceDisplay(ExtensionDist, block.textBuffer);
                    block.textBuffer.Append('\n');

                    builder.Add(block.textBuffer, valueFormat);
                }
            }
        }
    }
}