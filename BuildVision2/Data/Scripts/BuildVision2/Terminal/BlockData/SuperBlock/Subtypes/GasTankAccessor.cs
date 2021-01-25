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

        public class GasTankAccessor : SubtypeAccessor<IMyGasTank>
        {
            /// <summary>
            /// Returns the capacity of the tank
            /// </summary>
            public double Capacity => subtype.Capacity;

            public double Fill => subtype.Capacity * subtype.FilledRatio;

            /// <summary>
            /// Returns the fill ratio of the tank
            /// </summary>
            public double FillRatio => subtype.FilledRatio;

            public bool Stockpile { get { return subtype.Stockpile; } set { subtype.Stockpile = value; } }

            public bool AutoRefillBottles { get { return subtype.AutoRefillBottles; } set { subtype.AutoRefillBottles = value; } }

            public GasTankAccessor(SuperBlock block) : base(block, TBlockSubtypes.GasTank)
            { }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add($"{MyTexts.TrySubstitute("Gas")}: ", nameFormat);
                builder.Add($"{Fill.ToString("G6")} / {Capacity.ToString("G6")} L ", valueFormat);
                builder.Add($"({(FillRatio * 100d).Round(2)}%)\n", nameFormat);

                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_Stockpile)}: ", nameFormat);
                builder.Add($"{(Stockpile ? MyTexts.GetString(MySpaceTexts.HudInfoOn) : MyTexts.GetString(MySpaceTexts.HudInfoOff))}\n", valueFormat);

                builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_AutoRefill)}: ", nameFormat);
                builder.Add($"{(AutoRefillBottles ? MyTexts.GetString(MySpaceTexts.HudInfoOn) : MyTexts.GetString(MySpaceTexts.HudInfoOff))}\n", valueFormat);
            }
        }
    }
}