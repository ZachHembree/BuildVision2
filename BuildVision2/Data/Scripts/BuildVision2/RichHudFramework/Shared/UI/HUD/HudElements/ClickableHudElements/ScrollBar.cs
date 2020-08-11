using VRageMath;
using System;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Clickable scrollbar. Designed to mimic the appearance of the scrollbars used in SE.
    /// </summary>
    public class ScrollBar : HudElementBase, IClickableElement
    {
        /// <summary>
        /// Width of the scrollbar.
        /// </summary>
        public override float Width
        {
            get { return slide.Width + Padding.X; }
            set
            {
                if (value > Padding.X)
                    value -= Padding.X;

                slide.BarWidth = value;

                if (Vertical)
                    slide.SliderWidth = value;
            }
        }

        /// <summary>
        /// Height of the scrollbar.
        /// </summary>
        public override float Height
        {
            get { return slide.Height + Padding.Y; }
            set
            {
                if (value > Padding.Y)
                    value -= Padding.Y;

                slide.BarHeight = value;

                if (!Vertical)
                    slide.SliderHeight = value;
            }
        }

        /// <summary>
        /// Minimum allowable value.
        /// </summary>
        public float Min
        {
            get { return slide.Min; }
            set { slide.Min = value; }
        }

        /// <summary>
        /// Maximum allowable value.
        /// </summary>
        public float Max
        {
            get { return slide.Max; }
            set { slide.Max = value; }
        }

        /// <summary>
        /// Currently set value. Clamped between min and max.
        /// </summary>
        public float Current { get { return slide.Current; } set { slide.Current = value; } }

        /// <summary>
        /// Current value expressed as a percentage over the range between the min and max values.
        /// </summary>
        public float Percent { get { return slide.Percent; } set { slide.Percent = value; } }

        /// <summary>
        /// Determines whether or not the scrollbar will be oriented vertically.
        /// </summary>
        public bool Vertical { get { return slide.Vertical; } set { slide.Vertical = value; slide.Reverse = value; } }

        /// <summary>
        /// Indicates whether or not the hud element is currently moused over
        /// </summary>
        public override bool IsMousedOver => slide.IsMousedOver;

        public IMouseInput MouseInput => slide.MouseInput;

        public readonly SliderBar slide;

        public ScrollBar(HudParentBase parent = null) : base(parent)
        {
            slide = new SliderBar(this) 
            { 
                Reverse = true, 
                Vertical = true,
                SliderWidth = 12f,
                BarWidth = 12f,

                SliderColor = new Color(103, 109, 124),
                SliderHighlight = new Color(137, 140, 149),

                BarColor = new Color(41, 51, 61),
            };

            Padding = new Vector2(18f, 18f);
            Size = new Vector2(317f, 47f);
        }

        protected override void Layout()
        {
            if (Vertical)
            {
                slide.SliderVisible = slide.SliderHeight < slide.BarHeight;
            }
            else
            {
                slide.SliderVisible = slide.SliderWidth < slide.BarWidth;
            }
        }
    }    
}