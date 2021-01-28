using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRage;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public ProjectorAccessor Projector  { get { return _projector; } private set { _projector = value; } }

        private ProjectorAccessor _projector;

        public class ProjectorAccessor : SubtypeAccessor<IMyProjector>
        {
            public string GridName => subtype.ProjectedGrid?.CustomName;

            public float PctComplete => TotalBlocks > 0f ? 100f * (TotalBlocks - RemainingBlocks) / TotalBlocks : 0f;

            public int RemainingBlocks => subtype.RemainingBlocks;

            public int BlocksBuilt => subtype.TotalBlocks - RemainingBlocks;

            public int TotalBlocks => subtype.TotalBlocks;

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.Projector);
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add($"{MyTexts.GetString(MySpaceTexts.TerminalTab_Info_ShipName)} ", nameFormat);
                builder.Add($"{GridName ?? MyTexts.TrySubstitute("None")}\n", valueFormat);

                if (GridName != null)
                {
                    builder.Add($"{MyTexts.GetString(MySpaceTexts.TerminalTab_Info_Blocks)} ", nameFormat);
                    builder.Add($"{BlocksBuilt} / {TotalBlocks} ", valueFormat);
                    builder.Add($"({PctComplete:G5}%)\n", nameFormat);
                }
            }
        }
    }
}