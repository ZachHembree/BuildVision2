using RichHudFramework.UI;
using RichHudFramework;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.ModAPI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public ProductionAccessorBase Production { get; set; }

        public class ProductionAccessorBase : SubtypeAccessor<IMyProductionBlock>
        {
            public float? Productivity 
            {
                get 
                {
                    float value;

                    if (subtype.UpgradeValues.TryGetValue("Productivity", out value))
                        return value + 1f;
                    else
                        return null;
                }
            }

            public float? Effectiveness
            {
                get 
                {
                    float value;

                    if (subtype.UpgradeValues.TryGetValue("Effectiveness", out value))
                        return value;
                    else
                        return null;
                }
            }

            public float? PowerEfficiency
            {
                get
                {
                    float value;

                    if (subtype.UpgradeValues.TryGetValue("PowerEfficiency", out value))
                        return value;
                    else
                        return null;
                }
            }

            public ProductionAccessorBase(SuperBlock block) : base(block, TBlockSubtypes.Production)
            { }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                if (Productivity != null)
                {
                    builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Productivity)} ", nameFormat);
                    builder.Add($"{(Productivity.Value * 100f).Round(2)}%\n", valueFormat);
                }

                if (Effectiveness != null)
                {
                    builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Effectiveness)} ", nameFormat);
                    builder.Add($"{(Effectiveness.Value * 100f).Round(2)}%\n", valueFormat);
                }

                if (PowerEfficiency != null)
                {
                    builder.Add($"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Efficiency)} ", nameFormat);
                    builder.Add($"{(PowerEfficiency.Value * 100f).Round(2)}%\n", valueFormat);
                }
            }
        }
    }
}