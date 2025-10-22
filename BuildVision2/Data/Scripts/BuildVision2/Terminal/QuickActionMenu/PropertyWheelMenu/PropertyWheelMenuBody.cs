using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RichHudFramework.UI.Rendering;
using System;
using System.Diagnostics;
using System.Text;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
	public sealed partial class QuickActionMenu : HudElementBase
	{
		private class PropertyWheelMenuBody : HudElementBase
		{
			/// <summary>
			/// Returns true if a property widget is currently open
			/// </summary>
			public bool IsWidgetOpen => ActiveWidget != null;

			/// <summary>
			/// Current open widget. Null if closed.
			/// </summary>
			public PropertyWheelWidgetBase ActiveWidget { get; private set; }

			/// <summary>
			/// Gets/sets the menu's state
			/// </summary>
			private QuickActionMenuState MenuState
			{
				get { return propertyWheelMenu.quickActionMenu.MenuState; }
				set { propertyWheelMenu.quickActionMenu.MenuState = value; }
			}

			public float AnimPos { get; private set; }

			public readonly TexturedBox background;
			private readonly Label summaryLabel, notificationText;

			private readonly ColorWidget colorWidget;
			private readonly ColorWidgetHSV colorWidgetHSV;
			private readonly ComboWidget comboWidget;
			private readonly FloatWidget floatWidget;
			private readonly TextWidget textWidget;

			private readonly Action CloseWidgetCallback;

			private readonly PropertyWheelMenu propertyWheelMenu;
			private readonly Stopwatch notificationTimer;
			private readonly RichText summaryBuilder;
			private readonly StringBuilder textBuf;
			private StringBuilder notification;
			private bool contNotification;
			private int tick;

			public PropertyWheelMenuBody(PropertyWheelMenu parent) : base(parent)
			{
				this.propertyWheelMenu = parent;
				background = new TexturedBox(this)
				{
					Material = Material.CircleMat,
					Color = HeaderColor,
					Size = Vector2.Zero,
				};

				summaryLabel = new Label(this)
				{
					BuilderMode = TextBuilderModes.Wrapped,
					Width = 200f,
					IsSelectivelyMasked = true,
					LineWrapWidth = WheelBodyMaxWrap - 10f
				};

				notificationText = new Label(this)
				{
					AutoResize = true,
					ParentAlignment = ParentAlignments.InnerBottom,
					BuilderMode = TextBuilderModes.Wrapped,
					Width = WheelNotifiationWidth,
					LineWrapWidth = WheelNotifiationWidth,
					Offset = new Vector2(0f, 30f),
				};

				colorWidget = new ColorWidget(this) { Visible = false };
				colorWidgetHSV = new ColorWidgetHSV(this) { Visible = false };
				comboWidget = new ComboWidget(this) { Visible = false };
				floatWidget = new FloatWidget(this) { Visible = false };
				textWidget = new TextWidget(this) { Visible = false };

				summaryBuilder = new RichText();
				CloseWidgetCallback = CloseWidget;

				textBuf = new StringBuilder();
				notificationTimer = new Stopwatch();

				Padding = new Vector2(WheelBodyPeekPadding);
				Size = new Vector2(0);
			}

			public void OpenBlockMemberWidget(IBlockMember member)
			{
				switch (member.ValueType)
				{
					case BlockMemberValueTypes.Color:
						OpenWidget(colorWidget, member);
						break;
					case BlockMemberValueTypes.ColorHSV:
						OpenWidget(colorWidgetHSV, member);
						break;
					case BlockMemberValueTypes.Combo:
						OpenWidget(comboWidget, member);
						break;
					case BlockMemberValueTypes.Float:
						OpenWidget(floatWidget, member);
						break;
					case BlockMemberValueTypes.Text:
						OpenWidget(textWidget, member);
						break;
					default:
						throw new Exception(
							$"Widget for block value type {member?.ValueType} is unsupported.\n" +
							$"Member Type: {member?.GetType()}"
						);
				}
			}

			private void OpenWidget(PropertyWheelWidgetBase widget, object data)
			{
				CloseWidget();
				widget.SetData(data, CloseWidgetCallback);

				ActiveWidget = widget;
				ActiveWidget.Visible = true;
				summaryLabel.Visible = false;
			}

			public void CloseWidget()
			{
				if (ActiveWidget != null)
				{
					HudMain.EnableCursor = false;
					summaryLabel.Visible = true;
					ActiveWidget.Reset();
					ActiveWidget.Visible = false;
					ActiveWidget = null;
				}
			}

			public void ShowNotification(StringBuilder notificationText, bool continuous = false)
			{
				this.notification = notificationText;
				contNotification = continuous;
				notificationTimer.Restart();
			}

			protected override void Layout()
			{
				const float AnimationStep = 0.3f;
				notificationText.Visible = !propertyWheelMenu.IsHiding;

				if (ActiveWidget == null)
					summaryLabel.Visible = !propertyWheelMenu.IsHiding;

				if (!propertyWheelMenu.IsHiding)
					UpdateSize();

				// Animate background size
				if (propertyWheelMenu.IsHiding)
				{
					AnimPos = MathHelper.Clamp(AnimPos - AnimationStep, 0f, 1f);
					Size = Vector2.Lerp(
						Vector2.Zero,
						background.UnpaddedSize,
						AnimPos
					);
					background.UnpaddedSize = Size;
				}
				else if (AnimPos < 1f)
				{
					AnimPos = MathHelper.Clamp(AnimPos + AnimationStep, 0f, 1f);
					background.UnpaddedSize = Vector2.Lerp(
						background.UnpaddedSize,
						Size,
						AnimPos
					);
				}
				else
					background.UnpaddedSize = Size;

				// Update tick counter
				tick = (tick + 1) % TextTickDivider;
			}

			private void UpdateSize()
			{
				// Initialize text on first tick if no active widget
				if (tick == 0 && ActiveWidget == null)
					UpdateText();

				// Adjust layout for peek
				if (MenuState == QuickActionMenuState.WheelPeek)
				{
					UnpaddedSize = summaryLabel.TextBoard.TextSize;
					// Elliptical bg padding
					Padding = Vector2.Min(
						((UnpaddedSize * Sqrt2) - UnpaddedSize) + WheelBodyPeekPadding, 
						new Vector2(WheelBodyInnerPadding)
					);
				}
				else
				{
					UnpaddedSize = new Vector2(WheelBodyMaxWrap);
					Padding = new Vector2(WheelBodyInnerPadding);
					summaryLabel.Offset = Vector2.Zero;
				}

				// Trigger animation if background size changes
				if ((background.UnpaddedSize - Size).LengthSquared() > 4f)
					AnimPos = 0f;
			}

			private void UpdateText()
			{
				IPropertyBlock block = propertyWheelMenu.quickActionMenu.Target;
				summaryBuilder.Clear();
				summaryBuilder.Add(BvMain.modName, WheelHeaderFormat);
				summaryBuilder.Add("\n", WheelHeaderFormat);
				block.GetSummary(summaryBuilder, BodyFormatCenter, BodyValueFormatCenter);

				ITextBuilder notificationBuidler = notificationText.TextBoard;

				if (notification != null && notificationTimer.ElapsedMilliseconds < NotificationTime)
				{
					notificationBuidler.Clear();
					notificationBuidler.Append("\n", BodyFormatCenter);
					notificationBuidler.Append(notification, BodyValueFormatCenter);

					if (contNotification)
					{
						notification = null;
						contNotification = false;
					}
				}
				else if ((MenuState & QuickActionMenuState.PropertyDuplication) > 0)
				{
					textBuf.Clear();
					textBuf.Append("Copying ");
					textBuf.Append(block.Duplicator.GetSelectedEntryCount());
					textBuf.Append(" of ");
					textBuf.Append(block.Duplicator.GetValidEntryCount());
					notificationBuidler.SetText(textBuf, BodyValueFormatCenter);
				}
				else
				{
					notification = null;
					notificationBuidler.Clear();
					var target = propertyWheelMenu.quickActionMenu.Target;

					if (!target.IsFunctional)
						notificationBuidler.SetText("[Incomplete]", BlockIncFormat.WithAlignment(TextAlignment.Center));
				}

				summaryLabel.TextBoard.SetText(summaryBuilder);
			}
		}
	}
}