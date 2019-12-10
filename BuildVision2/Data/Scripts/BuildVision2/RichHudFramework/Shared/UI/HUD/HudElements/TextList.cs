using System.Collections.Generic;

namespace DarkHelmet.UI
{
    public class LabelList : HudElementBase, IIndexedCollection<IHudElement>
    {
        public IHudElement this[int index] => list.ChainElements[index];
        public int Count => (listText != null) ? listText.Count : 0;
        public override float Width { get { return list.Width; } set { list.Width = value; } }
        public override float Height { get { return list.Height; } set { list.Height = value; } }
        public bool AutoResize
        {
            get { return autoResize; }
            set
            {
                for (int n = 0; n < list.ChainElements.Count; n++)
                    list.ChainElements[n].AutoResize = value;

                autoResize = value;
            }
        }

        private bool autoResize;
        private readonly IList<RichText> listText;
        private readonly HudChain<Label> list;

        public LabelList(int capacity, IHudParent parent = null) : base(parent)
        {
            list = new HudChain<Label>(this);
            listText = new List<RichText>(capacity);
        }

        public void Add(RichText line)
        {
            listText.Add(line);

            if (list.ChainElements.Count < listText.Count)
                list.Add(new Label(this));

            list.ChainElements[listText.Count - 1].Text.SetText(listText[listText.Count - 1]);
        }

        public void Clear() =>
            listText.Clear();

        protected override void Draw()
        {
            if (Count > 0)
            {
                for (int n = 0; n < Count; n++)
                    list.ChainElements[n].Visible = true;

                for (int n = Count; n < list.ChainElements.Count; n++)
                    list.ChainElements[n].Visible = false;
            }
        }
    }
}
