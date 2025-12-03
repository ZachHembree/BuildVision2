using RichHudFramework.UI;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
	public partial class SuperBlock
	{
		public FarmPlotAccessor FarmPlot => _farmPlot;

		private FarmPlotAccessor _farmPlot;

		public class FarmPlotAccessor : ComponentAccessor<IMyFarmPlotLogic>
		{
			/// <summary>
			/// Returns true if the plant is alive
			/// </summary>
			public bool IsAlive => component.IsAlive;

			/// <summary>
			/// Returns true if the plant has finished growing
			/// </summary>
			public bool IsGrown => component.IsPlantFullyGrown;

			/// <summary>
			/// Returns true if a plant is in the plot
			/// </summary>
			public bool IsPlanted => component.IsPlantPlanted;

			/// <summary>
			/// Returns the name of the plant in the plot
			/// </summary>
			public string PlantName => plantDef?.DisplayNameText ?? MyTexts.GetString(MySpaceTexts.None);

			/// <summary>
			/// Detailed info from plant management screen
			/// </summary>
			public string PlanterInfo => component.GetDetailedInfoWithoutRequiredInput();

			private MyDefinitionBase plantDef;

			public override void SetBlock(SuperBlock block)
			{
				base.SetBlock(block, TBlockSubtypes.FarmPlot);

				if (component != null && component.OutputItem != default(MyDefinitionId))
				{
					plantDef = MyDefinitionManager.Static.GetDefinition(component.OutputItem);
				}
			}

			public override void Reset()
			{
				base.Reset();
				plantDef = null;
			}

			public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
			{
				string detailedInfo = component.GetDetailedInfoWithoutRequiredInput();

				/*
					Need: Crop Type, Growth Progress, Grow Time, Crop Health, Water Level and Usage

					Components to investigate:
					MyResourceSinkComponent
					MyResourceSourceComponent
					MyResourceStorageComponent
				 */

				// Crop type
				builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyProperties_CurrentCropType), nameFormat);
				builder.Add(PlantName, valueFormat);
				builder.Add('\n');

				// Growth Progres
				var textBuf = block.textBuffer;

				if (IsAlive)
				{
					textBuf.Clear();

					if (TerminalUtilities.TryParseValueFromInfoString(detailedInfo, MySpaceTexts.BlockPropertyProperties_GrowthProgress, textBuf))
					{
						builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyProperties_GrowthProgress), nameFormat);
						textBuf.Append('\n');
						builder.Add(textBuf, valueFormat);
					}

					// Grow Time
					textBuf.Clear();

					if (TerminalUtilities.TryParseValueFromInfoString(detailedInfo, MySpaceTexts.BlockPropertyProperties_GrowTime, textBuf))
					{
						builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyProperties_GrowTime), nameFormat);
						textBuf.Append('\n');
						builder.Add(textBuf, valueFormat);
					}
				}

				// Crop Health
				textBuf.Clear();

				if (TerminalUtilities.TryParseValueFromInfoString(detailedInfo, MySpaceTexts.BlockPropertyProperties_CropHealth, textBuf))
				{
					builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyProperties_CropHealth), nameFormat);
					textBuf.Append('\n');
					builder.Add(textBuf, valueFormat);
				}

				if (IsAlive)
				{
					// Water Level
					textBuf.Clear();

					if (TerminalUtilities.TryParseValueFromInfoString(detailedInfo, MySpaceTexts.BlockPropertyProperties_WaterLevel, textBuf))
					{
						builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyProperties_WaterLevel), nameFormat);
						textBuf.Append('\n');
						builder.Add(textBuf, valueFormat);
					}

					// Water Usage 
					textBuf.Clear();

					if (TerminalUtilities.TryParseValueFromInfoString(detailedInfo, MySpaceTexts.BlockPropertyProperties_WaterUsage, textBuf))
					{
						builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyProperties_WaterUsage), nameFormat);
						textBuf.Append('\n');
						builder.Add(textBuf, valueFormat);
					}
				}
			}
		}
	}
}
