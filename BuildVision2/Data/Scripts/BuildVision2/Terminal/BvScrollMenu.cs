using DarkHelmet.UI;
using System;
using VRageMath;

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
        public override Vector2D TextSize
        {
            get
            {
                Vector2D headerSize = header.MinimumSize, listSize = list.MinimumSize,
                    footerSize = footer.MinimumSize, newSize = Vector2D.Zero;

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

        public BvScrollMenu(int maxListLength)
        {
            list = new ListBox(maxListLength)
            { parent = this, Padding = new Vector2D(48d, 16d), TextAlignment = TextAlignment.Left, autoResize = false };

            header = new TextBox()
            { parent = list, Padding = new Vector2D(48d, 14d), parentAlignment = ParentAlignment.Top, autoResize = false };

            footer = new DoubleTextBox()
            { parent = list, Padding = new Vector2D(48d, 8d), parentAlignment = ParentAlignment.Bottom, autoResize = false };

            selectionBox = new TexturedBox()
            { parent = list };

            tab = new TexturedBox()
            { parent = selectionBox, Width = 3d, Offset = new Vector2D(2d, 0d), color = new Color(225, 225, 240, 255), parentAlignment = ParentAlignment.Left };
        }

        protected override void Draw()
        {
            Offset = new Vector2D(0d, -(header.Height - footer.Height) / 2d);
            header.SetSize(new Vector2D(Width, header.MinimumSize.Y));
            list.SetSize(new Vector2D(Width, list.MinimumSize.Y));
            footer.SetSize(new Vector2D(Width, footer.MinimumSize.Y));

            if (list.Count > 0)
            {
                selectionBox.SetSize(new Vector2D(Width - (32d * selectionBox.Scale), list[SelectionIndex].Size.Y));
                selectionBox.Offset = new Vector2D(0d, list[SelectionIndex].Offset.Y);
                tab.Height = selectionBox.Height;
            }
        }
    }
}