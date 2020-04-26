using RichHudFramework.UI;
using Sandbox.ModAPI;
using VRage;
using VRage.Game.ModAPI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public ProductionAccessorBase Production { get; set; }

        public class ProductionAccessorBase : SubtypeAccessorBase
        {
            public float? Productivity 
            {
                get 
                {
                    float value;

                    if (production.UpgradeValues.TryGetValue("Productivity", out value))
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

                    if (production.UpgradeValues.TryGetValue("Effectiveness", out value))
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

                    if (production.UpgradeValues.TryGetValue("PowerEfficiency", out value))
                        return value;
                    else
                        return null;
                }
            }

            private readonly IMyProductionBlock production;

            public ProductionAccessorBase(SuperBlock block) : base(block, TBlockSubtypes.Production)
            {
                production = block.TBlock as IMyProductionBlock;
            }

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                var summary = new RichText();

                if (Productivity != null)
                {
                    summary.Add(new RichText
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Productivity)} ", nameFormat },
                        { $"{(Productivity.Value * 100f).ToString("G5")}%\n", valueFormat } 
                    });
                }

                if (Effectiveness != null)
                {
                    summary.Add(new RichText
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Effectiveness)} ", nameFormat },
                        { $"{(Effectiveness.Value * 100f).ToString("G5")}%\n", valueFormat }
                    });
                }

                if (PowerEfficiency != null)
                {
                    summary.Add(new RichText
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Efficiency)} ", nameFormat },
                        { $"{(PowerEfficiency.Value * 100f).ToString("G5")}%\n", valueFormat }
                    });
                }

                return summary;
            }
        }
    }
}