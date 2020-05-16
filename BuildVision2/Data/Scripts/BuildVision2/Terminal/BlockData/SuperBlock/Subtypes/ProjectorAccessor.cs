﻿using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public ProjectorAccessor Projector { get; private set; }

        public class ProjectorAccessor : SubtypeAccessor<IMyProjector>
        {
            public string GridName => subtype.ProjectedGrid?.CustomName;

            public float PctComplete => TotalBlocks > 0f ? 100f * (TotalBlocks - RemainingBlocks) / TotalBlocks : 0f;

            public int RemainingBlocks => subtype.RemainingBlocks;

            public int BlocksBuilt => subtype.TotalBlocks - RemainingBlocks;

            public int TotalBlocks => subtype.TotalBlocks;

            public ProjectorAccessor(SuperBlock block) : base(block, TBlockSubtypes.Projector)
            { }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                var summary = new RichText
                {
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalTab_Info_ShipName)} ", nameFormat },
                    { $"{GridName ?? MyTexts.TrySubstitute("None")}\n", valueFormat },
                };

                if (GridName != null)
                {
                    summary.Add(new RichText
                {
                    { $"{MyTexts.GetString(MySpaceTexts.TerminalTab_Info_Blocks)} ", nameFormat },
                    { $"{BlocksBuilt} / {TotalBlocks} ", valueFormat },
                    { $"({PctComplete:G5}%)\n", nameFormat },
                });
                }

                return summary;
            }
        }
    }
}