using RichHudFramework.UI;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage;
using VRage.Utils;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;
using IMyRefinery = Sandbox.ModAPI.Ingame.IMyRefinery;
using System.Text;
using Sandbox.Definitions;

namespace DarkHelmet.BuildVision2
{
	public partial class SuperBlock
	{
		public RefineryAccessor Refinery => _refinery;

		private RefineryAccessor _refinery;

		public class RefineryAccessor : SubtypeAccessor<IMyRefinery>
		{
			/// <summary>
			/// Starting ore refining speed
			/// </summary>
			public float BaseProductivity => refineryDef?.RefineSpeed ?? 0f;

			/// <summary>
			/// Base ore refining efficiency
			/// </summary>
			public float BaseEffectiveness => (refineryDef?.MaterialEfficiency ?? 1f) - 1f;

			private MyRefineryDefinition refineryDef;

			public RefineryAccessor()
			{ }

			public override void SetBlock(SuperBlock block)
			{
				base.SetBlock(block, TBlockSubtypes.Refinery);
				
				if (block.SubtypeId.HasFlag(TBlockSubtypes.Refinery))
				{
					refineryDef = block.BlockDefinition as MyRefineryDefinition;
				}
			}

			public override void Reset()
			{
				base.Reset();
				refineryDef = null;
			}

			public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
			{ }
		}
	}
}
