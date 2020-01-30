using VRageMath;
using System;

namespace RichHudFramework.UI
{
    using Client;
    using Server;

    public class SliderBar : HudElementBase
    {
        public override float Width
        {
            get { return Math.Max(bar.Width, slider.Width) + Padding.X; }
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

        public override float Height
        {
            get { return Math.Max(bar.Height, slider.Height) + Padding.Y; }
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
            get { return min; }
            set
            {
                min = value;

                if (max - min != 0)
                    Percent = (current - min) / (max - min);
                else
                    Percent = 0;
            }
        }

        /// <summary>
        /// Upper limit for the slider.
        /// </summary>
        public float Max
        {
            get { return max; }
            set
            {
                max = value;

                if (max - min != 0)
                    Percent = (current - min) / (max - min);
                else
                    Percent = 0;
            }
        }

        /// <summary>
        /// Currently selected value bounded by the given Min and Max values.
        /// </summary>
        public float Current
        {
            get { return current; }
            set
            {
                if (max - min != 0)
                    Percent = (value - min) / (max - min);
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
            get { return percent; }
            set
            {
                percent = Utils.Math.Clamp(value, 0f, 1f);
                current = percent * (Max - Min) + Min;

                UpdateButtonOffset();
            }
        }

        /// <summary>
        /// The color of the slider bar
        /// </summary>
        public Color BarColor { get; set; }

        public Color BarHighlight { get; set; }

        /// <summary>
        /// The color of the slider box when not moused over.
        /// </summary>
        public Color SliderColor { get; set; }

        public Color SliderHighlight { get; set; }

        public Vector2 BarSize { get { return bar.Size; } set { bar.Size = value; } }

        public float BarWidth { get { return bar.Width; } set { bar.Width = value; } }

        public float BarHeight { get { return bar.Height; } set { bar.Height = value; } }

        public Vector2 SliderSize { get { return slider.Size; } set { slider.Size = value; } }

        public float SliderWidth { get { return slider.Width; } set { slider.Width = value; } }

        public float SliderHeight { get { return slider.Height; } set { slider.Height = value; } }

        public bool SliderVisible { get { return slider.Visible; } set { slider.Visible = value; } }

        /// <summary>
        /// If true, the slider will be oriented vertically s.t. the slider moves up and down.
        /// </summary>
        public bool Vertical { get; set; }

        /// <summary>
        /// Reverses the direction of the slider w/respect to its value.
        /// </summary>
        public bool Reverse { get; set; }

        public override bool IsMousedOver => mouseInput.IsMousedOver;

        public IClickableElement MouseInput => mouseInput;

        private readonly TexturedBox slider, bar;
        private readonly ClickableElement mouseInput;

        private float min, max, current, percent;
        private bool canMoveSlider;

        public SliderBar(IHudParent parent = null) : base(parent)
        {
            mouseInput = new ClickableElement(this) { DimAlignment = DimAlignments.Both };
            mouseInput.OnLeftClick += BarClicked;

            bar = new TexturedBox(this);
            slider = new TexturedBox(bar);

            bar.Size = new Vector2(100f, 12f);
            slider.Size = new Vector2(6f, 12f);

            SliderColor = new Color(180, 180, 180, 255);
            BarColor = new Color(140, 140, 140, 255);

            SliderHighlight = new Color(200, 200, 200, 255);

            min = 0f;
            max = 1f;

            Current = 0f;
            Percent = 0f;
        }

        private void BarClicked()
        {
            canMoveSlider = true;
        }

        protected override void HandleInput()
        {
            if (canMoveSlider && !SharedBinds.LeftButton.IsPressed)
            {
                canMoveSlider = false;
            }

            if (IsMousedOver)
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

        protected override void Draw()
        {
            if (canMoveSlider)
            {
                float minOffset, maxOffset, pos;

                if (Vertical)
                {
                    minOffset = -(bar.Height - slider.Height) / 2f;
                    maxOffset = -minOffset;
                    pos = Utils.Math.Clamp(HudMain.Cursor.Origin.Y - Origin.Y, minOffset, maxOffset);
                }
                else
                {
                    minOffset = -(bar.Width - slider.Width) / 2f;
                    maxOffset = -minOffset;
                    pos = Utils.Math.Clamp(HudMain.Cursor.Origin.X - Origin.X, minOffset, maxOffset);
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