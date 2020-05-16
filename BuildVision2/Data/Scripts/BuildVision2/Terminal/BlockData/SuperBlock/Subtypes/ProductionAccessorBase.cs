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

            public override RichText GetSummary(GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                var summary = new RichText();

                if (Productivity != null)
                {
                    summary.Add(new RichText
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Productivity)} ", nameFormat },
                        { $"{(Productivity.Value * 100f).Round(2)}%\n", valueFormat } 
                    });
                }

                if (Effectiveness != null)
                {
                    summary.Add(new RichText
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Effectiveness)} ", nameFormat },
                        { $"{(Effectiveness.Value * 100f).Round(2)}%\n", valueFormat }
                    });
                }

                if (PowerEfficiency != null)
                {
                    summary.Add(new RichText
                    {
                        { $"{MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Efficiency)} ", nameFormat },
                        { $"{(PowerEfficiency.Value * 100f).Round(2)}%\n", valueFormat }
                    });
                }

                return summary;
            }
        }
    }
}