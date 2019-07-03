using System;
using System.Collections.Generic;
using VRageMath;
using ElementBase = DarkHelmet.UI.HudUtilities.ElementBase;

namespace DarkHelmet.UI
{
    /// <summary>
    /// A text box with a list of text fields instead of one.
    /// </summary>
    public class ListBox : TextBoxBase
    {
        public Color BgColor { get { return background.color; } set { background.color = value; } }
        public override double UnscaledWidth { get { return background.UnscaledWidth; } set { background.UnscaledWidth = value; } }
        public override double UnscaledHeight { get { return background.UnscaledHeight; } set { background.UnscaledHeight = value; } }
        public TextAlignment TextAlignment
        {
            get => list.TextAlignment;
            set
            {
                if (value == TextAlignment.Left)
                    list.parentAlignment = ParentAlignment.Left;
                else if (value == TextAlignment.Right)
                    list.parentAlignment = ParentAlignment.Right;
                else
                    list.parentAlignment = ParentAlignment.Center;

                list.TextAlignment = value;
            }
        }
        public override Vector2D TextSize => list.Size;
        public override double TextScale
        {
            get => base.TextScale;
            set
            {
                base.TextScale = value;

                if (list != null)
                    list.Scale = value;
            }
        }
        public string[] ListText { get { return list.ListText; } set { list.ListText = value; } }
        public int Count => list.Count;
        public TextHudMessage this[int index] => list[index];

        private readonly TexturedBox background;
        public readonly TextList list;

        public ListBox(int maxListLength)
        {
            background = new TexturedBox() { parent = this };
            list = new TextList(maxListLength) { parent = this };
        }

        protected override void Draw()
        {
            if (TextAlignment == TextAlignment.Left)
                list.Offset = new Vector2D(list.Size.X + Padding.X / 2, 0);
            else if (TextAlignment == TextAlignment.Right)
                list.Offset = new Vector2D(-list.Size.X - Padding.X / 2, 0);
            else
                list.Offset = Vector2D.Zero;
        }
    }

    /// <summary>
    /// A list of text fields that all share the same alignment and text size.
    /// </summary>
    public class TextList : ElementBase
    {
        public string[] ListText
        {
            get { return listText; }
            set
            {
                listText = value;

                while (list.Count < listText.Length)
                    list.Add(new TextHudMessage() { parent = this, textAlignment = TextAlignment });

                for (int n = 0; n < listText.Length; n++)
                    list[n].Text = listText[n];
            }
        }
        public TextAlignment TextAlignment
        {
            get { return alignment; }
            set
            {
                for (int n = 0; n < list.Count; n++)
                    list[n].textAlignment = value;

                alignment = value;
            }
        }
        public int Count => (listText != null) ? listText.Length : 0;
        public TextHudMessage this[int index] => list[index];

        public readonly List<TextHudMessage> list;
        private string[] listText;
        private TextAlignment alignment;

        public TextList(int maxListLength)
        {
            list = new List<TextHudMessage>(maxListLength);
        }

        public void UpdateSize()
        {
            Vector2D listSize, lineSize;
            double maxLineWidth = 0;
            listSize = Vector2D.Zero;

            for (int n = 0; n < Count; n++)
            {
                lineSize = list[n].Size;
                listSize.Y += lineSize.Y;

                if (lineSize.X > maxLineWidth)
                    maxLineWidth = lineSize.X;
            }

            listSize.X = maxLineWidth;
            Size = listSize;
        }

        protected override void Draw()
        {
            if (Count > 0)
            {
                UpdateSize();

                Vector2D textOffset = Size / 2, pos;
                double textCenter = 0;

                if (alignment == TextAlignment.Left)
                    textCenter = -textOffset.X;
                else if (alignment == TextAlignment.Right)
                    textCenter = textOffset.X;

                pos = new Vector2D(textCenter, textOffset.Y - list[0].Size.Y / 2);

                for (int n = 0; n < Count; n++)
                {
                    list[n].Visible = true;
                    list[n].Offset = pos;
                    pos.Y -= list[n].Size.Y;
                }

                for (int n = Count; n < list.Count; n++)
                    list[n].Visible = false;
            }
        }
    }

    /// <summary>
    /// A text box with a text field on the left and another on the right.
    /// </summary>
    public class DoubleTextBox : TextBoxBase
    {
        public Color BgColor { get { return background.color; } set { background.color = value; } }
        public override double UnscaledWidth { get { return background.UnscaledWidth; } set { background.UnscaledWidth = value; } }
        public override double UnscaledHeight { get { return background.UnscaledHeight; } set { background.UnscaledHeight = value; } }
        public override Vector2D TextSize { get { return new Vector2D((left.Size.X + right.Size.X + Padding.X), Math.Max(left.Size.Y, right.Size.Y)); } }
        public override double TextScale
        {
            get => base.TextScale;
            set
            {
                base.TextScale = value;

                if (right != null)
                {
                    left.Scale = value;
                    right.Scale = value;
                }
            }
        }
        public string LeftText { get { return left.Text; } set { left.Text = value; } }
        public string RightText { get { return right.Text; } set { right.Text = value; } }

        private readonly TexturedBox background;
        private readonly TextHudMessage left, right;

        public DoubleTextBox()
        {
            background = new TexturedBox() { parent = this };
            left = new TextHudMessage() { parent = this, textAlignment = TextAlignment.Left, parentAlignment = ParentAlignment.Left };
            right = new TextHudMessage() { parent = this, textAlignment = TextAlignment.Right, parentAlignment = ParentAlignment.Right };
        }

        protected override void Draw()
        {
            left.Offset = new Vector2D((left.Size.X + Padding.X) / 2d, 0);
            right.Offset = new Vector2D((-right.Size.X - Padding.X) / 2d, 0);
        }
    }

    /// <summary>
    /// A box with a text field and a colored background.
    /// </summary>
    public class TextBox : TextBoxBase
    {
        public Color BgColor { get { return background.color; } set { background.color = value; } }
        public override double UnscaledWidth { get { return background.UnscaledWidth; } set { background.UnscaledWidth = value; } }
        public override double UnscaledHeight { get { return background.UnscaledHeight; } set { background.UnscaledHeight = value; } }
        public TextAlignment TextAlignment { get { return message.textAlignment; } set { message.textAlignment = value; } }
        public override Vector2D TextSize => message.Size;
        public override double TextScale
        {
            get => base.TextScale;
            set
            {
                base.TextScale = value;

                if (message != null)
                    message.Scale = value;
            }
        }
        public string Text { get { return message.Text; } set { message.Text = value; } }

        private readonly TexturedBox background;
        private readonly TextHudMessage message;

        public TextBox()
        {
            background = new TexturedBox() { parent = this };
            message = new TextHudMessage() { parent = this };
        }
    }
}
