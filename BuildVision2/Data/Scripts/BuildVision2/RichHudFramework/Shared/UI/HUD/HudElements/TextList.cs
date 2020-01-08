using System.Collections.Generic;

namespace RichHudFramework.UI
{
    public class LabelList : HudElementBase, IIndexedCollection<IHudElement>
    {
        public IHudElement this[int index] => list.List[index];
        public int Count => (listText != null) ? listText.Count : 0;
        public override float Width { get { return list.Width; } set { list.Width = value; } }
        public override float Height { get { return list.Height; } set { list.Height = value; } }
        public bool AutoResize
        {
            get { return autoResize; }
            set
            {
                for (int n = 0; n < list.List.Count; n++)
                    list.List[n].AutoResize = value;

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

            if (list.List.Count < listText.Count)
                list.RegisterChild(new Label(this));

            list.List[listText.Count - 1].TextBoard.SetText(listText[listText.Count - 1]);
        }

        public void Clear() =>
            listText.Clear();

        protected override void Draw()
        {
            if (Count > 0)
            {
                for (int n = 0; n < Count; n++)
                    list.List[n].Visible = true;

                for (int n = Count; n < list.List.Count; n++)
                    list.List[n].Visible = false;
            }
        }
    }
}
