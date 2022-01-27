using RichHudFramework.UI;
using System;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private class FloatWidget : BlockValueWidgetBase
        {
            private readonly TextField textField;
            private readonly NamedSliderBox sliderBox;

            private IBlockNumericValue<float> floatMember;
            private IBlockTextMember textMember;
            private float initValue;
            private bool useManualEntry;

            public FloatWidget(HudParentBase parent = null) : base(parent)
            {
                textField = new TextField() { Visible = false };
                sliderBox = new NamedSliderBox() { Padding = Vector2.Zero };
                useManualEntry = false;

                var layout = new HudChain(true, this)
                {
                    DimAlignment = DimAlignments.Width,
                    SizingMode = HudChainSizingModes.FitMembersOffAxis,
                    Spacing = 8f,
                    CollectionContainer =
                    {
                        textField,
                        sliderBox,
                        buttonChain
                    }
                };
            }

            public override void SetMember(IBlockMember member, Action CloseWidgetCallback)
            {
                floatMember = member as IBlockNumericValue<float>;
                textMember = member as IBlockTextMember;
                this.CloseWidgetCallback = CloseWidgetCallback;

                sliderBox.Min = floatMember.MinValue;
                sliderBox.Max = floatMember.MaxValue;
                sliderBox.Current = floatMember.Value;
                sliderBox.NameBuilder.SetText(floatMember.Name);

                textField.CharFilterFunc = textMember.CharFilterFunc;
                initValue = floatMember.Value;
            }

            public override void Reset()
            {
                floatMember = null;
                CloseWidgetCallback = null;
                CloseFieldInput();
            }

            protected override void Layout()
            {
                sliderBox.ValueBuilder.SetText(floatMember.FormattedValue);
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                if (!useManualEntry)
                {
                    floatMember.Value = sliderBox.Current;

                    if (SharedBinds.Control.IsNewPressed && sliderBox.MouseInput.IsNewLeftClicked)
                    {
                        OpenFieldInput();
                    }
                    else if (BvBinds.ScrollUp.IsNewPressed || BvBinds.ScrollDown.IsNewPressed)
                    {
                        float offset = floatMember.Increment;

                        if (BvBinds.MultZ.IsPressed)
                            offset *= BvConfig.Current.block.floatMult.Z;
                        else if (BvBinds.MultY.IsPressed)
                            offset *= BvConfig.Current.block.floatMult.Y;
                        else if (BvBinds.MultX.IsPressed)
                            offset *= BvConfig.Current.block.floatMult.X;

                        if (BvBinds.ScrollUp.IsNewPressed)
                        {
                            sliderBox.Current += offset;
                        }
                        else if (BvBinds.ScrollDown.IsNewPressed)
                        {
                            sliderBox.Current -= offset;
                        }
                    }
                }

                base.HandleInput(cursorPos);
            }

            protected override void Confirm()
            {
                if (!useManualEntry)
                { }
                else
                {
                    float newValue;

                    if (float.TryParse(textField.TextBoard.ToString(), out newValue))
                        floatMember.Value = newValue;

                    CloseFieldInput();
                }

                CloseWidgetCallback?.Invoke();
            }

            protected override void Cancel()
            {
                if (!useManualEntry)
                {
                    floatMember.Value = initValue;
                }
                else
                {
                    CloseFieldInput();
                }

                CloseWidgetCallback?.Invoke();
            }

            private void OpenFieldInput()
            {
                textField.Visible = true;
                sliderBox.Visible = false;

                textField.TextBoard.SetText(floatMember.ValueText);
                useManualEntry = true;
            }

            private void CloseFieldInput()
            {
                textField.Visible = false;
                sliderBox.Visible = true;
                useManualEntry = false;
            }
        }
    }
}