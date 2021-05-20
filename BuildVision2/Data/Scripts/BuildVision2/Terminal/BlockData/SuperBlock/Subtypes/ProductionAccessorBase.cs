using RichHudFramework;
using RichHudFramework.UI;
using Sandbox.ModAPI;
using System;
using VRage;
using VRage.Game.ModAPI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public ProductionAccessorBase Production  { get { return _production; } set { _production = value; } }

        private ProductionAccessorBase _production;

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

            public override void SetBlock(SuperBlock block)
            {
                base.SetBlock(block, TBlockSubtypes.Production);
            }

            public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                var buf = block.textBuffer;

                if (Productivity != null)
                {
                    builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Productivity), nameFormat);
                    builder.Add(" ", nameFormat);

                    buf.Clear();
                    buf.Append(Math.Round(Productivity.Value * 100f, 2));
                    buf.Append("%\n");
                    builder.Add(buf, valueFormat);
                }

                if (Effectiveness != null)
                {
                    builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Effectiveness), nameFormat);
                    builder.Add(" ", nameFormat);

                    buf.Clear();
                    buf.Append(Math.Round(Effectiveness.Value * 100f, 2));
                    buf.Append("%\n");
                    builder.Add(buf, valueFormat);
                }

                if (PowerEfficiency != null)
                {
                    builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Efficiency), nameFormat);
                    builder.Add(" ", nameFormat);

                    buf.Clear();
                    buf.Append(Math.Round(PowerEfficiency.Value * 100f, 2));
                    buf.Append("%\n");
                    builder.Add(buf, valueFormat);
                }
            }
        }
    }
}