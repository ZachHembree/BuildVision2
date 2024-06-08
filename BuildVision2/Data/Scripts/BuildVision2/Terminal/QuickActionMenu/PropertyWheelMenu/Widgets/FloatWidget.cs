using RichHudFramework.UI;
using RichHudFramework.UI.Client;
using RichHudFramework.UI.Rendering;
using System;
using Sandbox.ModAPI;
using VRage.Utils;
using VRageMath;
using RichHudFramework.UI.Rendering.Client;

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
            private double absRange, logRange;

            public FloatWidget(HudParentBase parent = null) : base(parent)
            {
                sliderBox = new CustomSliderBox()
                {
                    Min = 0f,
                    Max = 1f,
                    Padding = Vector2.Zero,
                };

                layout = new HudChain(true, this)
                {
                    DimAlignment = DimAlignments.UnpaddedSize,
                    SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.AlignMembersCenter,
                    Spacing = widgetInnerPadding,
                    CollectionContainer =
                    {
                        sliderBox,
                        buttonChain
                    }
                };
            }

            public override void SetData(object member, Action CloseWidgetCallback)
            {
                SetFont(FontManager.GetFont(BvConfig.Current.genUI.fontName));

                floatMember = member as IBlockNumericValue<float>;
                textMember = member as IBlockTextMember;
                this.CloseWidgetCallback = CloseWidgetCallback;

                absRange = Math.Abs(floatMember.MaxValue - floatMember.MinValue);
                logRange = Math.Ceiling(Math.Log10(absRange));

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

            protected override void SetFont(IFontMin newFont)
            {
                base.SetFont(newFont);

                if (newFont != null)
                {
                    sliderBox.NameBuilder.SetFormatting(sliderBox.NameBuilder.Format.WithFont(newFont));
                    sliderBox.ValueBuilder.SetFormatting(sliderBox.ValueBuilder.Format.WithFont(newFont));
                    sliderBox.FieldText.SetFormatting(sliderBox.FieldText.Format.WithFont(newFont));
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
                if (!sliderBox.IsTextInputOpen)
                {
                    ITextBuilder valueBuilder = sliderBox.ValueBuilder;
                    valueBuilder.Clear();

                    if (floatMember.StatusText != null && floatMember.StatusText.Length > 0)
                    {
                        valueBuilder.Append(floatMember.StatusText, wheelValueColor.WithAlignment(TextAlignment.Right));
                        valueBuilder.Append(" ");
                    }

                    valueBuilder.Append(floatMember.FormattedValue, wheelNameColor.WithAlignment(TextAlignment.Right));
                }
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                if (BindManager.IsChatOpen && !sliderBox.IsTextInputOpen
                    && MyAPIGateway.Input.IsNewGameControlPressed(MyStringId.Get("CHAT_SCREEN")) )
                {
                    sliderBox.FieldText.SetText(textMember.ValueText);
                    sliderBox.OpenTextInput();
                }
                else if (!BindManager.IsChatOpen && sliderBox.IsTextInputOpen && SharedBinds.Enter.IsNewPressed)
                {
                    Confirm();
                }
                else if (!sliderBox.IsTextInputOpen)
                {
                    floatMember.Value = GetSliderValue();

                    if (BvBinds.ScrollUp.IsPressed || BvBinds.ScrollDown.IsPressed)
                    {
                        double offset = Math.Min(floatMember.Increment, 1E5f);

                        if ((floatMember.Flags & BlockPropertyFlags.CanUseMultipliers) > 0)
                        {
                            float mult = 1f;

                            if (BvBinds.MultZ.IsPressed)
                                mult = BvConfig.Current.block.floatMult.Z;
                            else if (BvBinds.MultY.IsPressed)
                                mult = BvConfig.Current.block.floatMult.Y;
                            else if (BvBinds.MultXOrMouse.IsPressed)
                                mult = BvConfig.Current.block.floatMult.X;

                            if ((floatMember.Flags & BlockPropertyFlags.IsIntegral) == 0 
                                || MathHelper.IsEqual((float)(mult * offset), (int)(mult * offset)))
                            {
                                offset *= mult;
                            }
                        }

                        double value = floatMember.Value;

                        if (double.IsInfinity(value))
                            value = 0f;

                        if (BvBinds.ScrollUp.IsNewPressed || BvBinds.ScrollUp.IsPressedAndHeld)
                            SetSliderValue(value + offset);
                        else if (BvBinds.ScrollDown.IsNewPressed || BvBinds.ScrollDown.IsPressedAndHeld)
                            SetSliderValue(value - offset);
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
            private void SetSliderValue(double current)
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
                        DimAlignment = DimAlignments.UnpaddedWidth,
                        ParentAlignment = ParentAlignments.InnerBottom,
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