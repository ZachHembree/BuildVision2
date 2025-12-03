using RichHudFramework.UI.Rendering;
using System;
using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// A text box with a text field on the left and another on the right.
    /// </summary>
    public class DoubleLabelBox : LabelBoxBase
    {
        /// <summary>
        /// Size of the text element sans padding.
        /// </summary>
        public override Vector2 TextSize
        {
            get { return new Vector2(left.Width + right.Width, Math.Max(left.Height, right.Height)); }
            set
            {
                left.Width = value.X * .5f;
                right.Width = value.X * .5f;

                left.Height = value.Y;
                right.Height = value.Y;
            }
        }

		/// <summary>
		/// Gets or sets the padding applied to the text, offsetting it from the edges of the background.
		/// </summary>
		public override Vector2 TextPadding { get { return left.Padding; } set { left.Padding = value; right.Padding = value; } }

		/// <summary>
		/// If true, the element's size is driven by the size of the text content. 
		/// If false, the element's size is set manually, and text may be clipped if the bounds are too small.
		/// </summary>
		public override bool AutoResize { get { return left.AutoResize; } set { left.AutoResize = value; right.AutoResize = value; } }

		/// <summary>
		/// Gets or sets the text composition mode, which controls how text is arranged 
		/// (e.g., single line, wrapped, etc.).
		/// </summary>
		public TextBuilderModes BuilderMode { get { return left.BuilderMode; } set { left.BuilderMode = value; right.BuilderMode = value; } }

        /// <summary>
        /// Text rendered by the left label.
        /// </summary>
        public RichText LeftText { get { return left.TextBoard.GetText(); } set { left.TextBoard.SetText(value); } }

        /// <summary>
        /// Text rendered by the right label.
        /// </summary>
        public RichText RightText { get { return right.TextBoard.GetText(); } set { right.TextBoard.SetText(value); } }

        /// <summary>
        /// Text builder for the left text element
        /// </summary>
        public ITextBuilder LeftTextBuilder => left.TextBoard;

        /// <summary>
        /// Text builder for the right text element
        /// </summary>
        public ITextBuilder RightTextBuilder => right.TextBoard;

        /// <summary>
        /// Left and right aligned text elements
        /// </summary>
        /// <exclude/>
        protected readonly Label left, right;

        public DoubleLabelBox(HudParentBase parent = null) : base(parent)
        {
            left = new Label(this) { ParentAlignment = ParentAlignments.PaddedInnerLeft };
            right = new Label(this) { ParentAlignment = ParentAlignments.PaddedInnerRight };
        }

        public DoubleLabelBox() : this(null)
        { }

        /// <summary>
        /// Updates text layout
        /// </summary>
        /// <exclude/>
        protected override void Layout()
        {
            if (!AutoResize)
            {
                float xPadding = left.Padding.X,
                    leftWidthMin = left.TextBoard.TextSize.X + xPadding,
                    rightWidthMin = right.TextBoard.TextSize.X + xPadding,
                    fullWidth = left.Width + right.Width;

                if (leftWidthMin + rightWidthMin < fullWidth)
                {
                    float newLeft = fullWidth - rightWidthMin;
                    left.Width = newLeft;
                    right.Width = fullWidth - newLeft;
                }
                else
                {
                    left.Width = fullWidth * .5f;
                    right.Width = fullWidth * .5f;
                }
            }
        }
    }
}
