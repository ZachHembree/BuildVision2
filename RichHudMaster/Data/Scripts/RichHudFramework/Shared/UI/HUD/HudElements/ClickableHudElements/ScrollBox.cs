using VRageMath;
using System;

namespace DarkHelmet.UI
{
    public interface IScrollBoxMember : IHudElement
    {
        bool Enabled { get; }
    }

    public class ScrollBox<T> : PaddedElementBase where T : IScrollBoxMember
    {
        public HudChain<T> ChainContainer => chain;
        public ReadOnlyCollection<T> ChainElements => chain.ChainElements;

        public override float Width
        {
            get { return chain.Width + scrollBar.Width; }
            set { chain.Width = value - scrollBar.Width; }
        }
        public override float Height
        {
            get { return chain.Height; }
            set
            {
                chain.Height = value;
                UpdateVisibleRange();
            }
        }
        public override Vector2 Padding
        {
            get { return chain.Padding; }
            set
            {
                chain.Padding = value;
                scrollBar.Padding = value;
            }
        }

        public bool AutoResize { get { return chain.AutoResize; } set { chain.AutoResize = value; } }
        public float Spacing { get { return chain.Spacing; } set { chain.Spacing = value; } }

        public int VisStart
        {
            get { return visStart; }
            set { scrollBar.Current = value; }
        }
        public int VisEnd => visEnd;
        public Color Color { get { return background.Color; } set { background.Color = value; } }

        public readonly ScrollBar scrollBar;
        public readonly TexturedBox background;

        private readonly HudChain<T> chain;
        private readonly TexturedBox divider;
        private int visStart, visEnd;

        public ScrollBox(IHudParent parent = null) : base(parent)
        {
            background = new TexturedBox(this)
            { Color = new Color(41, 54, 62), MatchParentSize = true };

            chain = new HudChain<T>(this)
            {
                AlignVertical = true,
                ParentAlignment = ParentAlignment.Left | ParentAlignment.InnerH
            };

            scrollBar = new ScrollBar(chain)
            {
                ParentAlignment = ParentAlignment.Right,
                Padding = new Vector2(30f, 8f),
                Width = 45f,
                Min = 0
            };

            divider = new TexturedBox(scrollBar)
            {
                ParentAlignment = ParentAlignment.Left | ParentAlignment.InnerH,
                Padding = new Vector2(0f, 2f),
                Color = new Color(53, 66, 75),
                Width = 1f
            };

            visStart = 0;

            CaptureCursor = true;
            AutoResize = true;
            Padding = new Vector2(16f, 16f);
            Size = new Vector2(355f, 223f);
        }

        public void AddToList(T element)
        {
            chain.Add(element, true);

            scrollBar.Max = GetScrollMax();
            UpdateVisibleRange();
        }

        public void RemoveFromList(T member)
        {
            chain.Remove(member);
            scrollBar.Max = GetScrollMax();
        }

        public virtual void RemoveFromList(Func<T, bool> predicate)
        {
            chain.Remove(predicate);
        }

        protected override void HandleInput()
        {
            if (IsMousedOver)
            {
                if (SharedBinds.MousewheelUp.IsNewPressed)
                {
                    scrollBar.Current = visStart - 1;
                }

                if (SharedBinds.MousewheelDown.IsNewPressed)
                {
                    scrollBar.Current = visStart + 1;
                }
            }
        }

        protected override void Draw()
        {
            int newIndex = (int)scrollBar.Current.Round();

            if (visStart != newIndex)
            {
                visStart = Utils.Math.Clamp(newIndex, 0, ChainElements.Count - 1);
                scrollBar.Max = GetScrollMax();
                UpdateVisibleRange();
            }

            scrollBar.Height = Height;
            divider.Height = Height;
        }

        private void UpdateVisibleRange()
        {
            float height = 0f, end = Height - Padding.Y;

            for (int n = 0; n < ChainElements.Count; n++)
                ChainElements[n].Visible = false;

            for (int n = visStart; n < ChainElements.Count; n++)
            {
                if (ChainElements[n].Enabled)
                {
                    height += ChainElements[n].Height;

                    if (height <= end)
                    {
                        visEnd = n;
                        ChainElements[n].Visible = true;
                    }
                    else
                        break;
                }
            }

            if (visEnd - visStart > 1)
            {
                scrollBar.slide.button.Height = ((visEnd - visStart) / (float)(ChainElements.Count - 1)) * scrollBar.Height;
                scrollBar.slide.button.Visible = scrollBar.slide.button.Height < scrollBar.Height;
            }
            else
                scrollBar.slide.button.Visible = false;
        }

        private int GetScrollMax()
        {
            int max = 0;
            float height = 0f, end = Height - Padding.Y;

            for (int n = ChainElements.Count - 1; n >= 0; n--)
            {
                if (ChainElements[n].Enabled)
                {
                    height += ChainElements[n].Height;

                    if (height <= end)
                        max = n;
                    else
                        break;
                }
            }

            return max;
        }
    }
}