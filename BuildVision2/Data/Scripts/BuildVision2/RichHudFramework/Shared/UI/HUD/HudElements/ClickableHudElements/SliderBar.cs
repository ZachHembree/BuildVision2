using VRageMath;
using System;

namespace RichHudFramework.UI
{
    using Client;
    using Server;

    /// <summary>
    /// Generic clickable slider bar. Can be oriented vertically or horizontally. Current value
    /// automatically clamped between min and max.
    /// </summary>
    public class SliderBar : HudElementBase, IClickableElement
    {
        /// <summary>
        /// Width of the sliderbar.
        /// </summary>
        public override float Width
        {
            get 
            {
                if (Vertical)
                    return Math.Max(bar.Width, slider.Width) + Padding.X;
                else
                    return bar.Width;
            }
            set
            {
                if (value > Padding.X)
                    value -= Padding.X;

                if (bar.Width >= slider.Width)
                {
                    bar.Width = value;
                    slider.Width = Math.Min(slider.Width, bar.Width);
                }
                else
                {
                    slider.Width = value;
                    bar.Width = Math.Min(slider.Width, bar.Width);
                }
            }
        }

        /// <summary>
        /// Height of the sliderbar.
        /// </summary>
        public override float Height
        {
            get
            {
                if (Vertical)
                    return bar.Height;
                else
                    return Math.Max(bar.Height, slider.Height) + Padding.Y;
            }
            set
            {
                if (value > Padding.Y)
                    value -= Padding.Y;

                if (bar.Height >= slider.Height)
                {
                    bar.Height = value;
                    slider.Height = Math.Min(slider.Height, bar.Height);
                }
                else
                {
                    slider.Height = value;
                    bar.Height = Math.Min(slider.Height, bar.Height);
                }
            }
        }

        /// <summary>
        /// Lower limit.
        /// </summary>
        public float Min
        {
            get { return _min; }
            set
            {
                _min = value;

                if (_max - _min != 0)
                    Percent = (_current - _min) / (_max - _min);
                else
                    Percent = 0;
            }
        }

        /// <summary>
        /// Upper limit for the slider.
        /// </summary>
        public float Max
        {
            get { return _max; }
            set
            {
                _max = value;

                if (_max - _min != 0)
                    Percent = (_current - _min) / (_max - _min);
                else
                    Percent = 0;
            }
        }

        /// <summary>
        /// Currently selected value bounded by the given Min and Max values.
        /// </summary>
        public float Current
        {
            get { return _current; }
            set
            {
                if (_max - _min != 0)
                    Percent = (value - _min) / (_max - _min);
                else
                    Percent = 0;
            }
        }

        /// <summary>
        /// Position of the slider given as a percentage. At 0, the slider will be at its minimum value;
        /// at 1, the slider will be at the given maximum value.
        /// </summary>
        public float Percent
        {
            get { return _percent; }
            set
            {
                _percent = MathHelper.Clamp(value, 0f, 1f);
                _current = _percent * (Max - Min) + Min;

                UpdateButtonOffset();
            }
        }

        /// <summary>
        /// Color of the slider bar
        /// </summary>
        public Color BarColor { get; set; }

        /// <summary>
        /// Bar color when moused over
        /// </summary>
        public Color BarHighlight { get; set; }

        /// <summary>
        /// Color of the slider box when not moused over
        /// </summary>
        public Color SliderColor { get; set; }

        /// <summary>
        /// Color of the slider button when moused over
        /// </summary>
        public Color SliderHighlight { get; set; }

        /// <summary>
        /// Size of the slider bar
        /// </summary>
        public Vector2 BarSize { get { return bar.Size; } set { bar.Size = value; } }

        /// <summary>
        /// Width of the slider bar
        /// </summary>
        public float BarWidth { get { return bar.Width; } set { bar.Width = value; } }

        /// <summary>
        /// Height of the slider bar
        /// </summary>
        public float BarHeight { get { return bar.Height; } set { bar.Height = value; } }

        /// <summary>
        /// Size of the slider button
        /// </summary>
        public Vector2 SliderSize { get { return slider.Size; } set { slider.Size = value; } }

        /// <summary>
        /// Width of the slider button.
        /// </summary>
        public float SliderWidth { get { return slider.Width; } set { slider.Width = value; } }

        /// <summary>
        /// Height of the slider button
        /// </summary>
        public float SliderHeight { get { return slider.Height; } set { slider.Height = value; } }

        /// <summary>
        /// Determines whether or not the slider button is currently visible
        /// </summary>
        public bool SliderVisible { get { return slider.Visible; } set { slider.Visible = value; } }

        /// <summary>
        /// If true, the slider will be oriented vertically s.t. the slider moves up and down.
        /// </summary>
        public bool Vertical { get; set; }

        /// <summary>
        /// Reverses the direction of the slider w/respect to its value.
        /// </summary>
        public bool Reverse { get; set; }

        /// <summary>
        /// Indicates whether or not the hud element is currently moused over
        /// </summary>
        public override bool IsMousedOver => mouseInput.IsMousedOver;

        /// <summary>
        /// Handles mouse input for the slider bar
        /// </summary>
        public IMouseInput MouseInput => mouseInput;

        private readonly TexturedBox slider, bar;
        private readonly MouseInputElement mouseInput;

        private float _min, _max, _current, _percent;
        private bool canMoveSlider;

        public SliderBar(HudParentBase parent = null) : base(parent)
        {
            mouseInput = new MouseInputElement(this) { DimAlignment = DimAlignments.Both };
            mouseInput.OnLeftClick += BarClicked;

            bar = new TexturedBox(this);
            slider = new TexturedBox(bar);

            bar.Size = new Vector2(100f, 12f);
            slider.Size = new Vector2(6f, 12f);

            SliderColor = new Color(180, 180, 180, 255);
            BarColor = new Color(140, 140, 140, 255);

            SliderHighlight = new Color(200, 200, 200, 255);

            _min = 0f;
            _max = 1f;

            Current = 0f;
            Percent = 0f;
        }

        private void BarClicked(object sender, EventArgs args)
        {
            canMoveSlider = true;
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (canMoveSlider && !SharedBinds.LeftButton.IsPressed)
            {
                canMoveSlider = false;
            }

            if (IsMousedOver || canMoveSlider)
            {
                slider.Color = SliderHighlight;

                if (BarHighlight != default(Color))
                    bar.Color = BarHighlight;
            }
            else
            {
                slider.Color = SliderColor;
                bar.Color = BarColor;
            }
        }

        protected override void Layout()
        {
            if (canMoveSlider)
            {
                float minOffset, maxOffset, pos;

                if (Vertical)
                {
                    minOffset = -(bar.Height - slider.Height) / 2f;
                    maxOffset = -minOffset;
                    pos = MathHelper.Clamp(HudMain.Cursor.ScreenPos.Y - Origin.Y, minOffset, maxOffset);
                }
                else
                {
                    minOffset = -(bar.Width - slider.Width) / 2f;
                    maxOffset = -minOffset;
                    pos = MathHelper.Clamp(HudMain.Cursor.ScreenPos.X - Origin.X, minOffset, maxOffset);
                }

                if (Reverse)
                    Percent = 1f - ((pos - minOffset) / (maxOffset - minOffset));
                else 
                    Percent = (pos - minOffset) / (maxOffset - minOffset);
            }

            UpdateButtonOffset();
        }

        private void UpdateButtonOffset()
        {
            if (Vertical)
            {
                if (Reverse)
                    slider.Offset = new Vector2(0f, -(Percent - .5f) * (bar.Height - slider.Height));
                else
                    slider.Offset = new Vector2(0f, (Percent - .5f) * (bar.Height - slider.Height));
            }
            else
            {
                if (Reverse)
                    slider.Offset = new Vector2(-(Percent - .5f) * (bar.Width - slider.Width), 0f);
                else
                    slider.Offset = new Vector2((Percent - .5f) * (bar.Width - slider.Width), 0f);
            }
        }
    }
}