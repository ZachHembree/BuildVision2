using VRageMath;
using System;

namespace RichHudFramework.UI
{
    public class ScrollBox<T> : HudElementBase, IListBoxEntry where T : IListBoxEntry
    {
        public HudChain<T> Members { get; }
        public ReadOnlyCollection<T> List => Members.List;

        public override float Width
        {
            set
            {
                base.Width = value;
                maxSize.X = Width - Padding.X;
            }
        }

        public override float Height
        {
            set
            {
                base.Height = value;
                maxSize.Y = Height - Padding.Y;
            }
        }

        /// <summary>
        /// Minimum allowable size of the list.
        /// </summary>
        public Vector2 MinimumSize { get; set; }

        /// <summary>
        /// If set to true, the element will automatically resize to match the size of the chain.
        /// </summary>
        public bool FitToChain { get; set; }

        public bool Enabled { get; set; }

        public bool AlignVertical
        {
            get { return Members.AlignVertical; }
            set
            {
                Members.AlignVertical = value;
                scrollBar.Vertical = value;

                if (AlignVertical)
                {
                    Members.ParentAlignment = ParentAlignments.Top | ParentAlignments.Left | ParentAlignments.Inner;
                    scrollBar.ParentAlignment = ParentAlignments.Right | ParentAlignments.InnerH;
                    divider.ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH;

                    divider.Padding = new Vector2(2f, 0f);
                    divider.Width = 1f;

                    scrollBar.Padding = new Vector2(30f, 8f);
                    scrollBar.Width = 45f;
                }
                else
                {
                    Members.ParentAlignment = ParentAlignments.Top | ParentAlignments.Left | ParentAlignments.Inner;
                    scrollBar.ParentAlignment = ParentAlignments.Bottom | ParentAlignments.InnerV;
                    divider.ParentAlignment = ParentAlignments.Bottom | ParentAlignments.InnerV;

                    divider.Padding = new Vector2(16f, 2f);
                    divider.Height = 1f;

                    scrollBar.Padding = new Vector2(16f);
                    scrollBar.Height = 24f;
                }
            }
        }

        /// <summary>
        /// Distance between the chain elements.
        /// </summary>
        public float Spacing { get { return Members.Spacing; } set { Members.Spacing = value; } }

        /// <summary>
        /// Minimum number of visible elements allowed. Supercedes maximum size. If the number of elements that
        /// can fit within the maximum size is less than this value, then this element will expand beyond its maximum
        /// size.
        /// </summary>
        public int MinimumVisCount { get; set; }

        /// <summary>
        /// Index of the first element in the visible range in the chain.
        /// </summary>
        public int Start
        {
            get { return (int)scrollBar.Current.Round(); }
            set { scrollBar.Current = value; }
        }

        /// <summary>
        /// Index of the last element in the visible range in the chain.
        /// </summary>
        public int End
        {
            get { return end; }
            set
            {
                end = Utils.Math.Clamp(value, 0, List.Count - 1);
                Start = GetMaxStart(end);
            }
        }

        /// <summary>
        /// Position of the first visible element as it appears in the UI
        /// </summary>
        public int VisStart { get; private set; }

        /// <summary>
        /// Number of elements visible starting from the Start index
        /// </summary>
        public int VisCount { get; private set; }

        /// <summary>
        /// Total number of enabled elements
        /// </summary>
        public int EnabledCount { get; private set; }

        /// <summary>
        /// Background color of the scroll box.
        /// </summary>
        public Color Color { get { return background.Color; } set { background.Color = value; } }

        public readonly ScrollBar scrollBar;
        public readonly TexturedBox background;
        public readonly TexturedBox divider;

        private Vector2 maxSize, chainSize;
        private float totalSize;
        private int end;

        public ScrollBox(IHudParent parent = null) : base(parent)
        {
            background = new TexturedBox(this)
            {
                Color = new Color(41, 54, 62),
                DimAlignment = DimAlignments.Both,
            };

            scrollBar = new ScrollBar(this)
            {
                Min = 0,
            };

            divider = new TexturedBox(scrollBar)
            {
                Color = new Color(53, 66, 75),
            };

            Members = new HudChain<T>(this)
            {
                AutoResize = true,
            };

            CaptureCursor = true;
            AlignVertical = true;
            Enabled = true;
            MinimumVisCount = 1;
        }

        public void AddToList(T element) =>
            Members.Add(element);

        public T Find(Func<T, bool> predicate) =>
            Members.Find(x => predicate(x));

        public int FindIndex(Func<T, bool> predicate) =>
            Members.FindIndex(x => predicate(x));

        public void RemoveFromList(T member) =>
            Members.Remove(member);

        public void RemoveFromList(Func<T, bool> predicate) =>
            Members.Remove(predicate);

        protected override void HandleInput()
        {
            if (scrollBar.IsMousedOver)
                ShareCursor = false;
            else
                ShareCursor = true;

            if (IsMousedOver)
            {
                if (SharedBinds.MousewheelUp.IsPressed)
                {
                    ShareCursor = scrollBar.Current == scrollBar.Min;
                    scrollBar.Current = Start - 1;
                }
                else if (SharedBinds.MousewheelDown.IsPressed)
                {
                    ShareCursor = scrollBar.Current == scrollBar.Max;
                    scrollBar.Current = Start + 1;
                }
                else
                    ShareCursor = true;
            }
            else
                ShareCursor = true;
        }

        protected override void Draw()
        {
            scrollBar.Max = GetMaxStart(List.Count - 1);
            scrollBar.Min = GetFirstEnabled();
            EnabledCount = GetEnabledCount();

            UpdateVisible();

            Vector2 minSize = chainSize + Padding;

            Members.Size = chainSize;
            Members.Offset = new Vector2(Padding.X, -Padding.Y) * .5f;

            if (AlignVertical)
                minSize.X += scrollBar.Width;
            else
                minSize.Y += scrollBar.Height;

            if (FitToChain)
            {
                Size = Utils.Math.Max(minSize, MinimumSize);
            }
            else
            {
                minSize = Utils.Math.Max(minSize, MinimumSize);
                Size = Utils.Math.Clamp(Size, minSize, maxSize);
            }
        }

        private int GetFirstEnabled()
        {
            for (int n = 0; n < List.Count; n++)
            {
                if (List[n].Enabled)
                    return n;
            }

            return 0;
        }

        private int GetMaxStart(int end)
        {
            int start = 0;
            int visCount = 0;
            float size;

            if (AlignVertical)
            {
                size = maxSize.Y;

                for (int n = end; n >= 0; n--)
                {
                    if (List[n].Enabled)
                    {
                        if (size >= List[n].Height || visCount < MinimumVisCount)
                        {
                            start = n;
                            size -= List[n].Height;
                            visCount++;
                        }
                        else
                            break;

                        size -= Spacing;
                    }
                }
            }
            else
            {
                size = maxSize.X;

                for (int n = end; n >= 0; n--)
                {
                    if (List[n].Enabled)
                    {
                        if (size >= List[n].Width || visCount < MinimumVisCount)
                        {
                            start = n;
                            size -= List[n].Width;
                            visCount++;
                        }
                        else
                            break;

                        size -= Spacing;
                    }
                }
            }

            return start;
        }

        private void UpdateVisible()
        {
            if (AlignVertical)
            {
                UpdateVisibleVert();
            }
            else
            {
                UpdateVisibleHorz();
            }
        }

        private void UpdateVisibleVert()
        {
            int visCount = 0, visStart = 0;
            int newEnd = 0;
            float size = maxSize.Y;
            chainSize = new Vector2();

            for (int n = 0; n < List.Count; n++)
            {
                if (List[n].Enabled)
                {
                    if (n < Start)
                        visStart++;
                    else
                    {
                        if (size >= List[n].Height || visCount < MinimumVisCount)
                        {
                            List[n].Visible = true;
                            size -= List[n].Height;

                            chainSize.Y += List[n].Height;

                            if (List[n].Width > chainSize.X)
                                chainSize.X = List[n].Width;

                            visCount++;
                            newEnd = n;
                        }
                        else
                            break;

                        chainSize.Y += Spacing;
                        size -= Spacing;
                    }
                }
            }

            end = newEnd;
            VisStart = visStart;
            VisCount = visCount;
            UpdateScrollBarSize();
        }

        private void UpdateVisibleHorz()
        {
            int visCount = 0, visStart = 0;
            int newEnd = 0;
            float size = maxSize.X;
            chainSize = new Vector2();

            for (int n = 0; n < List.Count; n++)
            {
                if (List[n].Enabled)
                {
                    if (n < Start)
                        visStart++;
                    else
                    {
                        if (size >= List[n].Width || visCount < MinimumVisCount)
                        {
                            List[n].Visible = true;
                            size -= List[n].Width;

                            chainSize.X += List[n].Width;

                            if (List[n].Height > chainSize.Y)
                                chainSize.Y = List[n].Height;

                            visCount++;
                            newEnd = n;
                        }
                        else
                            break;

                        chainSize.X += Spacing;
                        size -= Spacing;
                    }
                }
            }

            end = newEnd;
            VisStart = visStart;
            VisCount = visCount;
            UpdateScrollBarSize();
        }

        /// <summary>
        /// Recalculates and updates the height of the scroll bar.
        /// </summary>
        private void UpdateScrollBarSize()
        {
            if (AlignVertical)
            {
                scrollBar.Height = Height;
                divider.Height = scrollBar.Height;

                scrollBar.slide.button.Height = (scrollBar.Height / totalSize) * scrollBar.Height;
                scrollBar.slide.button.Visible = scrollBar.slide.button.Height < scrollBar.slide.bar.Height;
            }
            else
            {
                scrollBar.Width = Width;
                divider.Width = scrollBar.Width;

                scrollBar.slide.button.Width = (scrollBar.Width / totalSize) * scrollBar.Width;
                scrollBar.slide.button.Visible = scrollBar.slide.button.Width < scrollBar.slide.bar.Width;
            }
        }

        private int GetEnabledCount()
        {
            int count = 0;
            totalSize = 0f;

            for (int n = 0; n < List.Count; n++)
            {
                List[n].Visible = false;

                if (List[n].Enabled)
                {
                    if (AlignVertical)
                        totalSize += List[n].Height;
                    else
                        totalSize += List[n].Width;

                    count++;
                }
            }

            return count;
        }
    }
}