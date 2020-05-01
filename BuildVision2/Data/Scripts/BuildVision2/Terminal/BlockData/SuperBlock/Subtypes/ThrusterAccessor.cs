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

        public class ThrusterAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Controls the current thrust override.
            /// </summary>
            public float Override { get { return thruster.ThrustOverride; } set { thruster.ThrustOverride = value; } }

            /// <summary>
            /// Indicates the current thrust output.
            /// </summary>
            public float CurrentThrust => thruster.CurrentThrust;

            /// <summary>
            /// Indicates the maximum effective thrust.
            /// </summary>
            public float MaxEffectiveThrust => thruster.MaxEffectiveThrust;

            public float MaxThrust => thruster.MaxThrust;

            public float ThrustEffectiveness => thruster.MaxEffectiveThrust / thruster.MaxThrust;

            private readonly IMyThrust thruster;

            public ThrusterAccessor(SuperBlock block) : base(block, TBlockSubtypes.Thruster)
            {
                thruster = block.TBlock as IMyThrust;
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText 
                {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Effectiveness)} ", nameFormat },
                    { $"{(ThrustEffectiveness * 100f).Round(2)}%\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_ThrustOverride)}: ", nameFormat },
                    { Override > 0f ? $"{TerminalUtilities.GetForceDisplay(Override * ThrustEffectiveness)}\n" : MyTexts.TrySubstitute("Disabled"), valueFormat },
                };
            }
        }
    }
}