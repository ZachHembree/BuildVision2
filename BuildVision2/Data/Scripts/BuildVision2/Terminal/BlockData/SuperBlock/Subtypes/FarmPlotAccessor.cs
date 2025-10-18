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
				char lastChar = ' ';
				var textBuf = block.textBuffer;
				textBuf.Clear();
				
				//MySpaceTexts.BlockPropertyProperties_CurrentCropType
				//MySpaceTexts.BlockPropertyProperties_CropHealth
				//MySpaceTexts.BlockPropertyProperties_WaterLevel
				
				foreach (char c in detailedInfo)
				{
					// ':' marks key end
					if (c == ':')
					{
						textBuf.Append(c);
						builder.Add(textBuf, nameFormat);
						textBuf.Clear();
					}
					// Line breaks or any control char indicates value end
					else if (c < ' ')
					{
						textBuf.Append(c);
						builder.Add(textBuf, valueFormat);
						textBuf.Clear();
					}
					// Append printable chars, no double spaces
					else if (c >= ' ' && !(c == ' ' && lastChar == ' '))
						textBuf.Append(c);

					lastChar = c;
				}
				
				// Append trailing value
				if (textBuf.Length > 0)
				{
					textBuf.Append('\n');
					builder.Add(textBuf, valueFormat);
					textBuf.Clear();
				}
			}
		}
	}
}
