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
				var textBuf = block.textBuffer;
				textBuf.Clear();

				if (TerminalUtilities.TryParseValueFromInfoString(subtype.DetailedInfo, MySpaceTexts.BlockPropertiesText_OxygenOutput, textBuf))
				{
					builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertiesText_OxygenOutput), nameFormat);
					textBuf.Append('\n');
					builder.Add(textBuf, valueFormat);
				}
			}
		}
	}
}
