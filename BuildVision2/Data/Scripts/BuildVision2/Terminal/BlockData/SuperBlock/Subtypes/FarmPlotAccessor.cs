using RichHudFramework.UI;
using Sandbox.Game.Components;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using System.Text;
using VRage;
using VRage.Game.Entity.UseObject;
using VRage.Utils;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
	public partial class SuperBlock
	{
		public FarmPlotAccessor FarmPlot => _farmPlot;

		private FarmPlotAccessor _farmPlot;

		public class FarmPlotAccessor : ComponentAccessor<IMyFarmPlotLogic>
		{
			public bool IsAlive => component.IsAlive;

			public bool IsGrown => component.IsPlantFullyGrown;

			public bool IsPlanted => component.IsPlantPlanted;

			/// <summary>
			/// Detailed info subsection for planter info
			/// </summary>
			public string PlanterInfo => component.GetDetailedInfoWithoutRequiredInput();

			public override void SetBlock(SuperBlock block)
			{
				base.SetBlock(block, TBlockSubtypes.FarmPlot);
			}

			public void Harvest()
			{
				component.Harvest();
			}

			public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
			{
				string detailedInfo = component.GetDetailedInfoWithoutRequiredInput();
				var textBuf = block.textBuffer;
				textBuf.Clear();

				// Crop type
				if (block.TryParseValueFromInfoString(detailedInfo, MySpaceTexts.BlockPropertyProperties_CurrentCropType, textBuf))
				{
					builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyProperties_CurrentCropType), nameFormat);
					textBuf.Append('\n');
					builder.Add(textBuf, valueFormat);
				}

				textBuf.Clear();

				// Crop health
				if (block.TryParseValueFromInfoString(detailedInfo, MySpaceTexts.BlockPropertyProperties_CropHealth, textBuf))
				{
					builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyProperties_CropHealth), nameFormat);
					textBuf.Append('\n');
					builder.Add(textBuf, valueFormat);
				}

				textBuf.Clear();

				// Water level
				if (block.TryParseValueFromInfoString(detailedInfo, MySpaceTexts.BlockPropertyProperties_WaterLevel, textBuf))
				{
					builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyProperties_WaterLevel), nameFormat);
					textBuf.Append('\n');
					builder.Add(textBuf, valueFormat);
				}
			}
		}
	}
}
