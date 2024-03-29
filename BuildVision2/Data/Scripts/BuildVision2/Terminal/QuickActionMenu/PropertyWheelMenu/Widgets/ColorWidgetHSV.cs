﻿using RichHudFramework.UI;
using RichHudFramework.UI.Rendering;
using RichHudFramework.UI.Rendering.Client;
using System;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public sealed partial class QuickActionMenu
    {
        private class ColorWidgetHSV : BlockValueWidgetBase
        {
            private readonly ColorPickerHSV colorPicker;

            private static float incrX, incrY, incrZ;
            private IBlockValue<Vector3> colorMember;
            private Vector3 initColor;
            private int selectedChannel;
            private bool channelSelected;

            public ColorWidgetHSV(HudParentBase parent = null) : base(parent)
            {
                colorPicker = new ColorPickerHSV();

                layout = new HudChain(true, this)
                {
                    Padding = new Vector2(20f),
                    DimAlignment = DimAlignments.Width,
                    SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.FitChainBoth,
                    Spacing = 8f,
                    CollectionContainer =
                    {
                        colorPicker,
                        buttonChain
                    }
                };
            }

            public override void SetData(object member, Action CloseWidgetCallback)
            {
                SetFont(FontManager.GetFont(BvConfig.Current.genUI.fontName));

                colorMember = member as IBlockValue<Vector3>;
                this.CloseWidgetCallback = CloseWidgetCallback;

                initColor = colorMember.Value;
                colorPicker.Color = colorMember.Value;
                colorPicker.NameBuilder.SetText(colorMember.Name);
                colorPicker.SetChannelFocused(selectedChannel);

                incrZ = BvConfig.Current.block.floatMult.Z; // x64
                incrY = BvConfig.Current.block.floatMult.Y; // x16
                incrX = BvConfig.Current.block.floatMult.X; // x8
            }

            public override void Reset()
            {
                selectedChannel = 0;
                channelSelected = false;
                colorMember = null;
                CloseWidgetCallback = null;
            }

            protected override void SetFont(IFontMin newFont)
            {
                base.SetFont(newFont);

                if (newFont != null)
                {
                    colorPicker.NameBuilder.SetFormatting(colorPicker.NameBuilder.Format.WithFont(newFont));
                    colorPicker.ValueFormat = colorPicker.ValueFormat.WithFont(newFont);
                }
            }

            protected override void Confirm()
            {
                if (channelSelected || IsMousedOver)
                {
                    channelSelected = false;
                    CloseWidgetCallback();
                }
                else
                {
                    channelSelected = true;
                }
            }

            protected override void Cancel()
            {
                if (channelSelected)
                {
                    channelSelected = false;
                }
                else
                {
                    colorMember.Value = initColor;
                    CloseWidgetCallback();
                }
            }

            protected override void HandleInput(Vector2 cursorPos)
            {
                colorMember.Value = colorPicker.Color;

                if (cancelButton.MouseInput.IsLeftReleased || BvBinds.Cancel.IsReleased)
                {
                    Cancel();
                }
                else if (confirmButton.MouseInput.IsLeftReleased ||
                    (BvBinds.Select.IsReleased && !IsMousedOver))
                {
                    Confirm();
                }
                else
                {
                    if (!IsMousedOver)
                    {
                        if (!channelSelected)
                        {
                            if (BvBinds.ScrollUp.IsNewPressed)
                                selectedChannel--;
                            else if (BvBinds.ScrollDown.IsNewPressed)
                                selectedChannel++;

                            if (BvBinds.Cancel.IsNewPressed)
                                cancelButton.MouseInput.GetInputFocus();
                            else if (!BvBinds.Cancel.IsReleased && !cancelButton.MouseInput.HasFocus)
                            {
                                selectedChannel = MathHelper.Clamp(selectedChannel, 0, 2);
                                colorPicker.SetChannelFocused(selectedChannel);
                            }
                        }
                        else
                        {
                            if (BvBinds.Select.IsNewPressed)
                                confirmButton.MouseInput.GetInputFocus();
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (colorPicker.sliders[i].MouseInput.HasFocus)
                                selectedChannel = i;
                        }

                        if (BvBinds.Select.IsReleased)
                            channelSelected = true;
                    }

                    if (channelSelected)
                    {
                        float offset = 1f;

                        if (BvBinds.MultZ.IsPressed)
                            offset *= incrZ;
                        else if (BvBinds.MultY.IsPressed)
                            offset *= incrY;
                        else if (BvBinds.MultXOrMouse.IsPressed)
                            offset *= incrX;

                        if (BvBinds.ScrollUp.IsNewPressed || BvBinds.ScrollUp.IsPressedAndHeld)
                            colorPicker.sliders[selectedChannel].Current += offset;
                        else if (BvBinds.ScrollDown.IsNewPressed || BvBinds.ScrollDown.IsPressedAndHeld)
                            colorPicker.sliders[selectedChannel].Current -= offset;
                    }
                }
            }
        }
    }
}