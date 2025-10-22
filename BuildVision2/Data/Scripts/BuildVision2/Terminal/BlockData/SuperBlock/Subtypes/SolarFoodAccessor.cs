using ProtoBuf.Meta;
using RichHudFramework;
using RichHudFramework.UI;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems.Conveyors;
using Sandbox.ModAPI;
using SpaceEngineers.Game.EntityComponents.Blocks;
using SpaceEngineers.Game.EntityComponents.GameLogic;
using SpaceEngineers.Game.ModAPI;
using System;
using VRage;
using static DarkHelmet.BuildVision2.SuperBlock;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
	public partial class SuperBlock
    {
        public SolarFoodAccessor SolarFood => _solarFood;

        private SolarFoodAccessor _solarFood;

        public class SolarFoodAccessor : ComponentAccessor<IMySolarFoodGenerator>
		{
            /// <summary>
            /// Number of food items produced per minute
            /// </summary>
            public float ItemsPerMinute => component.ItemsPerMinute;

            /// <summary>
            /// Time remaining, in seconds, until next batch of food is ready
            /// </summary>
            public float TimeRemainingUtilNextBatch => component.TimeRemainingUntilNextBatch;

            public SolarFoodAccessor()
            { }
            
            public override void SetBlock(SuperBlock block)
            {
				base.SetBlock(block, TBlockSubtypes.SolarFoodGenerator);
			}

			public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
            {
                float rate = ItemsPerMinute;

                if (rate != float.NaN)
                {
					if (rate < 1f) // Slow production
					{
						builder.Add($"{MyTexts.TrySubstitute("Minutes per Item:")} ", nameFormat);

						if (rate > 0f)
                        {
							TimeSpan minPerItem = TimeSpan.FromMinutes(1.0 / rate);
							builder.Add($"{minPerItem:mm\\:ss}\n", valueFormat);
						}
                        else // No production
							builder.Add($"--:--\n", valueFormat);
					}
					else // Fast production
					{
						builder.Add($"{MyTexts.TrySubstitute("Items per Minute:")} ", nameFormat);
						builder.Add($"{rate:G2}\n", valueFormat);
					}
				}

				float timeRem = TimeRemainingUtilNextBatch;

                if (timeRem > 0f && timeRem < 1E5f)
                {
					TimeSpan timeSpan = TimeSpan.FromSeconds(timeRem);
					builder.Add($"{MyTexts.Get(MySpaceTexts.BlockPropertyProperties_NextOutputIn)}", nameFormat);
					builder.Add($"{timeSpan:mm\\:ss}\n", valueFormat);
				}
            }
        }
    }
}