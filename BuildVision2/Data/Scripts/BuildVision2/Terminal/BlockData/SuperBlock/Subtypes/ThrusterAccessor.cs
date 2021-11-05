using RichHudFramework;
using RichHudFramework.UI;
using Sandbox.ModAPI;
using System;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to block thruster members.
        /// </summary>
        public ThrusterAccessor Thruster  { get { return _thruster; } private set { _thruster = value; } }

        private ThrusterAccessor _thruster;

        public class ThrusterAccessor : SubtypeAccessor<IMyThrust>
        {
            /// <summary>
            /// Controls the current thrust override.
            /// </summary>
            public float Override { get { return subtype.ThrustOverride; } set { subtype.ThrustOverride = value; } }

            /// <summary>
            /// Indicates the current thrust output.
            /// </summary>
            public float CurrentThrust => subtype.CurrentThrust;

            public float CurrentThrustPct => subtype.CurrentThrust / subtype.MaxThrust;

            /// <summary>
            /// Indicates the maximum effective thrust.
            /// </summary>
            public float MaxEffectiveThrust => subtype.MaxEffectiveThrust;

            public float MaxThrust => subtype.MaxThrust;

            public float ThrustEffectiveness => subtype.MaxEffectiveThrust / subtype.MaxThrust;

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.Thruster);
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                var buf = block.textBuffer;

                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Effectiveness), nameFormat);
                builder.Add(" ", nameFormat);

                buf.Clear();
                buf.Append(Math.Round(ThrustEffectiveness * 100f, 2));
                buf.Append("%\n");
                builder.Add(buf, valueFormat);

                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_ThrustOverride), nameFormat);
                builder.Add(": ", nameFormat);

                if (Override > 0f)
                {
                    buf.Clear();
                    TerminalUtilities.GetForceDisplay(Override * ThrustEffectiveness, buf);
                    buf.Append('\n');

                    builder.Add(buf, valueFormat);
                }
                else
                    builder.Add(MyTexts.TrySubstitute("Disabled"), valueFormat);
            }
        }
    }
}