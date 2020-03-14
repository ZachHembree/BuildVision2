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
            get { return new Vector2(left.Size.X + right.Size.X, Math.Max(left.Size.Y, right.Size.Y)); }
            protected set
            {
                left.Width = value.X * .5f;
                right.Width = value.X * .5f;

                left.Height = value.Y;
                right.Height = value.Y;
            }
        }

        /// <summary>
        /// Padding applied to the text element.
        /// </summary>
        public override Vector2 TextPadding { get { return left.Padding; } set { left.Padding = value; right.Padding = value; } }

        /// <summary>
        /// Determines whether or not the text box can be resized manually.
        /// </summary>
        public override bool AutoResize { get { return left.AutoResize; } set { left.AutoResize = value; right.AutoResize = value; } }

        /// <summary>
        /// Line formatting mode used by the label.
        /// </summary>
        public TextBuilderModes BuilderMode { get { return left.BuilderMode; } set { left.BuilderMode = value; right.BuilderMode = value; } }

        /// <summary>
        /// Text rendered by the left label.
        /// </summary>
        public RichText LeftText { get { return LeftTextBoard.GetText(); } set { LeftTextBoard.SetText(value); } }

        /// <summary>
        /// Text rendered by the right label.
        /// </summary>
        public RichText RightText { get { return RightTextBoard.GetText(); } set { RightTextBoard.SetText(value); } }

        public ITextBuilder LeftTextBoard => left.TextBoard;
        public ITextBuilder RightTextBoard => right.TextBoard;

        protected readonly Label left, right;

        public DoubleLabelBox(IHudParent parent = null) : base(parent)
        {
            left = new Label(this) { ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH | ParentAlignments.UsePadding };
            right = new Label(this) { ParentAlignment = ParentAlignments.Right | ParentAlignments.InnerH | ParentAlignments.UsePadding };
        }
    }
}
