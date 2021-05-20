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
        private GasTankAccessor _gasTank;

        /// <summary>
        /// Provides access to block tank members, if defined.
        /// </summary>
        public GasTankAccessor GasTank
        {
            get
            {
                return _gasTank;
            }
            private set
            {
                _gasTank = value;
            }
        }

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

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.GasTank);
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                var buf = block.textBuffer;

                // Fill
                builder.Add(MyTexts.TrySubstitute("Gas"), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add($"{Fill:G6} / {Capacity:G6} L ", valueFormat);

                buf.Clear();
                buf.Append('(');
                buf.Append(Math.Round(FillRatio * 100d, 2));
                buf.Append("%)\n");
                builder.Add(buf, nameFormat);

                // Stockpile status
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_Stockpile), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(Stockpile ? MyTexts.GetString(MySpaceTexts.HudInfoOn) : MyTexts.GetString(MySpaceTexts.HudInfoOff), valueFormat);
                builder.Add("\n");

                // Auto refil enabled/disabled
                builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyTitle_AutoRefill), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(AutoRefillBottles ? MyTexts.GetString(MySpaceTexts.HudInfoOn) : MyTexts.GetString(MySpaceTexts.HudInfoOff), valueFormat);
                builder.Add("\n");
            }
        }
    }
}