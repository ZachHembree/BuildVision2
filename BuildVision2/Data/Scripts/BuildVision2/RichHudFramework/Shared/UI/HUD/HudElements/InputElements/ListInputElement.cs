using System;
using VRageMath;

namespace RichHudFramework.UI
{
	/// <summary>
	/// Specialized <see cref="MouseInputElement"/> that manages selection, mouse highlighting, and keyboard navigation
	/// for scrollable lists (vertical or horizontal). Supports both mouse and keyboard-driven selection.
	/// </summary>
	/// <typeparam name="TElementContainer">
	/// Container type that holds each list entry (must implement <see cref="IScrollBoxEntry{T}"/>)
	/// </typeparam>
	/// <typeparam name="TElement">
	/// The actual UI element type displayed in each entry (must be a labeled element)
	/// </typeparam>
	public class ListInputElement<TElementContainer, TElement> : MouseInputElement
		where TElement : HudElementBase, IMinLabelElement
		where TElementContainer : class, IScrollBoxEntry<TElement>, new()
	{
		/// <summary>
		/// Invoked whenever the selected entry changes (including clearing selection).
		/// </summary>
		public event EventHandler SelectionChanged;

		/// <summary>
		/// Read-only view of all entries managed by this input element.
		/// </summary>
		public IReadOnlyHudCollection<TElementContainer, TElement> Entries { get; }

		/// <summary>
		/// Currently selected entry. Returns null/default if no selection or list is empty.
		/// </summary>
		public TElementContainer Selection
		{
			get
			{
				if (Entries.Count == 0 || SelectionIndex < 0 || SelectionIndex >= Entries.Count)
				{
					return default(TElementContainer);
				}
				else
				{
					return Entries[SelectionIndex];
				}
			}
		}

		/// <summary>
		/// Index of the currently selected entry. Returns -1 if nothing is selected.
		/// </summary>
		public int SelectionIndex => MathHelper.Clamp(_selectionIndex, -1, Entries.Count - 1);

		/// <summary>
		/// Index of the entry currently highlighted by the mouse cursor (or keyboard navigation when active).
		/// Always valid (clamped) even if the list is empty.
		/// </summary>
		public int HighlightIndex => MathHelper.Clamp(_highlightIndex, 0, Entries.Count - 1);

		/// <summary>
		/// Index of the entry that currently has keyboard focus for navigation.
		/// </summary>
		public int FocusIndex => MathHelper.Clamp(_focusIndex, 0, Entries.Count - 1);

		/// <summary>
		/// Indicates whether keyboard arrow keys are currently being used to navigate the list.
		/// When true, mouse movement will temporarily disable keyboard scrolling until a new key is pressed.
		/// </summary>
		public bool KeyboardScroll { get; protected set; }

		/// <summary>
		/// Range of entry indices currently visible within the scrollable viewport (inclusive).
		/// X = first visible index, Y = last visible index.
		/// </summary>
		public Vector2I ListRange { get; set; }

		/// <summary>
		/// Total size of the visible portion of the list (width × height).
		/// </summary>
		public Vector2 ListSize { get; set; }

		/// <summary>
		/// Local position of the top-left corner of the visible list area relative to this element.
		/// </summary>
		public Vector2 ListPos { get; set; }

		/// <exclude/>
		protected Vector2 lastCursorPos;
		private int _selectionIndex; // -1 == no selection
		private int _highlightIndex;
		private int _focusIndex;

		public ListInputElement(
			HudElementBase parent,
			IReadOnlyHudCollection<TElementContainer, TElement> entries
		) : base(parent)
		{
			Entries = entries;
			_selectionIndex = -1;
		}

		public ListInputElement(HudChain<TElementContainer, TElement> parent)
			: this(parent, parent)
		{ }

		/// <summary>
		/// Selects the entry at the specified index (clamped to valid range). Disabled entries are skipped automatically.
		/// Triggers <see cref="SelectionChanged"/> if the selection actually changes.
		/// </summary>
		public void SetSelectionAt(int index)
		{
			if (index != _selectionIndex)
			{
				_selectionIndex = MathHelper.Clamp(index, 0, Entries.Count - 1);
				Selection.Enabled = true;
				SelectionChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Sets the selection to the specified entry.
		/// </summary>
		public void SetSelection(TElementContainer member)
		{
			int index = Entries.FindIndex(x => member.Equals(x));

			if (index != -1 && index != _selectionIndex)
			{
				_selectionIndex = MathHelper.Clamp(index, 0, Entries.Count - 1);
				Selection.Enabled = true;
				SelectionChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Moves the current selection up/down by the given offset.
		/// Skips disabled entries. If <paramref name="wrap"/> is true, selection wraps around the list edges.
		/// </summary>
		public void OffsetSelectionIndex(int offset, bool wrap = false)
		{
			int index = _selectionIndex,
				dir = offset > 0 ? 1 : -1,
				absOffset = Math.Abs(offset);

			if (dir > 0)
			{
				for (int i = 0; i < absOffset; i++)
				{
					if (wrap)
						index = (index + dir) % Entries.Count;
					else
						index = Math.Min(index + dir, Entries.Count - 1);

					index = FindFirstEnabled(index, wrap);
				}
			}
			else
			{
				for (int i = 0; i < absOffset; i++)
				{
					if (wrap)
						index = (index + dir) % Entries.Count;
					else
						index = Math.Max(index + dir, 0);

					if (index < 0)
						index += Entries.Count;

					index = FindLastEnabled(index, wrap);
				}
			}

			SetSelectionAt(index);
		}

		/// <summary>
		/// Removes the current selection and resets highlight/focus to the first entry.
		/// </summary>
		public void ClearSelection()
		{
			_selectionIndex = -1;
			_highlightIndex = 0;
			_focusIndex = 0;
		}

		/// <summary>
		/// Updates list selection input
		/// </summary>
		/// <exclude/>
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
		/// <exclude/>
		protected virtual void UpdateSelectionInput(Vector2 cursorPos)
		{
			_selectionIndex = MathHelper.Clamp(_selectionIndex, -1, Entries.Count - 1);
			_highlightIndex = MathHelper.Clamp(_highlightIndex, 0, Entries.Count - 1);

			// If using arrow keys to scroll, adjust the scrollbox's start/end indices
			if (KeyboardScroll)
				// If using arrow keys to scroll, then the focus should follow the highlight
				_focusIndex = _highlightIndex;
			else // Otherwise, focus index should follow the selection
				_focusIndex = _selectionIndex;

			// Keyboard input
			if (FocusHandler?.HasFocus ?? false)
			{
				if (SharedBinds.UpArrow.IsNewPressed || SharedBinds.UpArrow.IsPressedAndHeld)
				{
					for (int i = _highlightIndex - 1; i >= 0; i--)
					{
						if (Entries[i].Enabled)
						{
							_highlightIndex = i;
							break;
						}
					}

					KeyboardScroll = true;
					lastCursorPos = cursorPos;
				}
				else if (SharedBinds.DownArrow.IsNewPressed || SharedBinds.DownArrow.IsPressedAndHeld)
				{
					for (int i = _highlightIndex + 1; i < Entries.Count; i++)
					{
						if (Entries[i].Enabled)
						{
							_highlightIndex = i;
							break;
						}
					}

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
							_highlightIndex = newIndex;
							listMousedOver = true;
						}
					}
				}
			}

			if ((listMousedOver && SharedBinds.LeftButton.IsNewPressed) ||
				((FocusHandler?.HasFocus ?? false) && SharedBinds.Space.IsNewPressed))
			{
				_selectionIndex = _highlightIndex;
                var owner = (object)(FocusHandler?.InputOwner) ?? Parent;
                SelectionChanged?.Invoke(owner, EventArgs.Empty);
				KeyboardScroll = false;
			}

			_highlightIndex = MathHelper.Clamp(_highlightIndex, 0, Entries.Count - 1);
		}

		/// <summary>
		/// Returns first enabled element at or after the given index. Wraps around.
		/// </summary>
		private int FindFirstEnabled(int index, bool wrap)
		{
			if (wrap)
			{
				int j = index;

				for (int n = 0; n < 2 * Entries.Count; n++)
				{
					if (Entries[j].Enabled)
						return j;

					j++;
					j %= Entries.Count;
				}
			}
			else
			{
				for (int n = index; n < Entries.Count; n++)
				{
					if (Entries[n].Enabled)
						return n;
				}
			}

			return -1;
		}

		/// <summary>
		/// Returns preceeding enabled element at or after the given index. Wraps around.
		/// </summary>
		private int FindLastEnabled(int index, bool wrap)
		{
			if (wrap)
			{
				int j = index;

				for (int n = 0; n < 2 * Entries.Count; n++)
				{
					if (Entries[j].Enabled)
						return j;

					j++;
					j %= Entries.Count;
				}
			}
			else
			{
				for (int n = index; n >= 0; n--)
				{
					if (Entries[n].Enabled)
						return n;
				}
			}

			return -1;
		}
	}
}
