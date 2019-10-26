using DarkHelmet.UI.Rendering;
using DarkHelmet.UI.TextHudApi;
using System;
using System.Collections.Generic;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace DarkHelmet.UI
{
    /// <summary>
    /// Creates a colored box of a given width and height with a given mateiral. The default material is just a plain color.
    /// </summary>
    public class TexturedBox : ResizableElementBase
    {
        public Material Material { get { return hudBoard.Material; } set { hudBoard.Material = value; } }
        public MaterialAlignment MatAlignment { get { return hudBoard.MatAlignment; } set { hudBoard.MatAlignment = value; } }
        public Color Color { get { return hudBoard.Color; } set { hudBoard.Color = value; } }
        public override float Width { set { base.Width = value; hudBoard.Width = Width; } }
        public override float Height { set { base.Height = value; hudBoard.Height = Height; } }
        public override Vector2 Offset { get { return hudBoard.offset; } set { hudBoard.offset = value; } }

        private readonly HudBoard hudBoard;

        public TexturedBox(IHudParent parent = null) : base(parent)
        {
            hudBoard = new HudBoard();
        }

        protected override void AfterDraw()
        {
            if (Color.A > 0)
            {
                hudBoard.Draw(Origin);
            }
        }
    }

    public class RichTextElement : HudElementBase
    {
        public TextBoard Text { get; set; }
        public override Vector2 Size => Text.TextSize;

        public RichTextElement(IHudParent parent = null, bool wordWrapping = false) : base(parent)
        {
            Text = new TextBoard(wordWrapping);
        }

        protected override void Draw()
        {
            Text.Draw(Origin + Offset);
        }

        protected override void ScaleChanged(float change) =>
            Text.Scale = Scale;
    }

    /// <summary>
    /// Wrapper used to make precise pixel-level manipluation of <see cref="HudAPIv2.HUDMessage"/> easier.
    /// </summary>
    public class TextHudMessage : HudElementBase
    {
        public string Text { get { return text; } set { text = value; UpdateMessage(); } }

        public TextAlignment textAlignment;
        private HudAPIv2.HUDMessage hudMessage;
        private Vector2 alignmentOffset;
        private string text;

        public TextHudMessage(IHudParent parent = null) : base(parent)
        {
            textAlignment = TextAlignment.Center;
        }

        protected override void Draw()
        {
            if (HudAPIv2.Heartbeat)
            {
                if (hudMessage == null)
                {
                    hudMessage = new HudAPIv2.HUDMessage
                    {
                        Blend = BlendTypeEnum.PostPP,
                        Scale = Scale * (1080f / HudMain.ScreenHeight),
                        Options = HudAPIv2.Options.Fixed,
                        Visible = false,
                    };

                    UpdateMessage();
                }

                hudMessage.Scale = Scale * (1080f / HudMain.ScreenHeight);
                UpdateTextOffset();

                Vector2 pos = HudMain.GetNativeVector(Origin + Offset + alignmentOffset);

                hudMessage.Origin = new Vector2D(pos.X, pos.Y);
                hudMessage.Draw();
            }
        }

        private void UpdateMessage()
        {
            if (hudMessage != null && Text != null)
            {
                hudMessage.Message.Clear();
                hudMessage.Message.Append(Text);

                Vector2D textLength = hudMessage.GetTextLength();
                Size = HudMain.GetPixelVector(new Vector2((float)textLength.X, (float)textLength.Y));
            }
        }

        private void UpdateTextOffset()
        {
            Vector2 offset = Size / 2f;
            alignmentOffset = offset;
            alignmentOffset.X *= -1;

            if (textAlignment == TextAlignment.Right)
                alignmentOffset.X -= offset.X;
            else if (textAlignment == TextAlignment.Left)
                alignmentOffset.X += offset.X;
        }
    }

    /// <summary>
    /// Aligns a group of <see cref="TextBoxBase"/>s, either vertically or horizontally, and matches their
    /// width/height along their axis of alignment.
    /// </summary>
    public class BoxChain : HudElementBase
    {
        public override Vector2 Offset
        {
            get { return base.Offset + (alignVertical ? new Vector2(0f, Size.Y) : new Vector2(Size.X, 0f)); }
            set { base.Offset = value; }
        }

        private readonly List<ResizableElementBase> elements;
        private readonly bool alignVertical;
        private readonly float spacing;

        public BoxChain(List<ResizableElementBase> elements, IHudNode parent = null, bool alignVertical = true, float spacing = 0f) : base(parent)
        {
            this.elements = elements;
            this.alignVertical = alignVertical;
            this.spacing = spacing / Scale;

            for (int n = 0; n < elements.Count; n++)
            {
                elements[n].autoResize = false;

                if (n > 0)
                    elements[n].Register(elements[n - 1]);
                else
                    elements[n].Register(this);

                if (alignVertical)
                    elements[n].ParentAlignment = ParentAlignment.Bottom;
                else
                    elements[n].ParentAlignment = ParentAlignment.Left;
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
                        if (n > 0) elements[n].Offset = new Vector2(0f, -spacing * Scale);
                        elements[n].Width = Size.X;
                    }
                    else
                    {
                        if (n > 0) elements[n].Offset = new Vector2(-spacing * Scale, 0f);
                        elements[n].Height = Size.Y;
                    }
                }
            }
        }

        private Vector2 GetSize()
        {
            Vector2 newSize = Vector2.Zero;

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
                newSize.Y += (spacing * elements.Count - 1) / 2f;
            else
                newSize.X += (spacing * elements.Count - 1) / 2f;

            return newSize;
        }
    }

    /// <summary>
    /// A box with a text field and a colored background.
    /// </summary>
    public class TextBox : TextBoxBase
    {
        public GlyphFormat Format { get { return textElement.Text.Format; } set { textElement.Text.Format = value; } }
        public override Vector2 TextSize => textElement.Size;
        public override float TextScale { get { return textElement.Scale; } set { textElement.Scale = value; } }
        public TextBoard Text { get { return textElement.Text; } set { textElement.Text = value; } }
        public IReadonlyHudElement TextElement => textElement;

        private readonly RichTextElement textElement;

        public TextBox(IHudParent parent = null) : base(parent)
        {
            textElement = new RichTextElement(this);
        }
    }

    /// <summary>
    /// A text box with a text field on the left and another on the right.
    /// </summary>
    public class DoubleTextBox : TextBoxBase
    {
        public override Vector2 TextSize { get { return new Vector2((left.Size.X + right.Size.X + Padding.X), Math.Max(left.Size.Y, right.Size.Y)); } }
        public override float TextScale { get { return left.Scale; } set { left.Scale = value; right.Scale = value; } }
        public TextBoard LeftText { get { return left.Text; } set { left.Text = value; } }
        public TextBoard RightText { get { return right.Text; } set { right.Text = value; } }
        public IReadonlyHudElement Left => left;
        public IReadonlyHudElement Right => Right;

        private readonly RichTextElement left, right;

        public DoubleTextBox(IHudParent parent = null) : base(parent)
        {
            left = new RichTextElement(this) { ParentAlignment = ParentAlignment.Left };
            right = new RichTextElement(this) { ParentAlignment = ParentAlignment.Right };
        }

        protected override void AfterDraw()
        {
            left.Offset = new Vector2(left.Size.X + (Padding.X / 2f), 0f);
            right.Offset = new Vector2(-right.Size.X - (Padding.X / 2f), 0f);
        }
    }

    /// <summary>
    /// A list of text fields that all share the same alignment and text size.
    /// </summary>
    public class TextList : HudElementBase
    {
        public TextAlignment TextAlignment
        {
            get { return alignment; }
            set
            {
                if (value == TextAlignment.Left)
                    ParentAlignment = ParentAlignment.Left;
                else if (value == TextAlignment.Right)
                    ParentAlignment = ParentAlignment.Right;
                else
                    ParentAlignment = ParentAlignment.Center;

                alignment = value;
            }
        }
        public int Count => (listText != null) ? listText.Count : 0;
        public IReadonlyHudElement this[int index] => list[index];

        private readonly List<RichTextElement> list;
        private readonly IList<RichText> listText;
        private TextAlignment alignment;

        public TextList(int capacity, IHudParent parent = null) : base(parent)
        {
            list = new List<RichTextElement>(capacity);
            listText = new List<RichText>(capacity);
            TextAlignment = TextAlignment.Center;
        }

        public void Add(RichText line)
        {
            listText.Add(line);

            if (list.Count < listText.Count)
                list.Add(new RichTextElement(this));

            list[listText.Count - 1].Text.SetText(listText[listText.Count - 1]);
        }

        public void Clear() =>
            listText.Clear();

        public void UpdateSize()
        {
            Vector2 listSize, lineSize;
            float maxLineWidth = 0;
            listSize = Vector2.Zero;

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
                float offset = 0f;
                Vector2 pos = new Vector2(0f, Size.Y / 2f);
                pos.Y -= list[0].Size.Y / 2f;

                if (alignment == TextAlignment.Left)
                    offset = -Size.X / 2f;
                else if (alignment == TextAlignment.Right)
                    offset = Size.X / 2f;

                for (int n = 0; n < Count; n++)
                {
                    if (alignment == TextAlignment.Left)
                        pos.X = offset + list[n].Size.X / 2f;
                    else if (alignment == TextAlignment.Right)
                        pos.X = offset - list[n].Size.X / 2f;

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
    /// A text box with a list of text fields instead of one.
    /// </summary>
    public class ListBox : TextBoxBase
    {
        public TextAlignment TextAlignment { get { return list.TextAlignment; } set { list.TextAlignment = value; } }
        public override Vector2 TextSize => list.Size;
        public override float TextScale { get { return list.Scale; } set { list.Scale = value; } }
        public int Count => list.Count;
        public IReadonlyHudElement this[int index] => list[index];

        private readonly TextList list;

        public ListBox(int maxListLength, IHudParent parent = null) : base(parent)
        {
            list = new TextList(maxListLength, this);
        }

        public void Add(RichText line) =>
            list.Add(line);

        public void Clear() =>
            list.Clear();

        protected override void AfterDraw()
        {
            if (TextAlignment == TextAlignment.Left)
                list.Offset = new Vector2(list.Size.X + Padding.X / 2f, 0f);
            else if (TextAlignment == TextAlignment.Right)
                list.Offset = new Vector2(-list.Size.X - Padding.X / 2f, 0f);
            else
                list.Offset = Vector2.Zero;
        }
    }
}
