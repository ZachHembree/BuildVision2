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
            get { return background.Width; }
            set { background.Width = value; }
        }
        public override float Height
        {
            get { return background.Height; }
            set
            {
                background.Height = value;
                MaximumHeight = value;
            }
        }

        /// <summary>
        /// Maximum allowable height of the list. Only applicable if AutoResize == true.
        /// </summary>
        public float MaximumHeight { get; set; }

        /// <summary>
        /// Minimum allowable width of the list. Only applicable if AutoResize == true.
        /// </summary>
        public float MinimumWidth { get; set; }

        public override Vector2 Padding
        {
            get { return Chain.Padding; }
            set
            {
                Chain.Padding = value;
                //scrollBar.Padding = value;
            }
        }

        /// <summary>
        /// Determines whether or not chain elements will be resized to match the
        /// size of the element along the axis of alignment.
        /// </summary>
        public bool AutoResize { get { return Chain.AutoResize; } set { Chain.AutoResize = value; } }

        /// <summary>
        /// Distance between the chain elements.
        /// </summary>
        public float Spacing { get { return Chain.Spacing; } set { Chain.Spacing = value; } }

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
                end = Utils.Math.Clamp(value, 0, ChainElements.Count - 1);
                Start = GetStartMax(end);
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

        private int end;

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

            CaptureCursor = true;
            AutoResize = true;
            Padding = new Vector2(16f, 16f);
            Size = new Vector2(355f, 223f);
        }

        public void AddToList(T element)
        {
            Chain.Add(element, true);
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
                    scrollBar.Current = Start - 1;
                }

                if (SharedBinds.MousewheelDown.IsNewPressed)
                {
                    scrollBar.Current = Start + 1;
                }
            }
        }

        protected override void Draw()
        {
            // The scroll bar automatically clamps the current value to the set min and max
            scrollBar.Min = GetFirstMember();
            scrollBar.Max = GetStartMax(ChainElements.Count - 1);

            UpdateVisibleRange();

            background.Height = Chain.Height;
            scrollBar.Height = Height;
            divider.Height = Height;

            background.Width = Math.Max(MinimumWidth, Chain.Width + divider.Width + scrollBar.Width);
        }

        /// <summary>
        /// Continually updates the range of elements to be rendered as well as the height of the scroll
        /// bar.
        /// </summary>
        private void UpdateVisibleRange()
        {
            int newStart = -1;

            EnabledCount = 0;
            VisCount = 0;
            VisStart = 0;

            // Reset the visibility of every element, count the number of
            // enabled elements, and find visStart
            for (int n = 0; n < ChainElements.Count; n++)
            {
                ChainElements[n].Visible = false;

                if (ChainElements[n].Enabled)
                {
                    EnabledCount++;

                    if (newStart == -1)
                        newStart = n;

                    if (n <= Start)
                        VisStart++;
                }
            }

            float height = MaximumHeight - Padding.Y;
            scrollBar.Min = newStart;

            // Set the elements in the visible range to be rendered and
            // update the ending index
            for (int n = Start; n < ChainElements.Count; n++)
            {
                if (ChainElements[n].Enabled)
                {
                    height -= ChainElements[n].Height;

                    if (height >= 0f)
                    {
                        ChainElements[n].Visible = true;
                        end = n;
                        VisCount++;
                    }
                    else
                        break;
                }
            }

            UpdateScrollBarHeight();
        }

        /// <summary>
        /// Recalculates and updates the height of the scroll bar.
        /// </summary>
        private void UpdateScrollBarHeight()
        {
            if (EnabledCount > VisCount && EnabledCount > 1)
            {
                scrollBar.slide.button.Height = (VisCount / (float)(EnabledCount)) * scrollBar.Height;
                scrollBar.slide.button.Visible = scrollBar.slide.button.Height < scrollBar.Height;
            }
            else
                scrollBar.slide.button.Visible = false;
        }

        private int GetFirstMember()
        {
            int index = 0;

            for (int n = 0; n < ChainElements.Count; n++)
            {
                if (ChainElements[n].Enabled)
                {
                    index = n;
                    break;
                }
            }

            return index;
        }

        /// <summary>
        /// Returns the maximum start value S.T. the list will always display the
        /// maximum number of elements.
        /// </summary>
        private int GetStartMax(int end)
        {
            int max = 0;
            float height = MaximumHeight - Padding.Y;

            for (int n = end; n >= 0; n--)
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