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
        /// Index of the first element in the visible range in the chain.
        /// </summary>
        public int Start
        {
            get { return MathHelper.Clamp(_intStart, 0, hudCollectionList.Count - 1); }
            set
            {
                if (value != _intStart)
                {
                    _intStart = MathHelper.Clamp(value, 0, hudCollectionList.Count - 1);
                    ScrollBar.Current = GetMinScrollOffset(_intStart, false);
                }
            }
        }

        /// <summary>
        /// Index of the last element in the visible range in the chain.
        /// </summary>
        public int End
        {
            get { return MathHelper.Clamp(_intEnd, 0, hudCollectionList.Count - 1); }
            set
            {
                if (value != _intEnd)
                {
                    _intEnd = MathHelper.Clamp(value, 0, hudCollectionList.Count - 1);
                    ScrollBar.Current = GetMinScrollOffset(_intEnd, true);
                }
            }
        }

        /// <summary>
        /// Range of elements including elements immediately before and after the logical visible
        /// range to allow for clipping.
        /// </summary>
        public Vector2I ClipRange => new Vector2I(_start, _end);

        /// <summary>
        /// Position of the first visible element as it appears in the UI. Does not correspond to actual index.
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

        /// <summary>
        /// If enabled scrolling using the scrollbar and mousewheel will be allowed
        /// </summary>
        public bool EnableScrolling { get; set; }

        /// <summary>
        /// Enable/disable smooth scrolling and range clipping
        /// </summary>
        public bool UseSmoothScrolling { get; set; }

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

        protected float scrollBarPadding;
        protected int _intStart, _intEnd, _start, _end, firstEnabled;

        public ScrollBox(bool alignVertical, HudParentBase parent = null) : base(alignVertical, parent)
        {
            ScrollBar = new ScrollBar(this);
            Divider = new TexturedBox(ScrollBar) { Color = new Color(53, 66, 75) };
            Background = new TexturedBox(this)
            {
                Color = TerminalFormatting.DarkSlateGrey,
                DimAlignment = DimAlignments.Size,
                ZOffset = -1,
            };

            UseCursor = true;
            ShareCursor = false;
            EnableScrolling = true;
            UseSmoothScrolling = true;
            ZOffset = 1;
            ScrollBar.Vertical = alignVertical;

            if (alignVertical)
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

        public ScrollBox(HudParentBase parent) : this(true, parent)
        { }

        public ScrollBox() : this(true, null)
        { }

        public override Vector2 GetRangeSize(int start = 0, int end = -1)
        {
            Vector2 size = base.GetRangeSize(start, end);
            size[offAxis] += scrollBarPadding;
            return size;
        }

        public int GetRangeEnd(int count, int start = 0)
        {
            start = MathHelper.Clamp(start, 0, hudCollectionList.Count - 1);
            count = MathHelper.Clamp(count, 0, hudCollectionList.Count - start);

            if ((start + count) <= hudCollectionList.Count)
            {
                int end = start,
                    enCount = 0;

                for (int i = start; i < hudCollectionList.Count; i++)
                {
                    if (hudCollectionList[i].Enabled)
                    {
                        end = i;
                        enCount++;
                    }

                    if (enCount >= count)
                        break;
                }

                return end;
            }

            return -1;
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            ScrollBar.MouseInput.InputEnabled = EnableScrolling;

            if (hudCollectionList.Count > 0 && EnableScrolling && (IsMousedOver || ScrollBar.IsMousedOver))
            {
                if (UseSmoothScrolling)
                {
                    if (SharedBinds.MousewheelUp.IsPressed)
                        ScrollBar.Current -= hudCollectionList[_intEnd].Element.Size[alignAxis] + Spacing;
                    else if (SharedBinds.MousewheelDown.IsPressed)
                        ScrollBar.Current += hudCollectionList[_intStart].Element.Size[alignAxis] + Spacing;
                }
                else
                {
                    if (SharedBinds.MousewheelUp.IsPressed)
                        Start--;
                    else if (SharedBinds.MousewheelDown.IsPressed)
                        End++;
                }
            }
        }

        protected override void Layout()
        {
            Vector2 effectivePadding = Padding;
            scrollBarPadding = ScrollBar.Size[offAxis];
            effectivePadding[offAxis] += scrollBarPadding;

            Vector2 chainSize = CachedSize - effectivePadding;
            float visRatio = 0f;

            if (hudCollectionList.Count > 0)
            {
                // Update visible range
                float totalEnabledLength, scrollOffset,
                    rangeLength = CachedSize[alignAxis] - Padding[alignAxis];

                UpdateElementRange(rangeLength, out totalEnabledLength, out scrollOffset);

                if (UseSmoothScrolling)
                {
                    if (hudCollectionList.Count > 0)
                    {
                        hudCollectionList[_start].Element.Visible = true;
                        hudCollectionList[_end].Element.Visible = true;
                    }
                }
                else
                    scrollOffset = 0f;

                float elementSpanLength;

                if (TryGetVisibleRange(chainSize[alignAxis], chainSize[offAxis], out elementSpanLength))
                {
                    Vector2 startOffset, endOffset;
                    float rcpSpanLength = 1f / Math.Max(elementSpanLength, 1E-6f);

                    if (alignAxis == 1) // Vertical
                    {
                        startOffset = new Vector2(-.5f * scrollBarPadding, .5f * chainSize.Y + scrollOffset);
                        endOffset = new Vector2(startOffset.X, startOffset.Y - elementSpanLength);
                    }
                    else
                    {
                        startOffset = new Vector2(-.5f * chainSize.X - scrollOffset, .5f * scrollBarPadding);
                        endOffset = new Vector2(startOffset.X + elementSpanLength, startOffset.Y);
                    }

                    UpdateMemberOffsets(startOffset, endOffset, rcpSpanLength);

                    // Update scrollbar max bound and calculate offset for scrolling
                    ScrollBar.Current = (float)Math.Round(ScrollBar.Current, 6);
                    ScrollBar.Max = (float)Math.Round(Math.Max(totalEnabledLength - chainSize[alignAxis], 0f), 6);

                    // Update slider size
                    visRatio = chainSize[alignAxis] / totalEnabledLength;
                }
            }

            Vector2 sliderSize = ScrollBar.slide.BarSize;
            sliderSize[alignAxis] = (chainSize[alignAxis] - ScrollBar.Padding[alignAxis]) * visRatio;
            ScrollBar.slide.SliderSize = sliderSize;
        }

        /// <summary>
        /// Updates the range of visible members starting with the given start index.
        /// </summary>
        private void UpdateElementRange(float maxLength, out float totalEnabledLength, out float scrollOffset)
        {
            float spacing = Spacing,
                scrollCurrent = ScrollBar.Current;

            EnabledCount = 0;
            _intEnd = -1;
            firstEnabled = -1;
            totalEnabledLength = 0f;
            scrollOffset = scrollCurrent;

            for (int i = 0; i < hudCollectionList.Count; i++)
            {
                if (hudCollectionList[i].Enabled)
                {
                    TElement element = hudCollectionList[i].Element;
                    float elementSize = element.UnpaddedSize[alignAxis] + element.Padding[alignAxis],
                        delta = totalEnabledLength + elementSize - scrollCurrent - maxLength;

                    // Get first enabled element
                    if (firstEnabled == -1)
                        firstEnabled = i;

                    // Find logical end of visible range
                    if (delta <= 0f)
                    {
                        scrollOffset -= elementSize + spacing;
                        _intEnd = i;
                    }

                    totalEnabledLength += elementSize + spacing;
                    EnabledCount++;
                }
            }

            totalEnabledLength -= spacing;
            VisCount = 0;

            // Clamp indices
            int max = hudCollectionList.Count - 1;
            firstEnabled = MathHelper.Clamp(firstEnabled, 0, max);
            _intEnd = MathHelper.Clamp(_intEnd, firstEnabled, max);
            _intStart = MathHelper.Clamp(_intStart, firstEnabled, max);

            // Find start of visible range
            for (int i = _intEnd; i >= firstEnabled; i--)
            {
                if (hudCollectionList[i].Enabled)
                {
                    TElement element = hudCollectionList[i].Element;
                    float elementSize = element.UnpaddedSize[alignAxis] + element.Padding[alignAxis];

                    if (maxLength >= elementSize)
                    {
                        scrollOffset += elementSize + spacing;
                        _intStart = i;
                        VisCount++;
                    }
                    else
                        break;

                    maxLength -= elementSize + spacing;
                }
            }

            // Find indices of nearest enabled element(s) before and after the logical visible range
            _start = _intStart;
            _end = _intEnd;

            if (UseSmoothScrolling)
            {
                for (int i = _intStart - 1; i >= firstEnabled; i--)
                {
                    if (hudCollectionList[i].Enabled)
                    {
                        _start = i;
                        break;
                    }
                }

                for (int i = _intEnd + 1; i < hudCollectionList.Count; i++)
                {
                    if (hudCollectionList[i].Enabled)
                    {
                        _end = i;
                        break;
                    }
                }

                if (_start != _intStart)
                    scrollOffset += hudCollectionList[_start].Element.Size[alignAxis] + spacing;
            }

            VisStart = GetVisibleIndex(_intStart);

            for (int i = 0; i < hudCollectionList.Count; i++)
                hudCollectionList[i].Element.Visible = (i >= _intStart && i <= _intEnd) && hudCollectionList[i].Enabled;
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
        /// Returns the shortest offset required to bring a member at the given index to either
        /// end of the scrollbox.
        /// </summary>
        private float GetMinScrollOffset(int index, bool getEnd)
        {
            if (hudCollectionList.Count > 0)
            {
                firstEnabled = MathHelper.Clamp(firstEnabled, 0, hudCollectionList.Count - 1);

                float elementSize,
                    offset = .1f,
                    spacing = Spacing;

                if (getEnd)
                    offset -= CachedSize[alignAxis] - Padding[alignAxis] + spacing;
                else
                {
                    index--;
                }

                for (int i = 0; i <= index && i < hudCollectionList.Count; i++)
                {
                    if (hudCollectionList[i].Enabled)
                    {
                        elementSize = hudCollectionList[i].Element.Size[alignAxis];
                        offset += (elementSize + spacing);
                    }
                }

                return Math.Max((float)Math.Round(offset, 6), 0f);
            }
            else
                return 0f;
        }

        public override void GetUpdateAccessors(List<HudUpdateAccessors> UpdateActions, byte treeDepth)
        {
            int preloadRange = Math.Max((End - Start) * 2, 10),
                preloadStart = MathHelper.Clamp(Start - preloadRange, 0, hudCollectionList.Count - 1),
                preloadCount = MathHelper.Clamp((2 * preloadRange), 0, hudCollectionList.Count - preloadStart);
            HudElementStates memberFlags = HudElementStates.CanPreload;

            if (UseSmoothScrolling)
                memberFlags |= HudElementStates.IsSelectivelyMasked;

            NodeUtils.SetNodesState<TElementContainer, TElement>
                (HudElementStates.CanPreload, true, hudCollectionList, 0, hudCollectionList.Count);
            NodeUtils.SetNodesState<TElementContainer, TElement>
                (memberFlags, false, hudCollectionList, preloadStart, preloadCount);

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

        public ScrollBox(HudParentBase parent = null) : base(parent)
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

        public ScrollBox(HudParentBase parent = null) : base(parent)
        { }
    }
}