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
        /// Base type for hud elements that have text elements and a <see cref="TexturedBox"/> background.
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

            public override float Width
            {
                get { return FitToTextElement ? TextSize.X + Padding.X : base.Width; }
                set
                {
                    if (!FitToTextElement)
                        value = MathHelper.Max(TextSize.X, value);

                    if (value > Padding.X)
                        value -= Padding.X;

                    if (FitToTextElement)
                        TextSize = new Vector2(value, TextSize.Y);
                    else
                        base.Width = value;
                }
            }

            public override float Height
            {
                get { return FitToTextElement ? TextSize.Y + Padding.Y : base.Height; }
                set
                {
                    if (!FitToTextElement)
                        value = MathHelper.Max(TextSize.Y, value);

                    if (value > Padding.Y)
                        value -= Padding.Y;

                    if (FitToTextElement)
                        TextSize = new Vector2(TextSize.X, value);
                    else
                        base.Height = value;
                }
            }

            public LabelBoxBase(HudParentBase parent) : base(parent)
            {
                background = new TexturedBox(this)
                {
                    DimAlignment = DimAlignments.Both,
                };

                FitToTextElement = true;
            }

            protected override void Layout()
            {
                // The element may not be smaller than the text
                if (!FitToTextElement)
                {
                    _absoluteWidth = MathHelper.Max(TextSize.Y, _absoluteWidth * Scale) / Scale;
                    _absoluteHeight = MathHelper.Max(TextSize.X, _absoluteHeight * Scale) / Scale;
                }
            }
        }
    }
}