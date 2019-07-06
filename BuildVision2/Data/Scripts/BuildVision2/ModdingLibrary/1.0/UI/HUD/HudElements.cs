using System;
using System.Collections.Generic;
using VRageMath;
using ElementBase = DarkHelmet.UI.HudUtilities.ElementBase;

namespace DarkHelmet.UI
{
    /// <summary>
    /// Aligns a group of <see cref="TextBoxBase"/>s, either vertically or horizontally, and matches their
    /// width or height along their axis of alignment.
    /// </summary>
    public class TextBoxChain : ElementBase
    {
        public override Vector2D Offset
        {
            get => base.Offset + (alignVertical ? new Vector2D(0d, Size.Y) : new Vector2D(Size.X, 0d));
            set => base.Offset = value;
        }

        public List<TextBoxBase> elements;
        private readonly bool alignVertical;
        private readonly double spacing;

        public TextBoxChain(List<TextBoxBase> elements, bool alignVertical = true, double spacing = 0d)
        {
            this.elements = elements;
            this.alignVertical = alignVertical;
            this.spacing = spacing / Scale;

            for (int n = 0; n < elements.Count; n++)
            {
                elements[n].autoResize = false;

                if (n > 0)
                    elements[n].parent = elements[n - 1];
                else
                    elements[n].parent = this;

                if (alignVertical)
                    elements[n].parentAlignment = ParentAlignment.Bottom;
                else
                    elements[n].parentAlignment = ParentAlignment.Left;
            }
        }

        protected override void Draw()
        {
            if (elements != null && elements.Count > 0)
            {
                Size = GetSize();

                for (int n = 0; n < elements.Count; n++)
                {
                    if (alignVertical)
                    {
                        if (n > 0) elements[n].Offset = new Vector2D(0d, -spacing * Scale);
                        elements[n].Width = Size.X;
                    }
                    else
                    {
                        if (n > 0) elements[n].Offset = new Vector2D(-spacing * Scale, 0d);
                        elements[n].Height = Size.Y;
                    }
                }
            }
        }

        private Vector2D GetSize()
        {
            Vector2D newSize = Vector2D.Zero;

            foreach (TextBoxBase box in elements)
            {
                if (alignVertical)
                {
                    if (box.MinimumSize.X > newSize.X)
                        newSize.X = box.MinimumSize.X;

                    newSize.Y += box.MinimumSize.Y;
                    box.Height = box.MinimumSize.Y;
                }
                else
                {
                    if (box.MinimumSize.Y > newSize.Y)
                        newSize.Y = box.MinimumSize.Y;

                    newSize.X += box.MinimumSize.X;
                    box.Width = box.MinimumSize.X;
                }
            }

            if (alignVertical)
                newSize.Y += (spacing * elements.Count - 1) / 2d;
            else
                newSize.X += (spacing * elements.Count - 1) / 2d;

            return newSize;
        }
    }

    /// <summary>
    /// A text box with a list of text fields instead of one.
    /// </summary>
    public class ListBox : TextBoxBase
    {
        public TextAlignment TextAlignment { get => list.TextAlignment; set =>list.TextAlignment = value; }
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
        public IList<string> ListText { get { return list.ListText; } set { list.ListText = value; } }
        public int Count => list.Count;
        public TextHudMessage this[int index] => list[index];

        public readonly TextList list;

        public ListBox(int maxListLength)
        {
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
        public IList<string> ListText
        {
            get { return listText; }
            set
            {
                listText = value;

                while (list.Count < listText.Count)
                    list.Add(new TextHudMessage() { parent = this, textAlignment = TextAlignment });

                for (int n = 0; n < listText.Count; n++)
                {
                    if (listText[n] != HudUtilities.LineBreak)
                        list[n].Text = listText[n];
                }
            }
        }
        public TextAlignment TextAlignment
        {
            get { return alignment; }
            set
            {
                if (value == TextAlignment.Left)
                    parentAlignment = ParentAlignment.Left;
                else if (value == TextAlignment.Right)
                    parentAlignment = ParentAlignment.Right;
                else
                    parentAlignment = ParentAlignment.Center;

                for (int n = 0; n < list.Count; n++)
                    list[n].textAlignment = value;

                alignment = value;
            }
        }
        public int Count => (listText != null) ? listText.Count : 0;
        public TextHudMessage this[int index] => list[index];

        public readonly List<TextHudMessage> list;
        private IList<string> listText;
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
                if (listText[n] != HudUtilities.LineBreak)
                {
                    lineSize = list[n].Size;
                    listSize.Y += lineSize.Y;

                    if (lineSize.X > maxLineWidth)
                        maxLineWidth = lineSize.X;
                }
                else
                    listSize.Y += HudUtilities.LineSpacing * Scale;
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
                double textCenter = 0d;

                if (alignment == TextAlignment.Left)
                    textCenter = -textOffset.X;
                else if (alignment == TextAlignment.Right)
                    textCenter = textOffset.X;

                pos = new Vector2D(textCenter, textOffset.Y - list[0].Size.Y / 2);

                for (int n = 0; n < Count; n++)
                {
                    if (listText[n] != HudUtilities.LineBreak)
                    {
                        list[n].Visible = true;
                        list[n].Offset = pos;
                        pos.Y -= list[n].Size.Y;
                    }
                    else
                    {
                        list[n].Visible = false;
                        pos.Y -= HudUtilities.LineSpacing * Scale;
                    }
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

        private readonly TextHudMessage left, right;

        public DoubleTextBox()
        {
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

        private readonly TextHudMessage message;

        public TextBox()
        {
            message = new TextHudMessage() { parent = this };
        }
    }
}
