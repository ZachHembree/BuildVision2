using VRageMath;

namespace RichHudFramework.UI
{
    using Server;

    /// <summary>
    /// Horizontal slider designed to mimic the appearance of the slider in the SE terminal.
    /// </summary>
    public class SliderBox : HudElementBase
    {
        /// <summary>
        /// Lower limit.
        /// </summary>
        public float Min { get { return slide.Min; } set { slide.Min = value; } }

        /// <summary>
        /// Upper limit.
        /// </summary>
        public float Max { get { return slide.Max; } set { slide.Max = value; } }

        /// <summary>
        /// Current value. Clamped between min and max.
        /// </summary>
        public float Current { get { return slide.Current; } set { slide.Current = value; } }

        /// <summary>
        /// Current value expressed as a percentage over the range between the min and max values.
        /// </summary>
        public float Percent { get { return slide.Percent; } set { slide.Percent = value; } }

        /// <summary>
        /// Border size. Included in total element size.
        /// </summary>
        public override Vector2 Padding { get { return slide.Padding; } set { slide.Padding = value; } }

        public override bool IsMousedOver => slide.IsMousedOver;

        public readonly TexturedBox background;
        public readonly BorderBox border;
        public readonly SliderBar slide;
        public readonly TexturedBox highlight;

        public SliderBox(HudParentBase parent) : base(parent)
        {
            background = new TexturedBox(this)
            { Color = new Color(41, 54, 62), DimAlignment = DimAlignments.Both };

            border = new BorderBox(background)
            { Color = new Color(53, 66, 75), Thickness = 1f, DimAlignment = DimAlignments.Both, };

            slide = new SliderBar(this) 
            { 
                DimAlignment = DimAlignments.Both,
                SliderSize = new Vector2(14f, 28f),
                BarHeight = 5f,

                SliderColor = new Color(103, 109, 124),
                BarColor = new Color(103, 109, 124),
                SliderHighlight = new Color(214, 213, 218),
                BarHighlight = new Color(181, 185, 190),
            };

            highlight = new TexturedBox(background)
            {
                Color = TerminalFormatting.HighlightOverlayColor,
                DimAlignment = DimAlignments.Both,
                Visible = false
            };

            Padding = new Vector2(18f, 18f);
            Size = new Vector2(317f, 47f);
        }

        public SliderBox() : this(null)
        { }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (IsMousedOver)
            {
                highlight.Visible = true;
            }
            else
            {
                highlight.Visible = false;
            }
        }
    }
}