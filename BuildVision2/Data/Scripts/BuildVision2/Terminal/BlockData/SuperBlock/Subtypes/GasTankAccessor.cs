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

            public double Fill => gasTank.Capacity * gasTank.FilledRatio;

            /// <summary>
            /// Returns the fill ratio of the tank
            /// </summary>
            public double FillRatio => gasTank.FilledRatio;

            public bool Stockpile { get { return gasTank.Stockpile; } set { gasTank.Stockpile = value; } }

            public bool AutoRefillBottles { get { return gasTank.AutoRefillBottles; } set { gasTank.AutoRefillBottles = value; } }

            private readonly IMyGasTank gasTank;

            public GasTankAccessor(SuperBlock block) : base(block, TBlockSubtypes.GasTank)
            {
                gasTank = block.TBlock as IMyGasTank;
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText {
                    { $"{MyTexts.TrySubstitute("Gas")}: ", nameFormat },
                    { $"{Fill.ToString("G6")} / {Capacity.ToString("G6")} L ", valueFormat },
                    { $"({(FillRatio * 100d).Round(2)}%)\n", nameFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_Stockpile)}: ", nameFormat },
                    { $"{(Stockpile ? MyTexts.GetString(MySpaceTexts.HudInfoOn) : MyTexts.GetString(MySpaceTexts.HudInfoOff))}\n", valueFormat },

                    { $"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_AutoRefill)}: ", nameFormat },
                    { $"{(AutoRefillBottles ? MyTexts.GetString(MySpaceTexts.HudInfoOn) : MyTexts.GetString(MySpaceTexts.HudInfoOff))}\n", valueFormat },
                };
            }
        }
    }
}