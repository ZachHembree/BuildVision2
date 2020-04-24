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
            public float MaxThrust => thruster.MaxEffectiveThrust;

            private readonly IMyThrust thruster;

            public ThrusterAccessor(SuperBlock block) : base(block, TBlockSubtypes.Thruster)
            {
                thruster = block.TBlock as IMyThrust;
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText 
                {
                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_ThrustOverride)}: ", nameFormat },
                    { $"{Override.Round(2)}n\n", valueFormat },
                };
            }
        }
    }
}