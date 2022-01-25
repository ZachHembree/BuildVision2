using RichHudFramework.UI.Client;
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
        /// Currently highlighted entry
        /// </summary>
        public virtual TContainer Selection => (selection != -1) ? hudCollectionList[selection] : default(TContainer);

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
        /// Enables/disables highlighting
        /// </summary>
        public virtual bool IsInputEnabled { get; set; }

        public readonly PuncturedPolyBoard polyBoard;

        protected int selection, effectiveMaxCount;
        protected bool isStartPosStale;
        protected Vector2 cursorStartPos;

        public RadialSelectionBox(HudParentBase parent = null) : base(parent)
        {
            polyBoard = new PuncturedPolyBoard()
            {
                Color = new Color(255, 255, 255, 128),
                Sides = 64
            };

            Size = new Vector2(512f);
            MaxEntryCount = 8;
        }

        protected override void Layout()
        {
            // Get enabled elements and effective max count
            EnabledCount = 0;

            for (int i = 0; i < hudCollectionList.Count; i++)
            {
                if (hudCollectionList[i].Enabled)
                    EnabledCount++;
            }

            effectiveMaxCount = Math.Max(MaxEntryCount, EnabledCount);

            // Update entry positions
            int entrySize = polyBoard.Sides / effectiveMaxCount;
            Vector2I slice = new Vector2I(0, entrySize - 1);
            Vector2 size = cachedSize - cachedPadding;

            for (int i = 0; i < hudCollectionList.Count; i++)
            {
                TContainer container = hudCollectionList[i];
                TElement element = container.Element;

                if (container.Enabled)
                {
                    element.Offset = polyBoard.GetSliceOffset(size, slice);
                    slice += entrySize;
                }
            }
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (IsInputEnabled)
            {
                if (isStartPosStale)
                {
                    cursorStartPos = cursorPos;
                    isStartPosStale = false;
                }

                Vector2 cursorOffset = cursorPos - cursorStartPos;
                float dot = .2f;
                int newSelection = -1;

                for (int i = 0; i < hudCollectionList.Count; i++)
                {
                    TContainer container = hudCollectionList[i];
                    TElement element = container.Element;

                    if (container.Enabled)
                    {
                        float newDot = Vector2.Dot(element.Offset, cursorOffset);

                        if (newDot > dot)
                        {
                            dot = newDot;
                            newSelection = i;
                        }
                    }
                }

                if (selection != newSelection)
                {
                    selection = newSelection;
                }
            }
            else
            {
                selection = -1;
                isStartPosStale = true;
            }
        }

        protected override void Draw()
        {
            Vector2 size = cachedSize - cachedPadding;
            polyBoard.Draw(size, cachedOrigin, ref HudSpace.PlaneToWorldRef[0]);

            if (selection != -1)
            {
                int entrySize = polyBoard.Sides / effectiveMaxCount;
                Vector2I slice = new Vector2I(0, entrySize - 1) + (selection * entrySize);

                polyBoard.Draw(size, cachedOrigin, ref HudSpace.PlaneToWorldRef[0], slice);
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