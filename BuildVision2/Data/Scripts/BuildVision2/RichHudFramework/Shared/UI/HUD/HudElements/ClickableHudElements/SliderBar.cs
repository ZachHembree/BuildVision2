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
                    return (Math.Max(_barSize.X, _sliderSize.X) + _absolutePadding.X) * Scale;
                else
                    return _barSize.X * Scale;
            }
            set
            {
                value /= Scale;

                if (value > _absolutePadding.X)
                    value -= _absolutePadding.X;

                if (_barSize.X >= _sliderSize.X)
                {
                    _barSize.X = value;
                    _sliderSize.X = Math.Min(_sliderSize.X, _barSize.X);
                }
                else
                {
                    _sliderSize.X = value;
                    _barSize.X = Math.Min(_sliderSize.X, _barSize.X);
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
                    return _barSize.Y * Scale;
                else
                    return (Math.Max(_barSize.Y, _sliderSize.Y) + _absolutePadding.Y) * Scale;
            }
            set
            {
                value /= Scale;

                if (value > _absolutePadding.Y)
                    value -= _absolutePadding.Y;

                if (_barSize.Y >= _sliderSize.Y)
                {
                    _barSize.Y = value;
                    _sliderSize.Y = Math.Min(_sliderSize.Y, _barSize.Y);
                }
                else
                {
                    _sliderSize.Y = value;
                    _barSize.Y = Math.Min(_sliderSize.Y, _barSize.Y);
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
        public Vector2 BarSize { get { return _barSize * Scale; } set { _barSize = value / Scale; } }

        /// <summary>
        /// Width of the slider bar
        /// </summary>
        public float BarWidth { get { return _barSize.X * Scale; } set { _barSize.X = value / Scale; } }

        /// <summary>
        /// Height of the slider bar
        /// </summary>
        public float BarHeight { get { return _barSize.Y * Scale; } set { _barSize.Y = value / Scale; } }

        /// <summary>
        /// Size of the slider button
        /// </summary>
        public Vector2 SliderSize { get { return _sliderSize * Scale; } set { _sliderSize = value / Scale; } }

        /// <summary>
        /// Width of the slider button.
        /// </summary>
        public float SliderWidth { get { return _sliderSize.X * Scale; } set { _sliderSize.X = value / Scale; } }

        /// <summary>
        /// Height of the slider button
        /// </summary>
        public float SliderHeight { get { return _sliderSize.Y * Scale; } set { _sliderSize.Y = value / Scale; } }

        /// <summary>
        /// Determines whether or not the slider button is currently visible
        /// </summary>
        public bool SliderVisible { get; set; }

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

        protected readonly TexturedBox slider, bar;
        protected readonly MouseInputElement mouseInput;
        protected Vector2 _barSize, _sliderSize;

        protected float _min, _max, _current, _percent;
        protected bool canMoveSlider;

        public SliderBar(HudParentBase parent) : base(parent)
        {
            mouseInput = new MouseInputElement(this) { DimAlignment = DimAlignments.Both };
            mouseInput.LeftClicked += BarClicked;

            bar = new TexturedBox(this);
            slider = new TexturedBox(bar);

            _barSize = new Vector2(100f, 12f);
            _sliderSize = new Vector2(6f, 12f);
            SliderVisible = true;

            bar.Size = _barSize;
            slider.Size = _sliderSize;

            SliderColor = new Color(180, 180, 180, 255);
            BarColor = new Color(140, 140, 140, 255);
            SliderHighlight = new Color(200, 200, 200, 255);

            _min = 0f;
            _max = 1f;

            Current = 0f;
            Percent = 0f;
        }

        public SliderBar() : this(null)
        { }

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
        }

        protected override void Layout()
        {
            float scale = Scale;
            bar.Size = _barSize * scale;
            slider.Size = _sliderSize * scale;
            slider.Visible = SliderVisible;

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

            if (canMoveSlider)
            {
                float minOffset, maxOffset, pos;
                Vector3 cursorPos = HudSpace.CursorPos;

                if (Vertical)
                {
                    minOffset = -((_barSize.Y - _sliderSize.Y) / 2f) * scale;
                    maxOffset = -minOffset;
                    pos = MathHelper.Clamp(cursorPos.Y - Origin.Y, minOffset, maxOffset);
                }
                else
                {
                    minOffset = -((_barSize.X - _sliderSize.X) / 2f) * scale;
                    maxOffset = -minOffset;
                    pos = MathHelper.Clamp(cursorPos.X - Origin.X, minOffset, maxOffset);
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
                    slider.Offset = new Vector2(0f, -(Percent - .5f) * (_barSize.Y - _sliderSize.Y) * Scale);
                else
                    slider.Offset = new Vector2(0f, (Percent - .5f) * (_barSize.Y - _sliderSize.Y) * Scale);
            }
            else
            {
                if (Reverse)
                    slider.Offset = new Vector2(-(Percent - .5f) * (_barSize.X - _sliderSize.X) * Scale, 0f);
                else
                    slider.Offset = new Vector2((Percent - .5f) * (_barSize.X - _sliderSize.X) * Scale, 0f);
            }
        }
    }
}