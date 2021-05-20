using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRage;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public SensorAccessor Sensor  { get { return _sensor; } private set { _sensor = value; } }

        private SensorAccessor _sensor;

        public class SensorAccessor : SubtypeAccessor<IMySensorBlock>
        {
            public bool IsEntityDetected => subtype.IsActive && block.TBlock.IsWorking;

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.Sensor);
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                builder.Add(MyTexts.TrySubstitute("Detected"), nameFormat);
                builder.Add(": ", nameFormat);

                builder.Add(MyTexts.TrySubstitute(IsEntityDetected.ToString()), valueFormat);
            }
        }
    }
}