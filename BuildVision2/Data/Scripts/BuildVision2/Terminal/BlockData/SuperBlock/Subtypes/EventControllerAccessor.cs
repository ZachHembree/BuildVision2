using RichHudFramework.UI;
using Sandbox.ModAPI;
using System.Text;
using VRage;
using VRage.Utils;
using MySpaceTexts = Sandbox.Game.Localization.MySpaceTexts;

namespace DarkHelmet.BuildVision2
{
	public partial class SuperBlock
	{
		public EventControllerAccessor Event
		{
			get
			{
				return _eventController;
			}
			private set
			{
				_eventController = value;
			}
		}

		private EventControllerAccessor _eventController;

		public class EventControllerAccessor : SubtypeAccessor<IMyEventControllerBlock>
		{
			/// <summary>
			/// Returns the name of the event currently selected
			/// </summary>
			public StringBuilder EventName => _eventName;

			/// <summary>
			/// Name ID corresponding to the event currently selected
			/// </summary>
			public MyStringId EventNameID => subtype.SelectedEvent.EventDisplayName;

			/// <summary>
			/// Returns the activation threshold for the event being monitored
			/// </summary>
			public float Threshold => subtype.Threshold;

			/// <summary>
			/// Returns true if the threshold is used
			/// </summary>
			public bool IsThresholdUsed => eventWithGui?.IsThresholdUsed ?? false;

			/// <summary>
			/// Returns true if the threshold condition is less than or equal. Otherwise, greater than or equal.
			/// </summary>
			public bool IsLowerOrEqualCondition => subtype.IsLowerOrEqualCondition;

			/// <summary>
			/// Returns true if the inequality conditions are used
			/// </summary>
			public bool IsConditionUsed => eventWithGui?.IsConditionSelectionUsed ?? false;

			/// <summary>
			/// Returns true if all blocks must meet the condition for the event
			/// </summary>
			public bool IsAndModeEnabled => subtype.IsAndModeEnabled;

			private IMyEventComponentWithGui eventWithGui;
			private readonly StringBuilder _eventName;

			public EventControllerAccessor()
			{
				_eventName = new StringBuilder();
				eventWithGui = null;
			}

			public override void SetBlock(SuperBlock block)
			{
				base.SetBlock(block, TBlockSubtypes.EventController);

				if (subtype?.SelectedEvent != null && subtype.SelectedEvent is IMyEventComponentWithGui)
				{
					eventWithGui = (IMyEventComponentWithGui)subtype.SelectedEvent;
					_eventName.Clear();
					string eventID = MyTexts.GetString(subtype.SelectedEvent.EventDisplayName);
					TerminalUtilities.GetBeautifiedTypeID(eventID, _eventName);
				}
			}

			public override void Reset()
			{
				base.Reset();
				_eventName.Clear();
				eventWithGui = null;
			}

			public override void GetSummary(RichText builder, GlyphFormat nameFormat, GlyphFormat valueFormat)
			{
				builder.Add($"{MyTexts.GetString(MySpaceTexts.EventControllerBlock_AvailableEvents)}: ", nameFormat);
				builder.Add(_eventName, valueFormat);

				if (IsThresholdUsed)
				{
					builder.Add($"\n{MyTexts.GetString(MySpaceTexts.EventControllerBlock_Threshold_Title)}: ", nameFormat);

					if (IsConditionUsed)
						builder.Add(subtype.IsLowerOrEqualCondition ? "<= " : ">= ", valueFormat);

					builder.Add($"{subtype.Threshold:P1}", valueFormat);
				}

				builder.Add($"\n{MyTexts.GetString(MySpaceTexts.EventControllerBlock_AgregateEvent_Title)}: ", nameFormat);
				builder.Add(MyTexts.TrySubstitute(subtype.IsAndModeEnabled.ToString()), valueFormat);
				builder.Add('\n', valueFormat);
			}
		}
	}
}