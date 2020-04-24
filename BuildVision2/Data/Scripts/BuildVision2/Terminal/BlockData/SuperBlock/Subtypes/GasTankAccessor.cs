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
        /// Provides access to block tank members, if defined.
        /// </summary>
        public GasTankAccessor GasTank { get; private set; }

        public class GasTankAccessor : SubtypeAccessorBase
        {
            /// <summary>
            /// Returns the capacity of the tank
            /// </summary>
            public double Capacity => gasTank.Capacity;

            /// <summary>
            /// Returns the fill ratio of the tank
            /// </summary>
            public double FillRatio => gasTank.FilledRatio;

            private readonly IMyGasTank gasTank;

            public GasTankAccessor(SuperBlock block) : base(block, TBlockSubtypes.GasTank)
            {
                gasTank = block.TBlock as IMyGasTank;
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText {
                    { $"{MyTexts.GetString(MySpaceTexts.Oxygen_Filled).Split(':')[0]}: ", nameFormat },
                    { $"{(FillRatio * 100d).Round(1)}%\n", valueFormat },
                };
            }
        }
    }
}