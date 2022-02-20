using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using RichHudFramework;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Client;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu : HudElementBase
    {
        private class PropertyWheelMenuBody : HudElementBase
        {
            /// <summary>
            /// Returns true if a property widget is currently open
            /// </summary>
            public bool IsWidgetOpen => activeWidget != null;

            /// <summary>
            /// Gets/sets the menu's state
            /// </summary>
            private QuickActionMenuState MenuState
            {
                get { return propertyWheelMenu.quickActionMenu.MenuState; }
                set { propertyWheelMenu.quickActionMenu.MenuState = value; }
            }

            public readonly TexturedBox background;
            private readonly Label summaryText, notificationText;

            private readonly ColorWidget colorWidget;
            private readonly ComboWidget comboWidget;
            private readonly FloatWidget floatWidget;
            private readonly TextWidget textWidget;

            private readonly Action CloseWidgetCallback;
            private PropertyWheelWidgetBase activeWidget;

            private readonly RichText summaryBuilder;
            private readonly PropertyWheelMenu propertyWheelMenu;
            private readonly Stopwatch notificationTimer;
            private StringBuilder notification;
            private bool contNotification;
            private int tick;

            public PropertyWheelMenuBody(PropertyWheelMenu parent) : base(parent)
            {
                this.propertyWheelMenu = parent;
                background = new TexturedBox(this)
                {
                    Material = Material.CircleMat,
                    Color = headerColor,
                    DimAlignment = DimAlignments.Both
                };

                summaryText = new Label(this)
                {
                    DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding,
                    BuilderMode = TextBuilderModes.Wrapped,
                };

                notificationText = new Label(summaryText)
                {
                    ParentAlignment = ParentAlignments.Bottom,
                    BuilderMode = TextBuilderModes.Lined,
                };

                colorWidget = new ColorWidget(this) { Visible = false };
                comboWidget = new ComboWidget(this) { Visible = false };
                floatWidget = new FloatWidget(this) { Visible = false };
                textWidget = new TextWidget(this) { Visible = false };

                summaryBuilder = new RichText();
                CloseWidgetCallback = CloseWidget;

                notificationTimer = new Stopwatch();
                Padding = new Vector2(90f);
            }

            public void OpenBlockMemberWidget(IBlockMember member)
            {
                switch (member.ValueType)
                {
                    case BlockMemberValueTypes.Color:
                        OpenWidget(colorWidget, member);
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
                activeWidget = widget;

                activeWidget.Visible = true;
                summaryText.Visible = false;
            }

            public void CloseWidget()
            {
                if (activeWidget != null)
                {
                    HudMain.EnableCursor = false;
                    activeWidget.Reset();
                    activeWidget.Visible = false;
                    summaryText.Visible = true;
                    activeWidget = null;
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
                if (tick == 0)
                {
                    if (activeWidget == null)
                    {
                        UpdateText();
                    }

                    if (MenuState == QuickActionMenuState.Peek)
                    {
                        Size = new Vector2(270f);
                        Padding = new Vector2(40f);
                    }
                    else
                    {
                        Padding = new Vector2(90f);
                    }
                }

                tick++;
                tick %= textTickDivider;
            }

            private void UpdateText()
            {
                PropertyBlock block = propertyWheelMenu.quickActionMenu.Target;
                summaryBuilder.Clear();
                summaryBuilder.Add("Build Vision\n", mainHeaderFormat);

                foreach (SuperBlock.SubtypeAccessorBase subtype in block.SubtypeAccessors)
                {
                    if (subtype != null)
                        subtype.GetSummary(summaryBuilder, bodyFormatCenter, bodyValueFormatCenter);
                }

                ITextBuilder notificationBuidler = notificationText.TextBoard;

                if (notification != null && notificationTimer.ElapsedMilliseconds < notificationTime)
                {
                    notificationBuidler.Clear();
                    notificationBuidler.Append("\n", bodyFormatCenter);
                    notificationBuidler.Append(notification, bodyValueFormatCenter);

                    if (contNotification)
                    {
                        notification = null;
                        contNotification = false;
                    }
                }
                else
                {
                    notification = null;
                    notificationBuidler.Clear();
                    var target = propertyWheelMenu.quickActionMenu.Target;

                    if (!target.IsFunctional)
                        notificationBuidler.SetText("[Incomplete]", blockIncFormat.WithAlignment(TextAlignment.Center));
                }

                summaryText.Padding = .1f * (cachedSize - cachedPadding);
                summaryText.TextBoard.SetText(summaryBuilder);
            }
        }
    }
}