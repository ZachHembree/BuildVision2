using DarkHelmet.UI;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    /// <summary>
    /// Scrollable list menu; the selection box position is based on the selection index.
    /// </summary>
    public class BvScrollMenu : HudElementBase
    {
        public override Vector2 Size => chain.Size;
        public float TextScale
        {
            get { return textScale; }
            set
            {
                textScale = value;
                list.TextScale = value;
                header.TextScale = value * 1.1f;
                footer.TextScale = value;
            }
        }
        public int SelectionIndex
        {
            get { return selectionIndex; }
            set { selectionIndex = Utils.Math.Clamp(value, 0, list.Count); }
        }

        public readonly TextBox header;
        public readonly ListBox list;
        public readonly DoubleTextBox footer;
        public readonly TexturedBox selectionBox, tab;
        private readonly BoxChain chain;
        private int selectionIndex = 0;
        private float textScale;

        public BvScrollMenu(int maxListLength) : base(HudMain.Root)
        {
            textScale = 1f;

            header = new TextBox()
            { Padding = new Vector2(48f, 14f) };

            list = new ListBox(maxListLength)
            { Padding = new Vector2(48f, 16f), TextAlignment = TextAlignment.Left };

            footer = new DoubleTextBox()
            { Padding = new Vector2(48f, 8f) };

            chain = new BoxChain(new List<ResizableElementBase>() { header, list, footer }, this);

            selectionBox = new TexturedBox((HudNodeBase)list.Background);

            tab = new TexturedBox(selectionBox)
            { Width = 3f, Offset = new Vector2(2f, 0f), Color = new Color(225, 225, 240, 255), ParentAlignment = ParentAlignment.Left };
        }

        protected override void Draw()
        {
            if (SelectionIndex < list.Count)
            {
                selectionBox.SetSize(new Vector2(Size.X - (32f * selectionBox.Scale), list[SelectionIndex].Size.Y + (2f * selectionBox.Scale)));
                selectionBox.Offset = new Vector2(0f, list[SelectionIndex].Offset.Y - (1f * selectionBox.Scale));
                tab.Height = selectionBox.Height;
            }
        }
    }
}