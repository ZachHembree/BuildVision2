using VRageMath;
using VRage;
using System;
using System.Collections.Generic;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework.UI
{
    using HudUpdateAccessors = MyTuple<
        ApiMemberAccessor,
        MyTuple<Func<ushort>, Func<Vector3D>>, // ZOffset + GetOrigin
        Action, // DepthTest
        Action, // HandleInput
        Action<bool>, // BeforeLayout
        Action // BeforeDraw
    >;

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
                    if (value > 0f && (SizingMode & (HudChainSizingModes.ClampMembersOffAxis | HudChainSizingModes.FitMembersOffAxis)) > 0)
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
                    if (value > 0f && (SizingMode & (HudChainSizingModes.ClampMembersOffAxis | HudChainSizingModes.FitMembersOffAxis)) > 0)
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
            get { return MathHelper.Clamp(_start, 0, hudCollectionList.Count - 1); }
            set 
            {
                _start = MathHelper.Clamp(value, 0, hudCollectionList.Count - 1);
                updateRangeReverse = false;
            }
        }

        /// <summary>
        /// Index of the last element in the visible range in the chain.
        /// </summary>
        public int End
        {
            get { return MathHelper.Clamp(_end, 0, hudCollectionList.Count - 1); } 
            set 
            {
                _end = MathHelper.Clamp(value, 0, hudCollectionList.Count - 1);
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
        public Color Color { get { return Background.Color; } set { Background.Color = value; } }

        /// <summary>
        /// Color of the slider bar
        /// </summary>
        public Color BarColor { get { return ScrollBar.slide.BarColor; } set { ScrollBar.slide.BarColor = value; } }

        /// <summary>
        /// Bar color when moused over
        /// </summary>
        public Color BarHighlight { get { return ScrollBar.slide.BarHighlight; } set { ScrollBar.slide.BarHighlight = value; } }

        /// <summary>
        /// Color of the slider box when not moused over
        /// </summary>
        public Color SliderColor { get { return ScrollBar.slide.SliderColor; } set { ScrollBar.slide.SliderColor = value; } }

        /// <summary>
        /// Color of the slider button when moused over
        /// </summary>
        public Color SliderHighlight { get { return ScrollBar.slide.SliderHighlight; } set { ScrollBar.slide.SliderHighlight = value; } }

        public bool EnableScrolling { get; set; }

        public override bool AlignVertical 
        { 
            set 
            {
                ScrollBar.Vertical = value;
                base.AlignVertical = value;

                if (value)
                {
                    ScrollBar.DimAlignment = DimAlignments.Height;
                    Divider.DimAlignment = DimAlignments.Height;

                    ScrollBar.ParentAlignment = ParentAlignments.Right | ParentAlignments.InnerH;
                    Divider.ParentAlignment = ParentAlignments.Left | ParentAlignments.InnerH;

                    Divider.Padding = new Vector2(2f, 0f);
                    Divider.Width = 1f;

                    ScrollBar.Padding = new Vector2(30f, 10f);
                    ScrollBar.Width = 43f;
                }
                else
                {
                    ScrollBar.DimAlignment = DimAlignments.Width;
                    Divider.DimAlignment = DimAlignments.Width;

                    ScrollBar.ParentAlignment = ParentAlignments.Bottom | ParentAlignments.InnerV;
                    Divider.ParentAlignment = ParentAlignments.Bottom | ParentAlignments.InnerV;

                    Divider.Padding = new Vector2(16f, 2f);
                    Divider.Height = 1f;

                    ScrollBar.Padding = new Vector2(16f);
                    ScrollBar.Height = 24f;
                }
            } 
        }

        public ScrollBar ScrollBar { get; protected set; }
        public TexturedBox Divider { get; protected set; }
        public TexturedBox Background { get; protected set; }

        private float scrollBarPadding, _absMinLength, _absMinLengthInternal;
        private bool updateRangeReverse;
        private int _start, _end, scrollMin, scrollMax;

        public ScrollBox(bool alignVertical, HudParentBase parent = null) : base(alignVertical, parent)
        {
            Background = new TexturedBox(this)
            {
                Color = TerminalFormatting.DarkSlateGrey,
                DimAlignment = DimAlignments.Both,
                ZOffset = -1,
            };

            MinVisibleCount = 1;
            UseCursor = true;
            ShareCursor = false;
            EnableScrolling = true;
            ZOffset = 1;
        }

        protected override void Init()
        {
            ScrollBar = new ScrollBar(this);
            Divider = new TexturedBox(ScrollBar) { Color = new Color(53, 66, 75) };
        }

        public ScrollBox(HudParentBase parent) : this(false, parent)
        { }

        public ScrollBox() : this(false, null)
        { }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (ScrollBar.MouseInput.IsLeftClicked)
                _start = (int)Math.Round(ScrollBar.Current);
            else if (EnableScrolling && (IsMousedOver || ScrollBar.IsMousedOver))
            {
                if (SharedBinds.MousewheelUp.IsPressed)
                    _start = MathHelper.Clamp(_start - 1, 0, scrollMax);
                else if (SharedBinds.MousewheelDown.IsPressed)
                    _start = MathHelper.Clamp(_start + 1, 0, scrollMax);
            }

            ScrollBar.Current = _start;
        }

        protected override void Layout()
        {
            // Calculate effective min and max element sizes
            Vector2 effectivePadding = cachedPadding;
            scrollBarPadding = ScrollBar.Size[offAxis];
            effectivePadding[offAxis] += scrollBarPadding;

            ClampElementSizeRange();
            UpdateMemberSizes();

            // Get the list length
            Vector2 largest = GetLargestElementSize();
            float rangeLength = Math.Max(Math.Max(MinLength, _absMinLengthInternal * Scale), largest[alignAxis]);

            // Update scrollbar range
            scrollMin = GetFirstEnabled();
            scrollMax = GetScrollMax(rangeLength);

            ScrollBar.Min = scrollMin;
            ScrollBar.Max = scrollMax;

            // Update visible range
            UpdateElementRange(rangeLength);
            UpdateElementVisibility();

            Vector2 size = cachedSize, 
                visibleTotalSize = GetVisibleTotalSize(),
                listSize = GetListSize(size - effectivePadding, visibleTotalSize);

            size = listSize;
            size[offAxis] += scrollBarPadding;
            _absoluteWidth = size.X / Scale;
            _absoluteHeight = size.Y / Scale;

            // Snap slider to integer offsets
            ScrollBar.Current = (int)Math.Round(ScrollBar.Current);

            // Update slider size
            float visRatio = ((float)Math.Round(VisCount / (double)EnabledCount, 2));
            Vector2 sliderSize = ScrollBar.slide.BarSize;

            sliderSize[alignAxis] = (Size[alignAxis] - ScrollBar.Padding[alignAxis]) * visRatio;
            ScrollBar.slide.SliderSize = sliderSize;

            // Calculate member start offset
            Vector2 startOffset;

            if (alignAxis == 1)
                startOffset = new Vector2(-scrollBarPadding, listSize.Y) * .5f;
            else
                startOffset = new Vector2(-listSize.X, scrollBarPadding) * .5f;

            UpdateMemberOffsets(startOffset, effectivePadding);
        }

        /// <summary>
        /// Returns the index of the first enabled element in the list.
        /// </summary>
        private int GetFirstEnabled()
        {
            for (int n = 0; n < hudCollectionList.Count; n++)
            {
                if (hudCollectionList[n].Enabled)
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

            for (int n = hudCollectionList.Count - 1; n >= 0; n--)
            {
                if (hudCollectionList[n].Enabled)
                {
                    TElement element = hudCollectionList[n].Element;

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
            EnabledCount = GetVisibleIndex(hudCollectionList.Count);
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

            for (int n = _start; n < hudCollectionList.Count; n++)
            {
                if (hudCollectionList[n].Enabled)
                {
                    TElement element = hudCollectionList[n].Element;

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
                    if (hudCollectionList[n].Enabled)
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
                if (hudCollectionList[n].Enabled)
                {
                    TElement element = hudCollectionList[n].Element;

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
                for (int n = _end + 1; (n < hudCollectionList.Count && VisCount < MinVisibleCount); n++)
                {
                    if (hudCollectionList[n].Enabled)
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
            if (hudCollectionList.Count > 0)
            {
                for (int n = 0; n < hudCollectionList.Count; n++)
                    hudCollectionList[n].Element.Visible = false;

                for (int n = _start; n <= _end; n++)
                    hudCollectionList[n].Element.Visible = hudCollectionList[n].Enabled;
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
                if (hudCollectionList[n].Enabled)
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

            for (int n = 0; n < hudCollectionList.Count; n++)
            {
                if (hudCollectionList[n].Enabled)
                    size = Vector2.Max(size, hudCollectionList[n].Element.Size);
            }

            return size;
        }

        public override void GetUpdateAccessors(List<HudUpdateAccessors> UpdateActions, byte treeDepth)
        {
            int preloadRange = Math.Max((End - Start) * 2, 10),
                preloadStart = MathHelper.Clamp(Start - preloadRange, 0, hudCollectionList.Count - 1),
                preloadCount = MathHelper.Clamp((End + preloadRange) - preloadStart, 0, hudCollectionList.Count - preloadStart);

            NodeUtils.SetNodesState<TElementContainer, TElement>
                (HudElementStates.CanPreload, true, hudCollectionList, 0, hudCollectionList.Count);
            NodeUtils.SetNodesState<TElementContainer, TElement>
                (HudElementStates.CanPreload, false, hudCollectionList, preloadStart, preloadCount);

            base.GetUpdateAccessors(UpdateActions, treeDepth);
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