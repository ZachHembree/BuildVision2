using VRageMath;
using System;

namespace DarkHelmet.UI
{
    using Client;
    using Server;

    public class SliderBar : HudElementBase
    {
        public override float Width
        {
            set
            {
                base.Width = value;

                if (vertical)
                {
                    button.Width = value;
                    //bar.Width = Math.Min(bar.Width, button.Width);
                }
                else
                {
                    bar.Width = value;
                    //button.Width = Math.Min(bar.Width, button.Width);
                }
            }
        }

        public override float Height
        {
            set
            {
                base.Height = value;

                if (vertical)
                {
                    bar.Height = value; 
                    //button.Height = Math.Min(button.Height, bar.Height);
                }
                else
                {
                    button.Height = value;
                    //bar.Height = Math.Min(button.Height, bar.Height);
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

                if (max > min)
                    Percent = (current - min) / (max - min);
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

                if (max > min)
                    Percent = (current - min) / (max - min);
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
                if (max > min)
                    Percent = (value - min) / (max - min);
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
                current = percent * (Max - Min);

                UpdateButtonOffset();
            }
        }
        public bool Reverse { get; set; }

        public readonly Button button, bar;
        private float min, max, current, percent;
        private bool canMoveSlider;
        private readonly bool vertical;

        public SliderBar(IHudParent parent = null, bool vertical = false) : base(parent)
        {
            bar = new Button(this)
            { Color = new Color(150, 150, 150, 255) };

            button = new Button(bar)
            { Color = new Color(200, 200, 200, 255) };

            this.vertical = vertical;

            if (vertical)
            {
                bar.Width = 3f;
                button.Size = new Vector2(24f, 6f);
            }
            else
            {
                bar.Height = 3f;
                button.Size = new Vector2(6f, 24f);
            }

            bar.MouseInput.OnLeftClick += BarClicked;
            button.MouseInput.OnLeftClick += BarClicked;

            min = 0f;
            max = 1f;

            Current = 0f;
            Percent = 0f;
        }

        private void BarClicked()
        {
            canMoveSlider = true;
        }

        protected override void Draw()
        {
            if (canMoveSlider)
            {
                float minOffset, maxOffset, pos;

                if (vertical)
                {
                    minOffset = -(bar.Height - button.Height) / 2f;
                    maxOffset = -minOffset;
                    pos = Utils.Math.Clamp(HudMain.Cursor.Origin.Y - Origin.Y, minOffset, maxOffset);
                }
                else
                {
                    minOffset = -(bar.Width - button.Width) / 2f;
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
            if (vertical)
            {
                if (Reverse)
                    button.Offset = new Vector2(button.Offset.X, -(Percent - .5f) * (bar.Height - button.Height));
                else
                    button.Offset = new Vector2(button.Offset.X, (Percent - .5f) * (bar.Height - button.Height));
            }
            else
            {
                if (Reverse)
                    button.Offset = new Vector2(-(Percent - .5f) * (bar.Width - button.Width), button.Offset.Y);
                else
                    button.Offset = new Vector2((Percent - .5f) * (bar.Width - button.Width), button.Offset.Y);
            }
        }

        protected override void HandleInput()
        {
            if (canMoveSlider && !SharedBinds.LeftButton.IsPressed)
            {
                canMoveSlider = false;
            }
        }
    }
}