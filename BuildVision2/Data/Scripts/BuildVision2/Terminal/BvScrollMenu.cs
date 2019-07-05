using DarkHelmet.UI;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Scrollable list menu; the selection box position is based on the selection index.
    /// </summary>
    public class BvScrollMenu : HudUtilities.ElementBase
    {
        public readonly TextBox header;
        public readonly ListBox list;
        public readonly DoubleTextBox footer;
        public readonly TexturedBox selectionBox, tab;
        private readonly TextBoxChain chain;
        private int selectionIndex = 0;
        private double textScale;

        public override Vector2D UnscaledSize => chain.UnscaledSize;
        public double TextScale
        {
            get => textScale;
            set
            {
                textScale = value;

                if (footer != null)
                {
                    list.TextScale = value;
                    header.TextScale = value * 1.1;
                    footer.TextScale = value;
                }
            }
        }
        public int SelectionIndex
        {
            get { return selectionIndex; }
            set { selectionIndex = Utils.Math.Clamp(value, 0, (list.ListText != null ? list.Count - 1 : 0)); }
        }

        public BvScrollMenu(int maxListLength)
        {
            textScale = 1d;

            header = new TextBox()
            { Padding = new Vector2D(48d, 14d) };

            list = new ListBox(maxListLength)
            { Padding = new Vector2D(48d, 16d), TextAlignment = TextAlignment.Left };

            footer = new DoubleTextBox()
            { Padding = new Vector2D(48d, 8d) };

            chain = new TextBoxChain(new List<TextBoxBase>() { header, list, footer })
            { parent = this };

            selectionBox = new TexturedBox()
            { parent = list };

            tab = new TexturedBox()
            { parent = selectionBox, Width = 3d, Offset = new Vector2D(2d, 0d), color = new Color(225, 225, 240, 255), parentAlignment = ParentAlignment.Left };
        }

        protected override void Draw()
        {
            if (list.Count > 0)
            {
                selectionBox.SetSize(new Vector2D(Size.X - (32d * selectionBox.Scale), list[SelectionIndex].Size.Y + (2d * selectionBox.Scale)));
                selectionBox.Offset = new Vector2D(0d, list[SelectionIndex].Offset.Y);
                tab.Height = selectionBox.Height;
            }
        }
    }
}