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
        public HudChain<T> Chain { get; }
        public ReadOnlyCollection<T> ChainElements => Chain.ChainElements;

        public override float Width
        {
            get { return Chain.Width + divider.Width + scrollBar.Width; }
            set { Chain.Width = value - divider.Width - scrollBar.Width; }
        }
        public override float Height
        {
            get { return Chain.Height; }
            set
            {
                Chain.Height = value;
                maxHeight = value;
                UpdateVisibleRange();
            }
        }
        public override Vector2 Padding
        {
            get { return Chain.Padding; }
            set
            {
                Chain.Padding = value;
                //scrollBar.Padding = value;
            }
        }

        public bool AutoResize { get { return Chain.AutoResize; } set { Chain.AutoResize = value; } }
        public float Spacing { get { return Chain.Spacing; } set { Chain.Spacing = value; } }

        public int VisStart
        {
            get { return start; }
            set { scrollBar.Current = value; }
        }
        public int VisEnd => end;
        public Color Color { get { return background.Color; } set { background.Color = value; } }

        public readonly ScrollBar scrollBar;
        public readonly TexturedBox background;
        private readonly TexturedBox divider;
        private int start, end, enabledStart, visCount, enabledCount;
        private float maxHeight;

        public ScrollBox(IHudParent parent = null) : base(parent)
        {
            background = new TexturedBox(this)
            { Color = new Color(41, 54, 62), MatchParentSize = true };

            Chain = new HudChain<T>(this)
            {
                AlignVertical = true,
                ParentAlignment = ParentAlignment.Left | ParentAlignment.InnerH
            };

            scrollBar = new ScrollBar(this)
            {
                ParentAlignment = ParentAlignment.Right | ParentAlignment.InnerH,
                //Padding = new Vector2(30f, 8f),
                Width = 45f,
                Min = 0
            };

            divider = new TexturedBox(scrollBar)
            {
                ParentAlignment = ParentAlignment.Left,
                Padding = new Vector2(2f, 2f),
                Color = new Color(53, 66, 75),
                Width = 3f
            };

            start = 0;

            CaptureCursor = true;
            AutoResize = true;
            Padding = new Vector2(16f, 16f);
            Size = new Vector2(355f, 223f);
        }

        public void AddToList(T element)
        {
            Chain.Add(element, true);
            UpdateVisibleRange();
        }

        public void RemoveFromList(T member)
        {
            Chain.Remove(member);
        }

        public virtual void RemoveFromList(Func<T, bool> predicate)
        {
            Chain.Remove(predicate);
        }

        protected override void HandleInput()
        {
            if (IsMousedOver)
            {
                if (SharedBinds.MousewheelUp.IsNewPressed)
                {
                    scrollBar.Current = start - 1;
                }

                if (SharedBinds.MousewheelDown.IsNewPressed)
                {
                    scrollBar.Current = start + 1;
                }
            }
        }

        protected override void Draw()
        {
            int newIndex = (int)scrollBar.Current.Round();
            scrollBar.Max = GetScrollMax();

            if (start != newIndex)
            {
                start = Utils.Math.Clamp(newIndex, 0, ChainElements.Count - 1);
                UpdateVisibleRange();
            }

            scrollBar.Height = Height;
            divider.Height = Height;
        }

        private void UpdateVisibleRange()
        {
            float height = 0f, end = maxHeight - Padding.Y;

            for (int n = 0; n < ChainElements.Count; n++)
                ChainElements[n].Visible = false;

            for (int n = start; n < ChainElements.Count; n++)
            {
                if (ChainElements[n].Enabled)
                {
                    height += ChainElements[n].Height;

                    if (height <= end)
                    {
                        this.end = n;
                        ChainElements[n].Visible = true;
                    }
                    else
                        break;
                }
            }

            if (this.end - start > 1)
            {
                scrollBar.slide.button.Height = ((this.end - start) / (float)(ChainElements.Count - 1)) * scrollBar.Height;
                scrollBar.slide.button.Visible = scrollBar.slide.button.Height < scrollBar.Height;
            }
            else
                scrollBar.slide.button.Visible = false;
        }

        private int GetEnabledCount()
        {
            int enabled = 0;

            for (int n = 0; n < ChainElements.Count; n++)
            {
                if (ChainElements[n].Enabled)
                    enabled++;
            }

            return enabled;
        }

        private int GetVisibleCount()
        {
            int visible = 0;

            for (int n = start; n <= VisEnd; n++)
            {
                if (ChainElements[n].Enabled)
                    visible++;
            }

            return visible;
        }

        private int GetScrollMax()
        {
            int max = 0;
            float height = maxHeight - Padding.Y;

            for (int n = ChainElements.Count - 1; n >= 0; n--)
            {
                if (ChainElements[n].Enabled)
                {
                    height -= ChainElements[n].Height;

                    if (height > 0f)
                        max = n;
                    else
                        break;
                }
            }

            return max;
        }
    }
}