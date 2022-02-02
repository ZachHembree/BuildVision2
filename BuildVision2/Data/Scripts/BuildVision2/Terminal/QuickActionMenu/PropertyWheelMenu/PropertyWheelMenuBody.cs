using RichHudFramework.Client;
using RichHudFramework.IO;
using RichHudFramework.UI;
using System;
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
        private class Body : HudElementBase
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

            private readonly TexturedBox background;
            private readonly Label summaryText;

            private readonly ColorWidget colorWidget;
            private readonly ComboWidget comboWidget;
            private readonly FloatWidget floatWidget;
            private readonly TextWidget textWidget;

            private readonly Action CloseWidgetCallback;
            private QuickActionWidgetBase activeWidget;

            private readonly RichText summaryBuilder;
            private readonly PropertyWheelMenu propertyWheelMenu;
            private int tick;

            public Body(PropertyWheelMenu parent) : base(parent)
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
                    BuilderMode = TextBuilderModes.Wrapped,
                    AutoResize = false,
                    DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding
                };

                colorWidget = new ColorWidget(this) { Visible = false };
                comboWidget = new ComboWidget(this) { Visible = false };
                floatWidget = new FloatWidget(this) { Visible = false };
                textWidget = new TextWidget(this) { Visible = false };

                summaryBuilder = new RichText();
                CloseWidgetCallback = CloseWidget;

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

            private void OpenWidget(QuickActionWidgetBase widget, object data)
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

            protected override void Layout()
            {
                if (tick == 0)
                {
                    if (activeWidget == null)
                    {
                        PropertyBlock block = propertyWheelMenu.quickActionMenu.Duplicator.Block;
                        summaryBuilder.Clear();
                        summaryBuilder.Add("Build Vision\n", mainHeaderFormat);

                        foreach (SuperBlock.SubtypeAccessorBase subtype in block.SubtypeAccessors)
                        {
                            if (subtype != null)
                                subtype.GetSummary(summaryBuilder, bodyFormatCenter, valueFormatCenter);
                        }

                        summaryText.Padding = .1f * (cachedSize - cachedPadding);
                        summaryText.TextBoard.SetText(summaryBuilder);
                    }
                }

                tick++;
                tick %= textTickDivider;
            }
        }
    }
}