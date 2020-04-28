using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRage;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public SensorAccessor Sensor { get; private set; }

        public class SensorAccessor : SubtypeAccessorBase
        {
            public bool IsEntityDetected => sensor.IsActive && block.TBlock.IsWorking;

            private readonly IMySensorBlock sensor;

            public SensorAccessor(SuperBlock block) : base(block, TBlockSubtypes.Sensor)
            {
                sensor = block.TBlock as IMySensorBlock;
            }

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