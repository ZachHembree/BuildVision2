using System;
using System.Text;
using VRage;
using VRageMath;
using System.Collections.Generic;
using RichHudFramework.UI.Rendering;
using System.Collections;

namespace RichHudFramework.UI
{
    /// <summary>
    /// MouseInputElement subtype designed to manage selection, highlighting of vertically or horizontally scrolling 
    /// lists.
    /// </summary>
    public class ListInputElement<TElementContainer, TElement> : MouseInputElement
        where TElement : HudElementBase, IMinLabelElement
        where TElementContainer : class, IScrollBoxEntry<TElement>, new()
    {
        /// <summary>
        /// Invoked when an entry is selected.
        /// </summary>
        public event EventHandler SelectionChanged;

        /// <summary>
        /// Read-only collection of list entries
        /// </summary>
        public IReadOnlyHudCollection<TElementContainer, TElement> Entries { get; }

        /// <summary>
        /// Current selection. Null if empty.
        /// </summary>
        public TElementContainer Selection => (SelectionIndex != -1 && Entries.Count > 0) ? Entries[SelectionIndex] : default(TElementContainer);

        /// <summary>
        /// Index of the current selection. -1 if empty.
        /// </summary>
        public int SelectionIndex { get; protected set; }

        /// <summary>
        /// Index of the highlighted entry
        /// </summary>
        public int HighlightIndex { get; protected set; }

        /// <summary>
        /// Index of the entry with input focus 
        /// </summary>
        public int FocusIndex { get; protected set; }

        /// <summary>
        /// If true, then the element is using the keyboard for scrolling
        /// </summary>
        public bool KeyboardScroll { get; protected set; }

        /// <summary>
        /// Range of entries visible to the input element
        /// </summary>
        public Vector2I ListRange { get; set; }

        /// <summary>
        /// Visible size of list entries
        /// </summary>
        public Vector2 ListSize { get; set; }

        /// <summary>
        /// Position of list entries
        /// </summary>
        public Vector2 ListPos { get; set; }

        protected Vector2 lastCursorPos;

        public ListInputElement(IReadOnlyHudCollection<TElementContainer, TElement> entries, HudElementBase parent = null) : base(parent)
        {
            Entries = entries;
            SelectionIndex = -1;
        }

        public ListInputElement(HudChain<TElementContainer, TElement> parent = null) : this(parent, parent)
        { }

        /// <summary>
        /// Sets the selection to the member associated with the given object.
        /// </summary>
        public void SetSelectionAt(int index)
        {
            SelectionIndex = MathHelper.Clamp(index, 0, Entries.Count - 1);
            Selection.Enabled = true;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the selection to the specified entry.
        /// </summary>
        public void SetSelection(TElementContainer member)
        {
            int index = Entries.FindIndex(x => member.Equals(x));

            if (index != -1)
            {
                SelectionIndex = MathHelper.Clamp(index, 0, Entries.Count - 1);
                Selection.Enabled = true;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Clears the current selection
        /// </summary>
        public void ClearSelection()
        {
            SelectionIndex = -1;
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (Entries.Count > 0)
            {
                base.HandleInput(cursorPos);
                UpdateSelectionInput(cursorPos);
            }
        }

        /// <summary>
        /// Update selection and highlight based on user input
        /// </summary>
        protected virtual void UpdateSelectionInput(Vector2 cursorPos)
        {
            HighlightIndex = MathHelper.Clamp(HighlightIndex, 0, Entries.Count - 1);

            // If using arrow keys to scroll, adjust the scrollbox's start/end indices
            if (KeyboardScroll)
                // If using arrow keys to scroll, then the focus should follow the highlight
                FocusIndex = HighlightIndex;
            else // Otherwise, focus index should follow the selection
                FocusIndex = SelectionIndex;

            // Keyboard input
            if (HasFocus)
            {
                if (SharedBinds.UpArrow.IsNewPressed || SharedBinds.UpArrow.IsPressedAndHeld)
                {
                    HighlightIndex--;
                    KeyboardScroll = true;
                    lastCursorPos = cursorPos;
                }
                else if (SharedBinds.DownArrow.IsNewPressed || SharedBinds.DownArrow.IsPressedAndHeld)
                {
                    HighlightIndex++;
                    KeyboardScroll = true;
                    lastCursorPos = cursorPos;
                }
            }
            else
            {
                KeyboardScroll = false;
                lastCursorPos = new Vector2(float.MinValue);
            }

            bool listMousedOver = false;

            // Mouse input
            if (IsMousedOver)
            {
                // If the user moves the cursor after using arrow keys for selection then moves the
                // mouse, disable arrow selection
                if ((cursorPos - lastCursorPos).LengthSquared() > 4f)
                    KeyboardScroll = false;

                if (!KeyboardScroll)
                {
                    Vector2 cursorOffset = cursorPos - ListPos;
                    BoundingBox2 listBounds = new BoundingBox2(-ListSize * .5f, ListSize * .5f);

                    // If the list is moused over, then calculate highlight index based on cursor position.
                    if (listBounds.Contains(cursorOffset) == ContainmentType.Contains)
                    {
                        int newIndex = ListRange.X;

                        for (int i = ListRange.X; i <= ListRange.Y; i++)
                        {
                            if (Entries[i].Enabled)
                            {
                                TElement element = Entries[i].Element;
                                Vector2 halfSize = element.Size * .5f,
                                    offset = element.Offset;
                                BoundingBox2 bb = new BoundingBox2(offset - halfSize, offset + halfSize);

                                if (bb.Contains(cursorOffset) == ContainmentType.Contains)
                                    break;
                            }

                            newIndex++;
                        }

                        if (newIndex >= 0 && newIndex < Entries.Count)
                        {
                            HighlightIndex = newIndex;
                            listMousedOver = true;
                        }
                    }
                }
            }

            if ((listMousedOver && SharedBinds.LeftButton.IsNewPressed) || (HasFocus && SharedBinds.Space.IsNewPressed))
            {
                SelectionIndex = HighlightIndex;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                KeyboardScroll = false;
            }

            HighlightIndex = MathHelper.Clamp(HighlightIndex, 0, Entries.Count - 1);
        }

    }
}
