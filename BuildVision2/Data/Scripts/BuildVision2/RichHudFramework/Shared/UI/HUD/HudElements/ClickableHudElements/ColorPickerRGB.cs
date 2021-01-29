﻿using System;
using System.Text;
using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI.Server
{
    using UI;

    /// <summary>
    /// Named color picker using sliders designed to mimic the appearance of the color picker in the SE terminal.
    /// RGB only. Alpha not supported.
    /// </summary>
    public class ColorPickerRGB : HudElementBase
    {
        /// <summary>
        /// Text rendered by the label
        /// </summary>
        public RichText Name { get { return name.TextBoard.GetText(); } set { name.TextBoard.SetText(value); } }

        /// <summary>
        /// Text builder backing the label
        /// </summary>
        public ITextBuilder NameBuilder => name.TextBoard;

        public override float Width
        {
            set
            {
                if (value > Padding.X)
                    value -= Padding.X;

                _absoluteWidth = (value / Scale);
                display.Width = value - name.Width;
                colorSliderColumn.Width = display.Width;
            }
        }

        public override float Height
        {
            set
            {
                if (value > Padding.Y)
                    value -= Padding.Y;

                _absoluteHeight = (value / Scale);
                value = (value - headerChain.Height - 15f) / 3f;
                colorNameColumn.MemberMaxSize = new Vector2(colorNameColumn.MemberMaxSize.X, value);
                colorSliderColumn.MemberMaxSize = new Vector2(colorSliderColumn.MemberMaxSize.X, value);
            }
        }

        /// <summary>
        /// Color currently specified by the color picker
        /// </summary>
        public Color Color 
        { 
            get { return _color; }
            set 
            {
                r.Current = value.R;
                g.Current = value.G;
                b.Current = value.B;
                _color = value;
            }
        }

        // Header
        private readonly Label name;
        private readonly TexturedBox display;
        private readonly HudChain headerChain;
        // Slider text
        private readonly Label rText, gText, bText;
        private readonly HudChain<HudElementContainer<Label>, Label> colorNameColumn;
        // Sliders
        private readonly SliderBox r, g, b;
        private readonly HudChain<HudElementContainer<SliderBox>, SliderBox> colorSliderColumn;

        private readonly HudChain mainChain, colorChain;
        private readonly StringBuilder valueBuilder;
        private Color _color;

        public ColorPickerRGB(HudParentBase parent) : base(parent)
        {
            // Header
            name = new Label()
            {
                Format = GlyphFormat.Blueish.WithSize(1.08f),
                Text = "NewColorPicker",
                AutoResize = false,
                Size = new Vector2(88f, 22f)
            };

            display = new TexturedBox()
            {
                Width = 231f,
            };

            var dispBorder = new BorderBox(display)
            {
                Color = Color.White,
                Thickness = 1f,
                DimAlignment = DimAlignments.Both,
            };

            headerChain = new HudChain(false)
            {
                SizingMode = HudChainSizingModes.FitMembersOffAxis | HudChainSizingModes.FitChainBoth,
                Height = 22f,
                Spacing = 0f,
                CollectionContainer = { name, display }
            };

            // Color picker
            rText = new Label() { AutoResize = false, Format = TerminalFormatting.ControlFormat, Height = 47f };
            gText = new Label() { AutoResize = false, Format = TerminalFormatting.ControlFormat, Height = 47f };
            bText = new Label() { AutoResize = false, Format = TerminalFormatting.ControlFormat, Height = 47f };

            colorNameColumn = new HudChain<HudElementContainer<Label>, Label>(true)
            {
                SizingMode = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.FitChainBoth,
                Width = 87f,
                Spacing = 5f,
                CollectionContainer = { rText, gText, bText }
            };

            r = new SliderBox() { Min = 0f, Max = 255f, Height = 47f };
            g = new SliderBox() { Min = 0f, Max = 255f, Height = 47f };
            b = new SliderBox() { Min = 0f, Max = 255f, Height = 47f };

            colorSliderColumn = new HudChain<HudElementContainer<SliderBox>, SliderBox>(true)
            {
                SizingMode = HudChainSizingModes.FitMembersBoth | HudChainSizingModes.FitChainBoth,
                Width = 231f,
                Spacing = 5f,
                CollectionContainer = { r, g, b }
            };

            colorChain = new HudChain(false)
            {
                SizingMode = HudChainSizingModes.FitChainBoth,
                CollectionContainer =
                {
                    colorNameColumn,
                    colorSliderColumn,
                }
            };

            mainChain = new HudChain(true, this)
            {
                SizingMode = HudChainSizingModes.FitChainBoth,
                Spacing = 5f,
                CollectionContainer =
                {
                    headerChain,
                    colorChain,
                }
            };

            Size = new Vector2(318f, 163f);
            valueBuilder = new StringBuilder();
        }

        public ColorPickerRGB() : this(null)
        { }

        protected override void HandleInput(Vector2 cursorPos)
        {
            _color = new Color()
            {
                R = (byte)Math.Round(r.Current),
                G = (byte)Math.Round(g.Current),
                B = (byte)Math.Round(b.Current),
                A = 255
            };

            valueBuilder.Clear();
            valueBuilder.Append("R: ");
            valueBuilder.Append(_color.R);
            rText.TextBoard.SetText(valueBuilder);

            valueBuilder.Clear();
            valueBuilder.Append("G: ");
            valueBuilder.Append(_color.G);
            gText.TextBoard.SetText(valueBuilder);

            valueBuilder.Clear();
            valueBuilder.Append("B: ");
            valueBuilder.Append(_color.B);
            bText.TextBoard.SetText(valueBuilder);

            display.Color = _color;
        }
    }
}