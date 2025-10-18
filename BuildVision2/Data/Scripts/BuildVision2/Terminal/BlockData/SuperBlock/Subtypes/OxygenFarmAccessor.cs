using ProtoBuf.Meta;
using RichHudFramework.UI;
using RichHudFramework.Internal;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.ModAPI;
using SpaceEngineers.Game.Entities.Blocks;
using SpaceEngineers.Game.ModAPI;
using System;
using VRage;
using VRage.Utils;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using System.Linq;
using VRage.Game.ModAPI;
using Sandbox.Definitions;
using Sandbox.ModAPI.Interfaces;

namespace DarkHelmet.BuildVision2
{
	public partial class SuperBlock
	{
		public OxygenFarmAccessor OxygenFarm => _oxygenFarm;

		private OxygenFarmAccessor _oxygenFarm;

		public class OxygenFarmAccessor : SubtypeAccessorBase
		{
			public bool CanProduce => subtype.CanProduce;

			private IMyOxygenFarm subtype;

			public override void SetBlock(SuperBlock block)
			{			
				this.block = block;
				subtype = block.TBlock as IMyOxygenFarm;
				this.SubtypeId = TBlockSubtypes.OxygenFarm;
				
				if (subtype != null)
				{
					block.SubtypeId |= TBlockSubtypes.OxygenFarm;
					block.subtypeAccessors.Add(this);
				}
			}

			public override void Reset()
			{
				block = null;
				subtype = null;
				SubtypeId = 0;
			}

			public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
			{
				string oxyFieldKey = MyTexts.GetString(MySpaceTexts.BlockPropertiesText_OxygenOutput);
				string infoText = subtype.DetailedInfo;
				int oxyFieldStart = infoText.IndexOf(oxyFieldKey);

				if (oxyFieldStart !=  -1)
				{
					int oxyValueStart = oxyFieldStart + oxyFieldKey.Length;
					var textBuf = block.textBuffer;
					textBuf.Clear();

					for (int i = oxyValueStart; i < infoText.Length; i++)
					{
						// Skip to text start
						if (textBuf.Length == 0 && infoText[i] <= ' ')
							oxyValueStart++;
						// Break on any special character
						else if (textBuf.Length > 0 && infoText[i] < ' ')
							break;
						// Append vaue
						else if (infoText[i] >= ' ')
							textBuf.Append(infoText[i]);
					}

					if (textBuf.Length > 0)
					{
						textBuf.Append('\n');
						builder.Add($"{MyTexts.Get(MySpaceTexts.BlockPropertiesText_OxygenOutput)}", nameFormat);
						builder.Add(textBuf, valueFormat);
					}
				}
			}
		}
	}
}
