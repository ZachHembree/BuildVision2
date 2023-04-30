using RichHudFramework.UI.Client;
using RichHudFramework.UI.Server;
using RichHudFramework.UI.Rendering;
using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;
using EventHandler = RichHudFramework.EventHandler;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Radial selection box. Represents a list of entries as UI elements arranged around
    /// a wheel.
    /// </summary>
    public class RadialSelectionBox<TContainer, TElement>
        : HudCollection<TContainer, TElement>
        where TContainer : IScrollBoxEntry<TElement>, new()
        where TElement : HudElementBase
    {
        /// <summary>
        /// List of entries in the selection box
        /// </summary>
        public virtual IReadOnlyList<TContainer> EntryList => hudCollectionList;

        /// <summary>
        /// Currently selected entry
        /// </summary>
        public virtual TContainer Selection 
        {
            get 
            {
                if (SelectionIndex >= 0 && SelectionIndex < hudCollectionList.Count)
                    return hudCollectionList[SelectionIndex];
                else
                    return default(TContainer);
            }
        }

        /// <summary>
        /// Currently highlighted entry
        /// </summary>
        public virtual TContainer HighlightedEntry
        {
            get
            {
                if (HighlightIndex >= 0 && HighlightIndex < hudCollectionList.Count)
                    return hudCollectionList[HighlightIndex];
                else
                    return default(TContainer);
            }
        }

        /// <summary>
        /// Returns the index of the current selection. Returns -1 if nothing is selected.
        /// </summary>
        public virtual int SelectionIndex { get; protected set; }

        /// <summary>
        /// Returns the index of the entry currently highlighted. Returns -1 if nothing is highlighted.
        /// </summary>
        public virtual int HighlightIndex { get; protected set; }

        /// <summary>
        /// Maximum number of entries. Used to determine subdivisions in circle. If enabled
        /// elements exceed this value, then the total number of entries will superceed this value.
        /// </summary>
        public virtual int MaxEntryCount { get; set; }

        /// <summary>
        /// Number of entries enabled
        /// </summary>
        public virtual int EnabledCount { get; protected set; }

        /// <summary>
        /// True if using mouse gestures for input instead of cursor
        /// </summary>
        public virtual bool UseGestureInput { get; set; }

        /// <summary>
        /// Background color for the polyboard
        /// </summary>
        public virtual Color BackgroundColor { get; set; }

        /// <summary>
        /// Highlight color for the polyboard
        /// </summary>
        public virtual Color HighlightColor { get; set; }

        /// <summary>
        /// Selection color for the polyboard
        /// </summary>
        public virtual Color SelectionColor { get; set; }

        /// <summary>
        /// Cursor sensitivity for wheel scrolling on a scale from .3 to 2.
        /// </summary>
        public float CursorSensitivity { get; set; }

        public readonly PuncturedPolyBoard polyBoard;

        protected int selectionVisPos, highlightVisPos, effectiveMaxCount, minPolySize;
        protected bool isStartPosStale;
        protected Vector2 lastCursorPos, cursorNormal;
        private float lastDot;

        public RadialSelectionBox(HudParentBase parent = null) : base(parent)
        {
            polyBoard = new PuncturedPolyBoard()
            {
                Sides = 64
            };

            BackgroundColor = new Color(70, 78, 86);
            HighlightColor = TerminalFormatting.DarkSlateGrey;
            SelectionColor = TerminalFormatting.Mint;

            minPolySize = 64;
            Size = new Vector2(512f);
            MaxEntryCount = 8;
            CursorSensitivity = .5f;

            UseGestureInput = false;
            UseCursor = true;
        }

        /// <summary>
        /// Sets the selection to the entry at the corresponding index
        /// </summary>
        public void SetSelectionAt(int index)
        {
            SelectionIndex = MathHelper.Clamp(index, 0, hudCollectionList.Count - 1);
            lastCursorPos = new Vector2(HudSpace.CursorPos.X, HudSpace.CursorPos.Y);
        }

        /// <summary>
        /// Sets selection to the given entry, if it is in the collection
        /// </summary>
        public void SetSelection(TContainer container)
        {
            int index = FindIndex(x => x.Equals(container));

            if (index != -1)
                SelectionIndex = index;

            lastCursorPos = new Vector2(HudSpace.CursorPos.X, HudSpace.CursorPos.Y);
        }

        /// <summary>
        /// Highlights the entry at the given index
        /// </summary>
        public void SetHighlightAt(int index)
        {
            HighlightIndex = MathHelper.Clamp(index, 0, hudCollectionList.Count - 1);
            lastCursorPos = new Vector2(HudSpace.CursorPos.X, HudSpace.CursorPos.Y);
        }

        /// <summary>
        /// Highlights the given entry, if it's in the collection
        /// </summary>
        public void SetHighlight(TContainer container)
        {
            int index = FindIndex(x => x.Equals(container));

            if (index != -1)
                HighlightIndex = index;

            lastCursorPos = new Vector2(HudSpace.CursorPos.X, HudSpace.CursorPos.Y);
        }

        public override void Clear()
        {
            HighlightIndex = -1;
            SelectionIndex = -1;
            base.Clear();
        }

        public void ClearHighlight()
        {
            HighlightIndex = -1;
        }

        public void ClearSelection()
        {
            SelectionIndex = -1;
        }

        protected override void Layout()
        {
            // Get enabled elements and effective max count
            EnabledCount = 0;
            SelectionIndex = MathHelper.Clamp(SelectionIndex, -1, hudCollectionList.Count - 1);
            HighlightIndex = MathHelper.Clamp(HighlightIndex, -1, hudCollectionList.Count - 1);
            CursorSensitivity = MathHelper.Clamp(CursorSensitivity, 0.3f, 2f);

            for (int i = 0; i < hudCollectionList.Count; i++)
            {
                if (hudCollectionList[i].Enabled)
                {
                    hudCollectionList[i].Element.Visible = true;
                    EnabledCount++;
                }
                else
                    hudCollectionList[i].Element.Visible = false;
            }

            effectiveMaxCount = Math.Max(MaxEntryCount, EnabledCount);

            // Update entry positions
            int entrySize = polyBoard.Sides / effectiveMaxCount;
            Vector2I slice = new Vector2I(0, entrySize - 1);
            Vector2 size = CachedSize - Padding;

            for (int i = 0; i < hudCollectionList.Count; i++)
            {
                TContainer container = hudCollectionList[i];
                TElement element = container.Element;

                if (container.Enabled)
                {
                    element.Offset = 1.05f * polyBoard.GetSliceOffset(size, slice);
                    slice += entrySize;
                }
            }

            polyBoard.Sides = Math.Max(effectiveMaxCount * 6, minPolySize);
        }

        protected override void InputDepth()
        {
            State &= ~HudElementStates.IsMouseInBounds;

            if (HudMain.InputMode != HudInputMode.NoInput && (HudSpace?.IsFacingCamera ?? false))
            {
                Vector2 size = CachedSize - Padding,
                    aspect = new Vector2(size.Y / size.X, size.X / size.Y),
                    cursorPos = new Vector2(HudSpace.CursorPos.X, HudSpace.CursorPos.Y) - Position;

                cursorPos *= aspect;

                float max = .5f * (CachedSize.X - Padding.X),
                    min = polyBoard.InnerRadius * max,
                    offsetLen = cursorPos.Length();

                if (offsetLen > min && offsetLen < max)
                {
                    State |= HudElementStates.IsMouseInBounds;
                    HudMain.Cursor.TryCaptureHudSpace(HudSpace.CursorPos.Z, HudSpace.GetHudSpaceFunc);
                }
            }
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (UseGestureInput || IsMousedOver)
            {
                if (isStartPosStale)
                {
                    lastDot = 0f;
                    cursorNormal = Vector2.Zero;
                    lastCursorPos = cursorPos;
                    isStartPosStale = false;
                }

                UpdateSelection(cursorPos);   
            }
            else
            {
                isStartPosStale = true;
            }
        }

        protected virtual void UpdateSelection(Vector2 cursorPos)
        {
            Vector2 cursorOffset = cursorPos;
            
            if (UseGestureInput)
                cursorOffset -= lastCursorPos;
            else
                cursorOffset -= Position;

            if (cursorOffset.LengthSquared() > 64f)
            {
                // Find enabled entry with the offset that most closely matches
                // the direction of the normal
                float dot = .5f;
                int newSelection = -1;

                if (UseGestureInput)
                {
                    Vector2 normalizedOffset = CursorSensitivity * 0.4f * Vector2.Normalize(cursorOffset);
                    cursorNormal = Vector2.Normalize(cursorNormal + normalizedOffset);
                }
                else
                    cursorNormal = Vector2.Normalize(cursorOffset);

                for (int i = 0; i < hudCollectionList.Count; i++)
                {
                    TContainer container = hudCollectionList[i];
                    TElement element = container.Element;

                    if (container.Enabled)
                    {
                        float newDot = (float)Math.Round(Vector2.Dot(element.Offset, cursorNormal), 4);

                        if (newDot > dot)
                        {
                            dot = newDot;
                            lastDot = dot;
                            newSelection = i;
                        }
                    }
                }

                lastCursorPos = cursorPos;
                HighlightIndex = newSelection;
            }
        }

        protected void UpdateVisPos()
        {
            selectionVisPos = -1;
            highlightVisPos = -1;
            SelectionIndex = MathHelper.Clamp(SelectionIndex, -1, hudCollectionList.Count - 1);
            HighlightIndex = MathHelper.Clamp(HighlightIndex, -1, hudCollectionList.Count - 1);

            if (hudCollectionList.Count > 0)
            {
                if (SelectionIndex != -1)
                {
                    // Find visible offset index
                    for (int i = 0; i <= SelectionIndex; i++)
                    {
                        TContainer container = hudCollectionList[i];

                        if (container.Enabled)
                            selectionVisPos++;
                    }
                }

                if (HighlightIndex != -1)
                {
                    // Find visible offset index
                    for (int i = 0; i <= HighlightIndex; i++)
                    {
                        TContainer container = hudCollectionList[i];

                        if (container.Enabled)
                            highlightVisPos++;
                    }
                }
            }
        }

        protected override void Draw()
        {
            Vector2 size = CachedSize - Padding;
            int entrySize = polyBoard.Sides / effectiveMaxCount;
            polyBoard.Color = BackgroundColor;
            UpdateVisPos();

            polyBoard.Draw(size, Position, HudSpace.PlaneToWorldRef);

            if (entrySize > 0)
            {
                if (selectionVisPos != -1 && (highlightVisPos != selectionVisPos || !UseGestureInput))
                {
                    Vector2I slice = new Vector2I(0, entrySize - 1) + (selectionVisPos * entrySize);
                    polyBoard.Color = SelectionColor;
                    polyBoard.Draw(size, Position, slice, HudSpace.PlaneToWorldRef);
                }

                if (highlightVisPos != -1 && (highlightVisPos != selectionVisPos || UseGestureInput))
                {
                    Vector2I slice = new Vector2I(0, entrySize - 1) + (highlightVisPos * entrySize);
                    polyBoard.Color = HighlightColor;
                    polyBoard.Draw(size, Position, slice, HudSpace.PlaneToWorldRef);
                }
            }
        }
    }

    /// <summary>
    /// Radial selection box. Represents a list of entries as UI elements arranged around
    /// a wheel.
    /// </summary>
    public class RadialSelectionBox : RadialSelectionBox<ScrollBoxEntry>
    {
        public RadialSelectionBox(HudParentBase parent = null) : base(parent)
        { }
    }

    /// <summary>
    /// Radial selection box. Represents a list of entries as UI elements arranged around
    /// a wheel.
    /// </summary>
    public class RadialSelectionBox<TContainer> : RadialSelectionBox<TContainer, HudElementBase>
        where TContainer : IScrollBoxEntry<HudElementBase>, new()
    {
        public RadialSelectionBox(HudParentBase parent = null) : base(parent)
        { }
    }
}