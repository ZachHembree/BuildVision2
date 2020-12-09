using VRageMath;
using VRage;
using System;
using System.Collections.Generic;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Scrollable list of hud elements. Can be oriented vertically or horizontally. Min/Max size determines
    /// the maximum size of scrollbox elements as well as the scrollbox itself.
    /// </summary>
    public class ScrollBox<TElementContainer, TElement> : HudChain<TElementContainer, TElement> 
        where TElementContainer : IScrollBoxEntry<TElement>, new()
        where TElement : HudElementBase
    {
        /// <summary>
        /// Width of the scrollbox
        /// </summary>
        public override float Width
        {
            set
            {
                if (value > Padding.X)
                    value -= Padding.X;

                _absoluteWidth = value / Scale;

                if (offAxis == 0)
                {
                    if ((SizingMode & (HudChainSizingModes.ClampMembersOffAxis | HudChainSizingModes.FitMembersOffAxis)) > 0)
                        _absMaxSize.X = (value - scrollBarPadding) / Scale;
                }
                else
                    _absMinLengthInternal = _absoluteWidth;
            }
        }

        /// <summary>
        /// Height of the scrollbox
        /// </summary>
        public override float Height
        {
            set
            {
                if (value > Padding.Y)
                    value -= Padding.Y;

                _absoluteHeight = value / Scale;

                if (offAxis == 1)
                {
                    if ((SizingMode & (HudChainSizingModes.ClampMembersOffAxis | HudChainSizingModes.FitMembersOffAxis)) > 0)
                        _absMaxSize.Y = (value - scrollBarPadding) / Scale;
                }
                else
                    _absMinLengthInternal = _absoluteHeight;
            }
        }

        /// <summary>
        /// Minimum number of visible elements allowed. Supercedes maximum length. If the number of elements that
        /// can fit within the maximum length is less than this value, then this element will expand beyond its maximum
        /// size.
        /// </summary>
        public int MinVisibleCount { get; set; }

        /// <summary>
        /// Minimum total length (on the align axis) of visible members allowed in the scrollbox.
        /// </summary>
        public float MinLength { get { return _absMinLength * Scale; } set { _absMinLength = value / Scale; } }

        /// <summary>
        /// Index of the first element in the visible range in the chain.
        /// </summary>
        public int Start
        {
            get { return _start; }
            set 
            {
                _start = MathHelper.Clamp(value, 0, chainElements.Count - 1);
                updateRangeReverse = false;
            }
        }

        /// <summary>
        /// Index of the last element in the visible range in the chain.
        /// </summary>
        public int End
        {
            get { return _end; } 
            set 
            {
                _end = MathHelper.Clamp(value, 0, chainElements.Count - 1);
                updateRangeReverse = true;
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

        /// <summary>
        /// Color of the slider bar
        /// </summary>
        public Color BarColor { get { return scrollBar.slide.BarColor; } set { scrollBar.slide.BarColor = value; } }

        /// <summary>
        /// Bar color when moused over
        /// </summary>
        public Color BarHighlight { get { return scrollBar.slide.BarHighlight; } set { scrollBar.slide.BarHighlight = value; } }

        /// <summary>
        /// Color of the slider box when not moused over
        /// </summary>
        public Color SliderColor { get { return scrollBar.slide.SliderColor; } set { scrollBar.slide.SliderColor = value; } }

        /// <summary>
        /// Color of the slider button when moused over
        /// </summary>
        public Color SliderHighlight { get { return scrollBar.slide.SliderHighlight; } set { scrollBar.slide.SliderHighlight = value; } }

        public bool EnableScrolling { get; set; }

        public readonly ScrollBar scrollBar;
        public readonly TexturedBox background;
        public readonly TexturedBox divider;

        private float scrollBarPadding, _absMinLength, _absMinLengthInternal;
        private bool updateRangeReverse;
        private int _start, _end, scrollMin, scrollMax;

        public ScrollBox(bool alignVertical, HudParentBase parent = null) : base(alignVertical, parent)
        {
            background = new TexturedBox(this)
            {
                Color = new Color(41, 54, 62, 196),
                DimAlignment = DimAlignments.Both,
                ZOffset = -1,
            };

            scrollBar = new ScrollBar(this) { Vertical = alignVertical };
            divider = new TexturedBox(scrollBar) { Color = new Color(53, 66, 75) };

            if (alignVertical)
            {
                scrollBar.DimAlignment = DimAlignments.Height;
                divider.DimAlignment = DimAlignments.Height;

                scrollBar.ParentAlignment = ParentAlignments.Right | ParentAlignments.InnerH;
                divider.ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH;

                divider.Padding = new Vector2(2f, 0f);
                divider.Width = 1f;

                scrollBar.Padding = new Vector2(30f, 8f);
                scrollBar.Width = 45f;
            }
            else
            {
                scrollBar.DimAlignment = DimAlignments.Width;
                divider.DimAlignment = DimAlignments.Width;

                scrollBar.ParentAlignment = ParentAlignments.Bottom | ParentAlignments.InnerV;
                divider.ParentAlignment = ParentAlignments.Bottom | ParentAlignments.InnerV;

                divider.Padding = new Vector2(16f, 2f);
                divider.Height = 1f;

                scrollBar.Padding = new Vector2(16f);
                scrollBar.Height = 24f;
            }

            MinVisibleCount = 1;
            UseCursor = true;
            ShareCursor = false;
            EnableScrolling = true;
        }

        /// <summary>
        /// Adds an element of type <see cref="TElement"/> to the scrollbox. Enabled
        /// by default.
        /// </summary>
        public override void Add(TElement chainElement) =>
            Add(chainElement, true);

        /// <summary>
        /// Adds an element of type <see cref="TElementContainer"/> to the scrollbox.
        /// </summary>
        public void Add(TElement chainElement, bool enabled)
        {
            blockChildRegistration = true;

            if (chainElement.Parent == this)
                throw new Exception("HUD Element already registered.");

            chainElement.Register(this);

            if (chainElement.Parent != this)
                throw new Exception("HUD Element registration failed.");

            chainElements.Add(new TElementContainer { Element = chainElement, Enabled = enabled });

            blockChildRegistration = false;
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (scrollBar.MouseInput.IsLeftClicked)
                _start = (int)Math.Round(scrollBar.Current);
            else if (EnableScrolling && (IsMousedOver || scrollBar.IsMousedOver))
            {
                if (SharedBinds.MousewheelUp.IsPressed)
                    _start = MathHelper.Clamp(_start - 1, 0, scrollMax);
                else if (SharedBinds.MousewheelDown.IsPressed)
                    _start = MathHelper.Clamp(_start + 1, 0, scrollMax);
            }

            scrollBar.Current = _start;
        }

        protected override void Layout()
        {
            // Calculate effective min and max element sizes
            Vector2 effectivePadding = cachedPadding;
            scrollBarPadding = scrollBar.Size[offAxis] + divider.Size[offAxis];
            effectivePadding[offAxis] += scrollBarPadding;

            ClampElementSizeRange();
            UpdateMemberSizes();

            // Get the list length
            Vector2 largest = GetLargestElementSize();
            float rangeLength = Math.Max(Math.Max(MinLength, _absMinLengthInternal * Scale), largest[alignAxis]);

            // Update scrollbar range
            scrollMin = GetFirstEnabled();
            scrollMax = GetScrollMax(rangeLength);

            scrollBar.Min = scrollMin;
            scrollBar.Max = scrollMax;

            // Update visible range
            UpdateElementRange(rangeLength);
            UpdateElementVisibility();

            Vector2 visibleTotalSize = GetVisibleTotalSize(),
                newSize = GetNewSize(cachedSize - effectivePadding, visibleTotalSize);

            cachedSize = newSize;
            cachedSize[offAxis] += scrollBarPadding;
            _absoluteWidth = cachedSize.X / Scale;
            _absoluteHeight = cachedSize.Y / Scale;
            cachedSize += cachedPadding;

            // Snap slider to integer offsets
            scrollBar.Current = (int)Math.Round(scrollBar.Current);
            UpdateSliderSize((float)Math.Round(VisCount / (double)EnabledCount, 2));

            // Calculate member start offset
            Vector2 startOffset;

            if (alignAxis == 1)
                startOffset = new Vector2(-scrollBarPadding, newSize.Y) * .5f;
            else
                startOffset = new Vector2(-newSize.X, scrollBarPadding) * .5f;

            UpdateMemberOffsets(startOffset, effectivePadding);
        }

        /// <summary>
        /// Returns the index of the first enabled element in the list.
        /// </summary>
        private int GetFirstEnabled()
        {
            for (int n = 0; n < chainElements.Count; n++)
            {
                if (chainElements[n].Enabled)
                    return n;
            }

            return 0;
        }

        /// <summary>
        /// Calculates the maximum index offset for the scroll bar
        /// </summary>
        private int GetScrollMax(float length)
        {
            int start = 0, visCount = 0;

            for (int n = chainElements.Count - 1; n >= 0; n--)
            {
                if (chainElements[n].Enabled)
                {
                    TElement element = chainElements[n].Element;

                    if (length >= element.Size[alignAxis] || VisCount < MinVisibleCount)
                    {
                        start = n;
                        length -= element.Size[alignAxis];
                        visCount++;
                    }
                    else
                        break;

                    length -= Spacing;
                }
            }

            return start;
        }

        /// <summary>
        /// Updates the range of visible members starting with the given start index.
        /// If the starting index doesn't satisfy the minimum visible count, it will 
        /// be decreased until it does.
        /// </summary>
        private void UpdateElementRange(float length)
        {
            EnabledCount = GetVisibleIndex(chainElements.Count);
            _start = MathHelper.Clamp(_start, scrollMin, scrollMax);

            if (updateRangeReverse)
                UpdateElementRangeReverse(length);
            else
                UpdateElementRangeForward(length);

            updateRangeReverse = false;
            VisStart = GetVisibleIndex(_start);
        }

        /// <summary>
        /// Updates range of visible elements starting with the starting index.
        /// </summary>
        private void UpdateElementRangeForward(float length)
        {
            Vector2I range = new Vector2I(_start);
            VisCount = 0;

            for (int n = _start; n < chainElements.Count; n++)
            {
                if (chainElements[n].Enabled)
                {
                    TElement element = chainElements[n].Element;

                    if (length >= element.Size[alignAxis] || VisCount < MinVisibleCount)
                    {
                        range.Y = n;
                        length -= element.Size[alignAxis];
                        VisCount++;
                    }
                    else
                        break;

                    length -= Spacing;
                }
            }

            if (EnabledCount > VisCount)
            {
                // Move starting index back until minimum visible requirment is met
                for (int n = _start - 1; (n >= scrollMin && VisCount < MinVisibleCount); n--)
                {
                    if (chainElements[n].Enabled)
                    {
                        range.X = n;
                        VisCount++;
                    }
                }
            }

            _start = range.X;
            _end = range.Y;
        }

        /// <summary>
        /// Updates range of visible elements starting with the ending index.
        /// </summary>
        private void UpdateElementRangeReverse(float length)
        {
            Vector2I range = new Vector2I(_end);
            VisCount = 0;

            for (int n = _end; n >= scrollMin; n--)
            {
                if (chainElements[n].Enabled)
                {
                    TElement element = chainElements[n].Element;

                    if (length >= element.Size[alignAxis] || VisCount < MinVisibleCount)
                    {
                        range.X = n;
                        length -= element.Size[alignAxis];
                        VisCount++;
                    }
                    else
                        break;

                    length -= Spacing;
                }
            }

            if (EnabledCount > VisCount)
            {
                // Move ending index up until minimum visible requirment is met
                for (int n = _end + 1; (n < chainElements.Count && VisCount < MinVisibleCount); n++)
                {
                    if (chainElements[n].Enabled)
                    {
                        range.Y = n;
                        VisCount++;
                    }
                }
            }

            _start = range.X;
            _end = range.Y;
        }

        /// <summary>
        /// Sets element visibility such that only elements in the visible range are drawn.
        /// </summary>
        private void UpdateElementVisibility()
        {
            if (chainElements.Count > 0)
            {
                for (int n = 0; n < chainElements.Count; n++)
                    chainElements[n].Element.Visible = false;

                for (int n = _start; n <= _end; n++)
                    chainElements[n].Element.Visible = chainElements[n].Enabled;
            }
        }

        /// <summary>
        /// Returns the number of enabled elements before the one at the given index
        /// </summary>
        private int GetVisibleIndex(int index)
        {
            int count = 0;

            for (int n = 0; n < index; n++)
            {
                if (chainElements[n].Enabled)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Returns the size of the largest element in the chain.
        /// </summary>
        private Vector2 GetLargestElementSize()
        {
            Vector2 size = new Vector2();

            for (int n = 0; n < chainElements.Count; n++)
            {
                if (chainElements[n].Enabled)
                    size = Vector2.Max(size, chainElements[n].Element.Size);
            }

            return size;
        }

        /// <summary>
        /// Recalculates and updates the height of the scroll bar.
        /// </summary>
        private void UpdateSliderSize(float visRatio)
        {
            Vector2 sliderSize = scrollBar.slide.BarSize;
            sliderSize[alignAxis] = (cachedSize[alignAxis] - scrollBar.Padding[alignAxis]) * visRatio;
            scrollBar.slide.SliderSize = sliderSize;
        }
    }

    /// <summary>
    /// Scrollable list of hud elements. Can be oriented vertically or horizontally. Min/Max size determines
    /// the maximum size of scrollbox elements as well as the scrollbox itself.
    /// </summary>
    public class ScrollBox<TElementContainer> : ScrollBox<TElementContainer, HudElementBase>
        where TElementContainer : IScrollBoxEntry<HudElementBase>, new()
    {
        public ScrollBox(bool alignVertical, HudParentBase parent = null) : base(alignVertical, parent)
        { }
    }

    /// <summary>
    /// Scrollable list of hud elements. Can be oriented vertically or horizontally. Min/Max size determines
    /// the maximum size of scrollbox elements as well as the scrollbox itself.
    /// </summary>
    public class ScrollBox : ScrollBox<ScrollBoxEntry>
    {
        public ScrollBox(bool alignVertical, HudParentBase parent = null) : base(alignVertical, parent)
        { }
    }
}