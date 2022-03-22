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
            private float animPos;

            public PropertyWheelMenuBody(PropertyWheelMenu parent) : base(parent)
            {
                this.propertyWheelMenu = parent;
                background = new TexturedBox(this)
                {
                    Material = Material.CircleMat,
                    Color = headerColor,
                    Size = Vector2.Zero,
                };

                summaryLabel = new Label(this)
                {
                    AutoResize = false,
                    BuilderMode = TextBuilderModes.Wrapped,
                    Width = 200f,
                };

                notificationText = new Label(this)
                {
                    AutoResize = true,
                    ParentAlignment = ParentAlignments.Bottom | ParentAlignments.Inner,
                    BuilderMode = TextBuilderModes.Wrapped,
                    Width = 150f,
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

                Padding = new Vector2(wheelBodyPeekPadding);
                Size = new Vector2(maxPeekWrapWidth);
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
                if (tick == 0)
                {
                    if (ActiveWidget == null)
                    {
                        UpdateText();
                    }                    
                }

                if (MenuState == QuickActionMenuState.WheelPeek)
                {
                    Padding = new Vector2(wheelBodyPeekPadding);
                    Size = new Vector2(maxPeekWrapWidth);
                }
                else
                {
                    Padding = new Vector2(wheelBodyPadding);
                    Size = 1.05f * propertyWheelMenu.Size * propertyWheelMenu.InnerDiam;
                }

                summaryLabel.Size = new Vector2(maxPeekWrapWidth - wheelBodyPeekPadding);

                Vector2 textSize = new Vector2(summaryLabel.TextBoard.TextSize.Length());
                textSize.Y += notificationText.Height;
                textSize = new Vector2(Math.Max(textSize.X, textSize.Y));
                textSize = Vector2.Min(Size, textSize);
                textSize = new Vector2(Math.Max(textSize.X, textSize.Y));

                if (MenuState == QuickActionMenuState.WheelPeek)
                {
                    Size = textSize + Padding;
                    summaryLabel.Size = new Vector2(textSize.X, textSize.Y - notificationText.Height);
                }

                if ((background.Size - Size).LengthSquared() > 1f)
                    animPos = 0f;

                if (animPos < 1f)
                {
                    animPos += .3f;
                    background.Size = Vector2.Lerp(background.Size, Size, animPos * QuickActionHudSpace.AnimScale);
                }

                tick++;
                tick %= textTickDivider;
            }

            private void UpdateText()
            {
                IPropertyBlock block = propertyWheelMenu.quickActionMenu.Target;
                summaryBuilder.Clear();
                summaryBuilder.Add("Build Vision\n", wheelHeaderFormat);
                block.GetSummary(summaryBuilder, bodyFormatCenter, bodyValueFormatCenter);

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
                else if ((MenuState & QuickActionMenuState.PropertyDuplication) > 0)
                {
                    textBuf.Clear();
                    textBuf.Append("Copying ");
                    textBuf.Append(block.Duplicator.GetSelectedEntryCount());
                    textBuf.Append(" of ");
                    textBuf.Append(block.Duplicator.GetValidEntryCount());
                    notificationBuidler.SetText(textBuf, bodyValueFormatCenter);
                }
                else
                {
                    notification = null;
                    notificationBuidler.Clear();
                    var target = propertyWheelMenu.quickActionMenu.Target;

                    if (!target.IsFunctional)
                        notificationBuidler.SetText("[Incomplete]", blockIncFormat.WithAlignment(TextAlignment.Center));
                }

                summaryLabel.TextBoard.SetText(summaryBuilder);
            }
        }
    }
}