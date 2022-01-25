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
            public bool IsWidgetOpen => activeWidget != null;

            private readonly TexturedBox background;
            private readonly Label summaryText;

            private readonly ColorWidget colorWidget;
            private readonly ComboWidget comboWidget;
            private readonly FloatWidget floatWidget;
            private readonly TextWidget textWidget;
            private readonly Action CloseWidgetCallback;
            private BlockValueWidgetBase activeWidget;

            private readonly RichText summaryBuilder;
            private int tick;

            public Body(HudParentBase parent = null) : base(parent)
            {
                background = new TexturedBox(this)
                {
                    Material = Material.CircleMat,
                    Color = headerColor,
                    DimAlignment = DimAlignments.Both
                };

                summaryText = new Label(this)
                {
                    BuilderMode = TextBuilderModes.Lined,
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

            public void OpenMemberWidget(IBlockMember member)
            {
                CloseWidget();

                switch (member.ValueType)
                {
                    case BlockMemberValueTypes.Color:
                        colorWidget.SetMember(member, CloseWidgetCallback);
                        activeWidget = colorWidget;
                        break;
                    case BlockMemberValueTypes.Combo:
                        comboWidget.SetMember(member, CloseWidgetCallback);
                        activeWidget = comboWidget;
                        break;
                    case BlockMemberValueTypes.Float:
                        floatWidget.SetMember(member, CloseWidgetCallback);
                        activeWidget = floatWidget;
                        break;
                    case BlockMemberValueTypes.Text:
                        textWidget.SetMember(member, CloseWidgetCallback);
                        activeWidget = textWidget;
                        break;
                    default:
                        throw new Exception(
                            $"Widget for block value type {member?.ValueType} is unsupported.\n" +
                            $"Member Type: {member?.GetType()}"
                        );
                }

                activeWidget.Visible = true;
                summaryText.Visible = false;
            }

            public void CloseWidget()
            {
                if (activeWidget != null)
                {
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
                        summaryBuilder.Clear();

                        foreach (SuperBlock.SubtypeAccessorBase subtype in MenuManager.Target.SubtypeAccessors)
                        {
                            if (subtype != null)
                                subtype.GetSummary(summaryBuilder, bodyTextCenter, valueTextCenter);
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