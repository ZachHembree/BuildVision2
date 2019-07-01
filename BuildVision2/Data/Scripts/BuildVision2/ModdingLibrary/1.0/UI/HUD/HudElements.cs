using System;
using System.Collections.Generic;
using VRageMath;
using HudElementBase = DarkHelmet.UI.HudUtilities.HudElementBase;
using TextBoxBase = DarkHelmet.UI.HudUtilities.TextBoxBase;
using TextHudMessage = DarkHelmet.UI.HudUtilities.TextHudMessage;
using TexturedBox = DarkHelmet.UI.HudUtilities.TexturedBox;

namespace DarkHelmet.UI
{
    public class ListBox : TextBoxBase
    {
        public readonly TexturedBox background;
        public readonly TextList list;

        public override int Width { get { return background.Width; } set { background.Width = Math.Abs(value); } }
        public override int Height { get { return background.Height; } set { background.Height = Math.Abs(value); } }
        public override Vector2I TextSize { get { return list.Size; } }
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

        public ListBox(HudElementBase parent, int maxListLength, Vector2I padding = default(Vector2I), OffsetAlignment offsetAlignment = OffsetAlignment.Center,
            TextAlignment alignment = TextAlignment.Center, Color bgColor = default(Color), bool ignoreParentScale = false) : base(parent, padding, offsetAlignment, ignoreParentScale)
        {
            background = new TexturedBox(this, OffsetAlignment.Center, bgColor, ignoreParentScale: true);
            list = new TextList(this, maxListLength, OffsetAlignment.Center, alignment) { Scale = TextScale };
        }
    }

    public class TextList : HudElementBase
    {
        public readonly List<TextHudMessage> list;
        private string[] listText;
        private TextAlignment alignment;

        public string[] ListText
        {
            get { return listText; }
            set
            {
                listText = value;

                while (list.Count < listText.Length)
                    list.Add(new TextHudMessage(this, Alignment, ignoreParentScale: true));

                for (int n = 0; n < listText.Length; n++)
                    list[n].Text = listText[n];
            }
        }
        public TextAlignment Alignment
        {
            get { return alignment; }
            set
            {
                for (int n = 0; n < list.Count; n++)
                    list[n].alignment = value;

                alignment = value;
            }
        }
        public int Count => (listText != null) ? listText.Length : 0;
        public TextHudMessage this[int index] => list[index];

        public TextList(HudElementBase parent, int maxListLength, OffsetAlignment offsetAlignment = OffsetAlignment.Center, TextAlignment alignment = TextAlignment.Left, 
            bool ignoreParentScale = false) : base(parent, offsetAlignment, ignoreParentScale)
        {
            list = new List<TextHudMessage>(maxListLength);
            Alignment = alignment;
        }

        public void UpdateSize()
        {
            Vector2I listSize, lineSize;
            int maxLineWidth = 0;
            listSize = Vector2I.Zero;

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

                Vector2I textOffset = Size / 2, pos;
                int textCenter = 0;

                if (alignment == TextAlignment.Left)
                    textCenter = -textOffset.X;
                else if (alignment == TextAlignment.Right)
                    textCenter = textOffset.X;

                pos = new Vector2I(textCenter, textOffset.Y - list[0].Size.Y / 2);

                for (int n = 0; n < Count; n++)
                {
                    list[n].Scale = Scale;
                    list[n].Visible = true;
                    list[n].Offset = pos;
                    pos.Y -= list[n].Size.Y;
                }

                for (int n = Count; n < list.Count; n++)
                    list[n].Visible = false;
            }
        }
    }

    public class DoubleTextBox : TextBoxBase
    {
        public readonly TexturedBox background;
        public readonly TextHudMessage left, right;

        public override int Width { get { return background.Width; } set { background.Width = Math.Abs(value); } }
        public override int Height { get { return background.Height; } set { background.Height = Math.Abs(value); } }
        public override Vector2I TextSize { get { return new Vector2I((left.Size.X + right.Size.X + Padding.X), Math.Max(left.Size.Y, right.Size.Y)); } }
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

        public DoubleTextBox(HudElementBase parent, Vector2I padding = default(Vector2I), OffsetAlignment offsetAlignment = OffsetAlignment.Center, Color bgColor = default(Color), 
            bool ignoreParentScale = false) : base(parent, padding, offsetAlignment, ignoreParentScale)
        {
            background = new TexturedBox(this, OffsetAlignment.Center, bgColor, ignoreParentScale: true);
            left = new TextHudMessage(this, TextAlignment.Left, OffsetAlignment.Left) { Scale = TextScale };
            right = new TextHudMessage(this, TextAlignment.Right, OffsetAlignment.Right) { Scale = TextScale };
        }

        protected override void Draw()
        {
            left.Offset = new Vector2I((left.Size.X + Padding.X) / 2, 0);
            right.Offset = new Vector2I((-right.Size.X - Padding.X) / 2, 0);
        }
    }

    public class TextBox : TextBoxBase
    {
        public readonly TexturedBox background;
        public readonly TextHudMessage message;

        public override int Width { get { return background.Width; } set { background.Width = Math.Abs(value); } }
        public override int Height { get { return background.Height; } set { background.Height = Math.Abs(value); } }
        public override Vector2I TextSize { get { return message.Size; } }
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

        public TextBox(HudElementBase parent, Vector2I padding = default(Vector2I), OffsetAlignment offsetAlignment = OffsetAlignment.Center, TextAlignment alignment = TextAlignment.Center, 
            Color bgColor = default(Color), bool ignoreParentScale = false) : base(parent, padding, offsetAlignment, ignoreParentScale)
        {
            background = new TexturedBox(this, OffsetAlignment.Center, bgColor, ignoreParentScale: true);
            message = new TextHudMessage(this, alignment) { Scale = TextScale };
        }
    }
}
