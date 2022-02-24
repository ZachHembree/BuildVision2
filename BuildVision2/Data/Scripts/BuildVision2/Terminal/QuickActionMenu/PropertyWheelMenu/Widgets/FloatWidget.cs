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
            private double absRange, logMax, logRange;

            public FloatWidget(HudParentBase parent = null) : base(parent)
            {
                sliderBox = new CustomSliderBox() 
                { 
                    Min = 0f,
                    Max = 1f,
                    Padding = Vector2.Zero,
                };

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

                absRange = Math.Abs(floatMember.MaxValue - floatMember.MinValue);
                logRange = Math.Ceiling(Math.Log10(absRange));
                logMax = Math.Log10(Math.Abs(floatMember.MaxValue));

                SetSliderValue(floatMember.Value);
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
                    floatMember.Value = GetSliderValue();

                    if (BvBinds.ScrollUp.IsNewPressed || BvBinds.ScrollDown.IsNewPressed)
                    {
                        float offset = Math.Min(floatMember.Increment, 1E5f);

                        if (BvBinds.MultZ.IsPressed)
                            offset *= BvConfig.Current.block.floatMult.Z;
                        else if (BvBinds.MultY.IsPressed)
                            offset *= BvConfig.Current.block.floatMult.Y;
                        else if (BvBinds.MultX.IsPressed)
                            offset *= BvConfig.Current.block.floatMult.X;

                        if (BvBinds.ScrollUp.IsNewPressed)
                        {
                            SetSliderValue(floatMember.Value + offset);
                        }
                        else if (BvBinds.ScrollDown.IsNewPressed)
                        {
                            SetSliderValue(floatMember.Value - offset);
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

            /// <summary>
            /// Sets slider value using unscaled value
            /// </summary>
            private void SetSliderValue(float current)
            {
                if (absRange > floatPropLogThreshold)
                {
                    current = MathHelper.Clamp(current, floatMember.MinValue, floatMember.MaxValue);
                    sliderBox.Current = (float)(Math.Log10(Math.Abs(current - floatMember.MinValue) + 1d) / logRange);
                }
                else
                {
                    sliderBox.Current = (float)((current - floatMember.MinValue) / absRange);
                }
            }

            /// <summary>
            /// Returns slider value without scaling
            /// </summary>
            private float GetSliderValue()
            {
                double value = sliderBox.Current;

                if (absRange > floatPropLogThreshold)
                {
                    value = Math.Pow(10d, value * logRange) - 1d + floatMember.MinValue;
                }
                else
                {
                    value = value * absRange + floatMember.MinValue;
                }

                return (float)value;
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