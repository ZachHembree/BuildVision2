using DarkHelmet.UI;
using System;
using VRageMath;
using HudElementBase = DarkHelmet.UI.HudUtilities.HudElementBase;
using TextBoxBase = DarkHelmet.UI.HudUtilities.TextBoxBase;
using TexturedBox = DarkHelmet.UI.HudUtilities.TexturedBox;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Scrollable list menu; the selection box position is based on the selection index.
    /// </summary>
    public class BvScrollMenu : TextBoxBase
    {
        public readonly TextBox header;
        public readonly ListBox list;
        public readonly DoubleTextBox footer;
        public readonly TexturedBox selectionBox, tab;
        private int selectionIndex = 0;

        public int SelectionIndex
        {
            get { return selectionIndex; }
            set { selectionIndex = Utils.Math.Clamp(value, 0, (list.ListText != null ? list.Count - 1 : 0)); }
        }
        public override Vector2I TextSize
        {
            get
            {
                Vector2I headerSize = header.MinimumSize, listSize = list.MinimumSize,
                    footerSize = footer.MinimumSize, newSize = Vector2I.Zero;

                newSize.X = Math.Max(headerSize.X, listSize.X);
                newSize.X = Math.Max(newSize.X, footerSize.X);
                newSize.Y = headerSize.Y + listSize.Y + footerSize.Y;

                return newSize;
            }
        }
        public override double TextScale
        {
            get => base.TextScale;
            set
            {
                base.TextScale = value;

                if (footer != null)
                {
                    list.TextScale = value;
                    header.TextScale = value * 1.1;
                    footer.TextScale = value;
                }
            }
        }

        public BvScrollMenu(int maxListLength, HudElementBase parent, Vector2I padding = default(Vector2I), OffsetAlignment offsetAlignment = OffsetAlignment.Center, 
            bool ignoreParentScale = false) : base(parent, padding, offsetAlignment, ignoreParentScale)
        {
            list = new ListBox(this, maxListLength, new Vector2I(48, 16), OffsetAlignment.Center, TextAlignment.Left) { TextScale = TextScale };
            header = new TextBox(list, new Vector2I(48, 14), OffsetAlignment.Top, TextAlignment.Center) { TextScale = TextScale * 1.1 };
            footer = new DoubleTextBox(list, new Vector2I(48, 10), OffsetAlignment.Bottom) { TextScale = TextScale };
            selectionBox = new TexturedBox(list);
            tab = new TexturedBox(selectionBox, OffsetAlignment.Left, new Color(225, 225, 240, 255)) { Offset = new Vector2I(2, 0), Width = 3 };
        }

        protected override void Draw()
        {
            Size = TextSize + Padding;
            header.SetSize(new Vector2I(Width, header.TextSize.Y));
            list.SetSize(new Vector2I(Width, list.TextSize.Y));
            footer.SetSize(new Vector2I(Width, footer.TextSize.Y));

            if (list.Count > 0)
            {
                selectionBox.SetSize(new Vector2I(list.TextSize.X + 16, list[SelectionIndex].Size.Y + 1));
                selectionBox.Offset = new Vector2I(0, list[SelectionIndex].Offset.Y);
                tab.Height = selectionBox.Height;
            }
        }
    }
}