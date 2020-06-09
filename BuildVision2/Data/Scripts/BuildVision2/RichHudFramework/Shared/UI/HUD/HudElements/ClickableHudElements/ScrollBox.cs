using VRageMath;
using System;
using System.Collections.Generic;

namespace RichHudFramework.UI
{
    public enum ScrollBoxSizingModes : byte
    {
        None = 0,

        /// <summary>
        /// In this mode, the element will automatically resize to match the size of the chain.
        /// </summary>
        FitToMembers = 1,

        /// <summary>
        /// In this mode, scrollbox members will be automatically resized to fill the scrollbox along the axis of alignment.
        /// </summary>
        FitMembersToBox = 2,

        /// <summary>
        /// In this mode, scrollbox member sizes will be clamped between the minimum size and the size of the scrollbox.
        /// </summary>
        ClampMembers = 3,
    }

    /// <summary>
    /// Scrollable list of hud elements. Can be oriented vertically or horizontally.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ScrollBox<T> : HudElementBase, IListBoxEntry where T : class, IListBoxEntry
    {
        /// <summary>
        /// Chain element used to position list members.
        /// </summary>
        public HudList<T> List { get; }

        /// <summary>
        /// Width of the scrollbox in pixels.
        /// </summary>
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

        /// <summary>
        /// Height of the scrollbox in pixels.
        /// </summary>
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
        /// Determines how/if the scrollbox will attempt to resize member elements.
        /// </summary>
        public ScrollBoxSizingModes SizingMode { get; set; }

        /// <summary>
        /// Determines whether or not the element will be enabled and visible in other scroll boxes.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Determines whether or not the scrollbox will be oriented vertically. True by default.
        /// </summary>
        public bool AlignVertical
        {
            get { return axis1 == 0; }
            set
            {
                axis1 = value ? 0 : 1;
                List.AlignVertical = value;
                scrollBar.Vertical = value;

                if (AlignVertical)
                {
                    List.ParentAlignment = ParentAlignments.Top | ParentAlignments.Left | ParentAlignments.Inner;
                    scrollBar.ParentAlignment = ParentAlignments.Right | ParentAlignments.InnerH;
                    divider.ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH;

                    divider.Padding = new Vector2(2f, 0f);
                    divider.Width = 1f;

                    scrollBar.Padding = new Vector2(30f, 8f);
                    scrollBar.Width = 45f;
                }
                else
                {
                    List.ParentAlignment = ParentAlignments.Top | ParentAlignments.Left | ParentAlignments.Inner;
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
        public float Spacing { get { return List.Spacing; } set { List.Spacing = value; } }

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

        public bool EnableScrolling 
        { 
            get { return scrollInput.Visible; } 
            set 
            { 
                scrollInput.Visible = value;
                scrollBar.slide.MouseInput.CaptureCursor = value;
            } 
        }

        private Vector2 MaximumSize { get { return maxSize * Scale; } set { maxSize = value / Scale; } }

        public readonly ScrollBar scrollBar;
        public readonly TexturedBox background;
        public readonly TexturedBox divider;

        private readonly MouseInputFilter scrollInput;
        private Vector2 maxSize, minSize, chainSize;
        private float totalSize;
        private int end, axis1;

        public ScrollBox(IHudParent parent = null) : base(parent)
        {
            background = new TexturedBox(this)
            {
                Color = new Color(41, 54, 62),
                DimAlignment = DimAlignments.Both,
            };

            scrollInput = new MouseInputFilter(this)
            {
                Binds = new IBind[] { SharedBinds.MousewheelUp, SharedBinds.MousewheelDown },
                DimAlignment = DimAlignments.Both
            };

            scrollBar = new ScrollBar(this)
            {
                Min = 0,
            };

            divider = new TexturedBox(scrollBar)
            {
                Color = new Color(53, 66, 75),
            };

            List = new HudList<T>(this)
            {
                AutoResize = true,
            };

            AlignVertical = true;
            Enabled = true;
            MinimumVisCount = 1;
        }

        /// <summary>
        /// Adds a hud element to the scrollbox.
        /// <param name="element"></param>
        public void AddToList(T element) =>
            List.Add(element);

        /// <summary>
        /// Finds the scrollbox member that meets the conditions
        /// required by the predicate.
        /// </summary>
        public T Find(Func<T, bool> predicate) =>
            List.Find(x => predicate(x));

        /// <summary>
        /// Finds the index of the scrollbox member that meets the conditions
        /// required by the predicate.
        /// </summary>
        public int FindIndex(Func<T, bool> predicate) =>
            List.FindIndex(x => predicate(x));

        /// <summary>
        /// Removes the given member from the scrollbox.
        /// </summary>
        public void RemoveFromList(T member) =>
            List.RemoveChild(member);

        /// <summary>
        /// Removes the scrollbox member that meets the conditions
        /// required by the predicate.
        /// </summary>
        public void RemoveFromList(Func<T, bool> predicate) =>
            List.Remove(predicate);

        /// <summary>
        /// Unparents all HUD elements from list.
        /// </summary>
        public void Clear()
        {
            List.Clear();
        }

        /// <summary>
        /// Resets hud elements for reuse.
        /// </summary>
        public void Reset()
        {
            Enabled = false;
            List.Reset();
        }

        protected override void HandleInput()
        {
            scrollInput.CaptureCursor = scrollBar.Min != scrollBar.Max;

            if (scrollInput.IsControlPressed)
            {
                if (SharedBinds.MousewheelUp.IsPressed)
                    scrollBar.Current = Start - 1;
                else if (SharedBinds.MousewheelDown.IsPressed)
                    scrollBar.Current = Start + 1;
            }
        }

        protected override void Layout()
        {
            scrollBar.Max = GetMaxStart(List.Count - 1);
            scrollBar.Min = GetFirstEnabled();
            EnabledCount = GetEnabledCount();

            UpdateVisible();

            Vector2 min = chainSize + Padding, newSize = Size;

            List.Size = chainSize;
            List.Offset = new Vector2(Padding.X, -Padding.Y) * .5f;

            if (AlignVertical)
                min.X += scrollBar.Width;
            else
                min.Y += scrollBar.Height;

            if (SizingMode == ScrollBoxSizingModes.FitToMembers)
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
            int start = 0, visCount = 0,
                axis2 = axis1 == 0 ? 1 : 0;
            float size = MaximumSize[axis2];

            for (int n = end; n >= 0; n--)
            {
                if (List[n].Enabled)
                {
                    if (size >= List[n].Size[axis2] || visCount < MinimumVisCount)
                    {
                        start = n;
                        size -= List[n].Size[axis2];
                        visCount++;
                    }
                    else
                        break;

                    size -= Spacing;
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

                            if (SizingMode == ScrollBoxSizingModes.ClampMembers)
                                List[n].Width = MathHelper.Clamp(List[n].Width, min, max);
                            else if (SizingMode == ScrollBoxSizingModes.FitMembersToBox)
                                List[n].Width = max;

                            if (List[n].Width > chainSize.X)
                                chainSize.X = List[n].Width;

                            chainSize.Y += Spacing;
                            size -= Spacing;
                            visCount++;
                            newEnd = n;
                        }
                        else
                            break;
                    }
                }
            }

            chainSize.Y -= Spacing;
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

                            if (SizingMode == ScrollBoxSizingModes.ClampMembers)
                                List[n].Height = MathHelper.Clamp(List[n].Height, min, max);
                            else if (SizingMode == ScrollBoxSizingModes.FitMembersToBox)
                                List[n].Height = max;

                            if (List[n].Height > chainSize.Y)
                                chainSize.Y = List[n].Height;

                            chainSize.X += Spacing;
                            size -= Spacing;
                            visCount++;
                            newEnd = n;
                        }
                        else
                            break;
                    }
                }
            }

            chainSize.X -= Spacing;
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
                divider.Height = Height;

                scrollBar.slide.SliderHeight = (chainSize.Y / totalSize) * Height;
            }
            else
            {
                scrollBar.Width = Width;
                divider.Width = Width;

                scrollBar.slide.SliderWidth = (chainSize.X / totalSize) * Width;
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

                    totalSize += Spacing;
                    count++;
                }
            }

            totalSize -= Spacing;
            return count;
        }
    }
}