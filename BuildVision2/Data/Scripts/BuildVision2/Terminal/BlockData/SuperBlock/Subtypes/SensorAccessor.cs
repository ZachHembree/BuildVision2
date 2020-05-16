using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRage;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public SensorAccessor Sensor { get; private set; }

        public class SensorAccessor : SubtypeAccessor<IMySensorBlock>
        {
            public bool IsEntityDetected => subtype.IsActive && block.TBlock.IsWorking;

            public SensorAccessor(SuperBlock block) : base(block, TBlockSubtypes.Sensor)
            { }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                return new RichText 
                {
                    { $"{MyTexts.TrySubstitute("Detected")}: ", nameFormat },
                    { $"{MyTexts.TrySubstitute(IsEntityDetected.ToString())}", valueFormat }
                };
            }
        }
    }
}