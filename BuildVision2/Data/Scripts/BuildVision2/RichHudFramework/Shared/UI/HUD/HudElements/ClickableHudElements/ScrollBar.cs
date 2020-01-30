using VRageMath;

namespace RichHudFramework.UI
{
    public class ScrollBar : HudElementBase
    {
        public override float Width
        {
            get { return slide.Width + Padding.X; }
            set
            {
                if (value > Padding.X)
                    value -= Padding.X;

                slide.BarWidth = value;
                slide.SliderWidth = value;
            }
        }

        public override float Height
        {
            get { return slide.Height + Padding.Y; }
            set
            {
                if (value > Padding.Y)
                    value -= Padding.Y;

                slide.BarHeight = value;
                slide.SliderHeight = value;
            }
        }

        public float Min
        {
            get { return slide.Min; }
            set { slide.Min = value; }
        }
        public float Max
        {
            get { return slide.Max; }
            set { slide.Max = value; }
        }

        public float Current { get { return slide.Current; } set { slide.Current = value; } }
        public float Percent { get { return slide.Percent; } set { slide.Percent = value; } }
        public bool Vertical { get { return slide.Vertical; } set { slide.Vertical = value; slide.Reverse = value; } }

        public readonly SliderBar slide;

        public ScrollBar(IHudParent parent = null) : base(parent)
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

        protected override void Draw()
        {
            if (Vertical)
            {
                slide.SliderWidth = slide.BarWidth;
                slide.SliderVisible = slide.SliderHeight < slide.BarHeight;
            }
            else
            {
                slide.BarHeight = slide.SliderHeight;
                slide.SliderVisible = slide.SliderWidth < slide.BarWidth;
            }
        }
    }    
}