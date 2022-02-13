using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RichHudFramework.UI.Rendering;
using System;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private class FloatWidget : BlockValueWidgetBase
        {
            private readonly CustomSliderBox sliderBox;

            private IBlockNumericValue<float> floatMember;
            private IBlockTextMember textMember;
            private float initValue;

            public FloatWidget(HudParentBase parent = null) : base(parent)
            {
                sliderBox = new CustomSliderBox() { Padding = Vector2.Zero };

                var layout = new HudChain(true, this)
                {
                    DimAlignment = DimAlignments.Width,
                    SizingMode = HudChainSizingModes.FitMembersOffAxis,
                    Spacing = 8f,
                    CollectionContainer =
                    {
                        sliderBox,
                        buttonChain
                    }
                };
            }

            public override void SetData(object member, Action CloseWidgetCallback)
            {
                floatMember = member as IBlockNumericValue<float>;
                textMember = member as IBlockTextMember;
                this.CloseWidgetCallback = CloseWidgetCallback;

                sliderBox.Min = floatMember.MinValue;
                sliderBox.Max = floatMember.MaxValue;
                sliderBox.Current = floatMember.Value;
                sliderBox.NameBuilder.SetText(floatMember.Name);

                sliderBox.MouseInput.GetInputFocus();
                sliderBox.CharFilterFunc = textMember.CharFilterFunc;
                initValue = floatMember.Value;

                if (BindManager.IsChatOpen && !sliderBox.IsTextInputOpen)
                {
                    sliderBox.FieldText.SetText(textMember.ValueText);
                    sliderBox.OpenTextInput();
                }
            }

            public override void Reset()
            {
                floatMember = null;
                CloseWidgetCallback = null;
                sliderBox.CloseTextInput();
            }

            protected override void Layout()
            {
                sliderBox.ValueBuilder.SetText(floatMember.FormattedValue);
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                if (SharedBinds.Enter.IsNewPressed)
                {
                    if (BindManager.IsChatOpen && !sliderBox.IsTextInputOpen)
                    {
                        sliderBox.FieldText.SetText(textMember.ValueText);
                        sliderBox.OpenTextInput();
                    }
                    else if (!BindManager.IsChatOpen && sliderBox.IsTextInputOpen)
                        Confirm();
                }
                else if (!sliderBox.IsTextInputOpen)
                {
                    floatMember.Value = sliderBox.Current;

                    if (!sliderBox.IsTextInputOpen && (BvBinds.ScrollUp.IsNewPressed || BvBinds.ScrollDown.IsNewPressed))
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
                if (sliderBox.IsTextInputOpen)
                {
                    float newValue;

                    if (float.TryParse(sliderBox.FieldText.ToString(), out newValue))
                        floatMember.Value = newValue;

                    sliderBox.CloseTextInput();
                }

                CloseWidgetCallback?.Invoke();
            }

            protected override void Cancel()
            {
                if (!sliderBox.IsTextInputOpen)
                {
                    floatMember.Value = initValue;
                }
                else
                {
                    sliderBox.CloseTextInput();
                }

                CloseWidgetCallback?.Invoke();
            }

            private class CustomSliderBox : NamedSliderBox
            {
                public bool IsTextInputOpen => textField.InputOpen;

                public ITextBuilder FieldText => textField.TextBoard;

                public Func<char, bool> CharFilterFunc 
                { 
                    get { return textField.CharFilterFunc; } 
                    set { textField.CharFilterFunc = value; } 
                }

                private readonly TextField textField;

                public CustomSliderBox(HudParentBase parent = null) : base(parent)
                {
                    textField = new TextField()
                    {
                        Height = 47f,
                        DimAlignment = DimAlignments.Width | DimAlignments.IgnorePadding,
                        ParentAlignment = ParentAlignments.Bottom | ParentAlignments.InnerV,
                        Visible = false,
                    };
                    textField.Register(this, true);
                }

                public void OpenTextInput()
                {
                    if (!IsTextInputOpen)
                    {
                        sliderBox.Visible = false;
                        textField.Visible = true;
                        current.Visible = false;

                        textField.OpenInput();
                        textField.MouseInput.GetInputFocus();
                    }
                }

                public void CloseTextInput()
                {
                    if (IsTextInputOpen)
                    {
                        current.Visible = true;
                        sliderBox.Visible = true;
                        textField.Visible = false;

                        textField.CloseInput();
                        sliderBox.MouseInput.GetInputFocus();
                    }
                }
            }
        }
    }
}