using VRageMath;
using System;

namespace RichHudFramework.UI
{
    public class ScrollBox<T> : HudElementBase, IListBoxEntry where T : class, IListBoxEntry
    {
        public HudChain<T> Members { get; }
        public ReadOnlyCollection<T> List => Members.List;

        public override float Width
        {
            set
            {
                base.Width = value;

                if (value > Padding.X)
                    value -= Padding.X;

                maxSize.X = value / Scale;
            }
        }

        public override float Height
        {
            set
            {
                base.Height = value;

                if (value > Padding.Y)
                    value -= Padding.Y;

                maxSize.Y = value / Scale;
            }
        }

        /// <summary>
        /// Minimum allowable size of the list.
        /// </summary>
        public Vector2 MinimumSize { get { return minSize * Scale; } set { minSize = value / Scale; } }

        /// <summary>
        /// If set to true, the element will automatically resize to match the size of the chain.
        /// </summary>
        public bool FitToChain { get; set; }

        /// <summary>
        /// If true, the scrollbox will attempt to clamp the size of its members between the minimum and set size.
        /// </summary>
        public bool ClampMembers { get; set; }

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
                end = MathHelper.Clamp(value, 0, List.Count - 1);
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

        private Vector2 MaximumSize { get { return maxSize * Scale; } set { maxSize = value / Scale; } }

        public readonly ScrollBar scrollBar;
        public readonly TexturedBox background;
        public readonly TexturedBox divider;

        private Vector2 maxSize, minSize, chainSize;
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
            ShareCursor = true;
            CedeCursor = true;

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
            Members.RemoveChild(member);

        public void RemoveFromList(Func<T, bool> predicate) =>
            Members.Remove(predicate);

        protected override void HandleInput()
        {
            if (IsMousedOver)
            {
                if (SharedBinds.MousewheelUp.IsPressed)
                {
                    CedeCursor = false;
                    scrollBar.Current = Start - 1;
                }
                else if (SharedBinds.MousewheelDown.IsPressed)
                {
                    CedeCursor = false;
                    scrollBar.Current = Start + 1;
                }
                else
                    CedeCursor = true;
            }
            else
                CedeCursor = true;
        }

        protected override void BeforeDraw()
        {
            scrollBar.Max = GetMaxStart(List.Count - 1);
            scrollBar.Min = GetFirstEnabled();
            EnabledCount = GetEnabledCount();

            UpdateVisible();

            Vector2 min = chainSize + Padding, newSize = Size;

            Members.Size = chainSize;
            Members.Offset = new Vector2(Padding.X, -Padding.Y) * .5f;

            if (AlignVertical)
                min.X += scrollBar.Width;
            else
                min.Y += scrollBar.Height;

            if (FitToChain)
            {
                newSize = Vector2.Max(min, MinimumSize);
            }
            else
            {
                min = Vector2.Max(min, MinimumSize);
                newSize = Vector2.Clamp(newSize, min, MaximumSize);
            }

            base.Width = newSize.X;
            base.Height = newSize.Y;
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
                size = MaximumSize.Y;

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
                size = MaximumSize.X;

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
            float size = MaximumSize.Y,
                min = MinimumSize.X - Padding.X - scrollBar.Width,
                max = MaximumSize.X - Padding.X - scrollBar.Width;

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

                            if (ClampMembers)
                                List[n].Width = MathHelper.Clamp(List[n].Width, min, max);

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
            float size = MaximumSize.X, 
                min = MinimumSize.Y - Padding.Y - scrollBar.Height, 
                max = MaximumSize.Y - Padding.Y - scrollBar.Height;

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

                            if (ClampMembers)
                                List[n].Height = MathHelper.Clamp(List[n].Height, min, max);

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

                scrollBar.slide.SliderSize = new Vector2(scrollBar.slide.SliderSize.X, ((Height - Padding.Y) / totalSize) * scrollBar.Height);
            }
            else
            {
                scrollBar.Width = Width;
                divider.Width = scrollBar.Width;

                scrollBar.slide.SliderSize = new Vector2(((Width - Padding.X) / totalSize) * scrollBar.Width, scrollBar.slide.SliderSize.Y);
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