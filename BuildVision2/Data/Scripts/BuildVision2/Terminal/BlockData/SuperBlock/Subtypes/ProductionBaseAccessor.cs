using RichHudFramework;
using RichHudFramework.UI;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using System;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
    public partial class SuperBlock
    {
        public ProductionBaseAccessor Production  { get { return _production; } }

        private ProductionBaseAccessor _production;

        public class ProductionBaseAccessor : SubtypeAccessor<IMyProductionBlock>
        {
            /// <summary>
            /// Production speed scale
            /// </summary>
            public float? Productivity 
            {
                get 
                {
                    float value;

                    if (subtype.UpgradeValues.TryGetValue("Productivity", out value))
                    {
                        if (block.SubtypeId.HasFlag(TBlockSubtypes.Assembler))
                            value += block._assembler.BaseProductivity;
                        else if (block.SubtypeId.HasFlag(TBlockSubtypes.Refinery))
                            value += block._refinery.BaseProductivity;
                        else
                            value += 1f;
                            
                        return value;
					}
                    else
                        return null;
                }
            }

            /// <summary>
            /// Production efficiency. For refineries, this refers to material/ore refining efficiency.
            /// </summary>
            public float? Effectiveness
            {
                get 
                {
                    float value;

                    if (subtype.UpgradeValues.TryGetValue("Effectiveness", out value))
                    {
						if (block.SubtypeId.HasFlag(TBlockSubtypes.Refinery))
							value += block._refinery.BaseEffectiveness;
                            
						return value;
					}
                    else
                        return null;
                }
            }

            /// <summary>
            /// Power efficiency scale
            /// </summary>
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

                if (Productivity != null && Productivity != 1f)
                {
                    if (block.SubtypeId.HasFlag(TBlockSubtypes.Assembler))
                        builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Productivity_Assembler), nameFormat);
                    else
						builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Productivity), nameFormat);

					builder.Add(" ", nameFormat);

                    buf.Clear();
                    buf.Append($"{Productivity.Value:P0}\n");
                    builder.Add(buf, valueFormat);
                }
                
                if (Effectiveness != null && Effectiveness != 1f)
                {
                    builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Effectiveness), nameFormat);
                    builder.Add(" ", nameFormat);

                    buf.Clear();
                    buf.Append($"{Effectiveness.Value:P0}\n");
                    builder.Add(buf, valueFormat);
                }

                if (PowerEfficiency != null && PowerEfficiency != 1f)
                {
                    builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesText_Efficiency), nameFormat);
                    builder.Add(" ", nameFormat);

                    buf.Clear();
                    buf.Append($"{PowerEfficiency.Value:P0}\n");
                    builder.Add(buf, valueFormat);
                }
            }
        }
    }
}