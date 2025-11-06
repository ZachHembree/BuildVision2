using System;
using VRage;
using VRageMath;

namespace RichHudFramework
{
    namespace UI
    {
        using Client;
        using Server;

        /// <summary>
        /// Base type for elements combining Labels with textured backgrounds.
        /// </summary>
        public abstract class LabelBoxBase : HudElementBase
        {
            /// <summary>
            /// Size of the text element sans padding.
            /// </summary>
            public abstract Vector2 TextSize { get; set; }

            /// <summary>
            /// Padding applied to the text element.
            /// </summary>
            public abstract Vector2 TextPadding { get; set; }

            /// <summary>
            /// Determines whether or not the text box can be resized manually.
            /// </summary>
            public abstract bool AutoResize { get; set; }

            /// <summary>
            /// If true, then the background will resize to match the size of the text plus padding. Otherwise,
            /// size will be clamped such that the element will not be smaller than the text element.
            /// </summary>
            public bool FitToTextElement { get; set; }

            /// <summary>
            /// Background color
            /// </summary>
            public virtual Color Color { get { return background.Color; } set { background.Color = value; } }

            /// <summary>
            /// Label box background
            /// </summary>
            public readonly TexturedBox background;

            public LabelBoxBase(HudParentBase parent) : base(parent)
            {
                background = new TexturedBox(this)
                {
                    DimAlignment = DimAlignments.UnpaddedSize,
                };

                FitToTextElement = true;
                Color = Color.Gray;
            }

            protected override void UpdateSize()
            {
				if (!AutoResize)
				{
					TextSize = UnpaddedSize;
				}
  
				if (FitToTextElement)
				{
					UnpaddedSize = TextSize;
				}
			}
        }
    }
}