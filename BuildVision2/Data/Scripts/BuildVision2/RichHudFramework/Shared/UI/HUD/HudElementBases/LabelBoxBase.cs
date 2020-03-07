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
            public abstract Vector2 TextSize { get; protected set; }

            /// <summary>
            /// Padding applied to the text element.
            /// </summary>
            public abstract Vector2 TextPadding { get; set; }

            /// <summary>
            /// Determines whether or not the text box can be resized manually.
            /// </summary>
            public abstract bool AutoResize { get; set; }

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
                get { return TextSize.X + Padding.X; }
                set
                {
                    if (value > Padding.X)
                        value -= Padding.X;

                    TextSize = new Vector2(value, TextSize.Y);
                }
            }

            public override float Height
            {
                get { return TextSize.Y + Padding.Y; }
                set
                {
                    if (value > Padding.Y)
                        value -= Padding.Y;

                    TextSize = new Vector2(TextSize.X, value);
                }
            }

            public LabelBoxBase(IHudParent parent) : base(parent)
            {
                background = new TexturedBox(this)
                {
                    DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding,
                };
            }
        }
    }
}