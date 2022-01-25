using RichHudFramework.UI;
using System;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private class ColorWidget : BlockValueWidgetBase
        {
            private readonly ColorPickerRGB colorPicker;

            private IBlockValue<Color> colorMember;
            private Color initColor;

            public ColorWidget(HudParentBase parent = null) : base(parent)
            {
                colorPicker = new ColorPickerRGB();
                cancelButton.Visible = true;

                var layout = new HudChain(true, this)
                {
                    Padding = new Vector2(20f),
                    DimAlignment = DimAlignments.Width,
                    SizingMode = HudChainSizingModes.FitMembersOffAxis,
                    Spacing = 8f,
                    CollectionContainer = 
                    {
                        colorPicker,
                        buttonChain
                    }
                };
            }

            public override void SetMember(IBlockMember member, Action CloseWidgetCallback)
            {
                colorMember = member as IBlockValue<Color>;
                this.CloseWidgetCallback = CloseWidgetCallback;

                initColor = colorMember.Value;
                colorPicker.Color = colorMember.Value;
                colorPicker.NameBuilder.SetText(colorMember.Name);
            }

            public override void Reset()
            {
                colorMember = null;
                CloseWidgetCallback = null;
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                colorMember.Value = colorPicker.Color;

                if (confirmButton.MouseInput.IsLeftClicked)
                {
                    CloseWidgetCallback();
                }
                else if (cancelButton.MouseInput.IsLeftClicked)
                {
                    colorMember.Value = initColor;
                    CloseWidgetCallback();
                }
            }
        }
    }
}