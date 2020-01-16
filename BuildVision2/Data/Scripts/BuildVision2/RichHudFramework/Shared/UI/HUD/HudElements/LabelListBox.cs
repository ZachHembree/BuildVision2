using VRageMath;

namespace RichHudFramework.UI
{
    public class LabelListBox : LabelBoxBase, IIndexedCollection<IHudElement>
    {
        public int Count => text.Count;
        public IHudElement this[int index] => text[index];

        public override Vector2 TextSize { get { return text.Size; } protected set { text.Size = value; } }
        public override Vector2 TextPadding { get; set; }

        public override bool AutoResize { get { return text.AutoResize; } set { text.AutoResize = value; } }

        private readonly LabelList text;

        public LabelListBox(int maxListLength, IHudParent parent = null) : base(parent)
        {
            text = new LabelList(maxListLength, this);
        }

        public void Add(RichText line) =>
            text.Add(line);

        public void Clear() =>
            text.Clear();
    }
}
