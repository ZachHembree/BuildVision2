using RichHudFramework.UI;
using System.Collections.Generic;
using Sandbox.Game.Components;
using Sandbox.Definitions;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using VRage;
using VRage.Utils;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
	public partial class SuperBlock
	{
		public ButtonPanelAccessor ButtonPanel => _buttonPanel;

		private ButtonPanelAccessor _buttonPanel;

		public class ButtonPanelAccessor : SubtypeAccessor<IMyButtonPanel>
		{
			/// <summary>
			/// Returns true if anyone can use the button panel
			/// </summary>
			public bool AnyoneCanUse => subtype.AnyoneCanUse;

			/// <summary>
			/// Returns a list of panel buttons
			/// </summary>
			public IReadOnlyList<Button> Buttons => buttons;

			private List<Button> buttons;
			private MyButtonPanelDefinition panelDef;

			public ButtonPanelAccessor()
			{
				buttons = new List<Button>();
			}

			public override void SetBlock(SuperBlock block)
			{
				base.SetBlock(block, TBlockSubtypes.ButtonPanel);

				if (subtype != null)
				{
					panelDef = block.BlockDefinition as MyButtonPanelDefinition;

					if (panelDef != null)
					{
						for (int i = 0; i < panelDef.ButtonCount; i++)
							buttons.Add(new Button(subtype, i));
					}
				}
			}

			public override void Reset()
			{
				base.Reset();
				buttons.Clear();
			}

			public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
			{
				builder.Add(MyTexts.GetString(MySpaceTexts.BlockPropertyText_AnyoneCanUse), nameFormat);
				builder.Add(": ", nameFormat);
				builder.Add(MyTexts.GetString(AnyoneCanUse ? MySpaceTexts.SwitchText_On : MySpaceTexts.SwitchText_Off), valueFormat);
				builder.Add('\n');
			}

			/// <summary>
			/// Wrapper over individual IMyButtonPanel buttons
			/// </summary>
			public struct Button
			{
				/// <summary>
				/// Gets or sets cusotm button label
				/// </summary>
				public string CustomName
				{
					get { return panel.GetButtonName(index); }
					set { panel.SetCustomButtonName(index, value); }
				}

				/// <summary>
				/// Returns true if the button is assigned to an event
				/// </summary>
				public bool IsAssigned => panel.IsButtonAssigned(index);

				private readonly IMyButtonPanel panel;
				private readonly int index;

				public Button(IMyButtonPanel panel, int index)
				{
					this.panel = panel;
					this.index = index;
				}
			}
		}
	}
}
