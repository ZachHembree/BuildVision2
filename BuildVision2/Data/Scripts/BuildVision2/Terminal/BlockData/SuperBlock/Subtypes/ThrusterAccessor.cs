using Sandbox.ModAPI;
using VRage;
using RichHudFramework;
using RichHudFramework.UI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        /// <summary>
        /// Provides access to block thruster members.
        /// </summary>
        public ThrusterAccessor Thruster { get; private set; }

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

            public ThrusterAccessor(SuperBlock block) : base(block, TBlockSubtypes.Thruster)
            { }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Effectiveness)} ", nameFormat);
                builder.Add($"{(ThrustEffectiveness * 100f).Round(2)}%\n", valueFormat);

                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_ThrustOverride)}: ", nameFormat);
                builder.Add((Override > 0f ? $"{TerminalUtilities.GetForceDisplay(Override * ThrustEffectiveness)}\n" : MyTexts.TrySubstitute("Disabled")), valueFormat);
            }
        }
    }
}