using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRage;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public GeneralAccessor General { get; private set; }

        public class GeneralAccessor : SubtypeAccessorBase
        {
            public string CustomName { get { return block.TBlock.CustomName; } set { block.TBlock.CustomName = value; } }

            public GeneralAccessor(SuperBlock block) : base(block, TBlockSubtypes.None)
            { }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText
                {
                    { $"{MyTexts.TrySubstitute("Name")}: ", nameFormat },
                    { $"{CustomName}\n", valueFormat },
                };
            }
        }
    }
}