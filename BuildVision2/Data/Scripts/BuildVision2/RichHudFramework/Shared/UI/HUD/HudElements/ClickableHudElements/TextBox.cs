using RichHudFramework.UI.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using VRageMath;

namespace RichHudFramework.UI
{
	using Client;
    using Server;

	/// <summary>
	/// Interactive, clickable text box with caret and highlighting. Text only, no background or
	/// scrollbars.
	/// </summary>
	public class TextBox : Label, IClickableElement, IBindInputElement, IValueControl<ITextBuilder>
	{
		/// <summary>
		/// Invoked whenever a change is made to the text. Invokes once every 500ms, at most.
		/// </summary>
		public event EventHandler ValueChanged;

		/// <summary>
		/// Registers a text update callback. For use in object initializers.
		/// </summary>
		public EventHandler UpdateValueCallback { set { ValueChanged += value; } }

		/// <summary>
		/// Returns an interface to the rich text content of the control.
		/// <para>Supports <see cref="Object.ToString"/> for getting plain text copies.</para>
		/// </summary>
		public ITextBuilder Value => TextBoard;

        /// <summary>
        /// Determines whether or not the textbox will allow the user to edit its contents
        /// </summary>
        public bool EnableEditing { get { return caret.ShowCaret; } set { caret.ShowCaret = value; } }

		/// <summary>
		/// Determines whether the user will be allowed to highlight text
		/// </summary>
		public bool EnableHighlighting { get; set; }

		/// <summary>
		/// Indicates whether or not the textbox will accept input
		/// </summary>
		public bool InputOpen { get; private set; }

		/// <summary>
		/// Used to restrict the range of characters allowed for input.
		/// </summary>
		public Func<char, bool> CharFilterFunc { get; set; }

        /// <summary>
        /// Index of the character currently selected by the caret.
        /// </summary>
        public Vector2I CaretPosition => Vector2I.Max(caret.CaretIndex, Vector2I.Zero);

		/// <summary>
		/// Index of the first character in the selected range.
		/// </summary>
		public Vector2I SelectionStart => selectionBox.Start;

		/// <summary>
		/// Index of the last character in the selected range.
		/// </summary>
		public Vector2I SelectionEnd => selectionBox.End;

		/// <summary>
		/// If true, then text box currently has a range of characters selected.
		/// </summary>
		public bool SelectionEmpty => selectionBox.Empty;

		/// <summary>
		/// If true, the caret will move to the end of the text when it gains focus.
		/// </summary>
		public bool MoveToEndOnGainFocus { get; set; }

		/// <summary>
		/// If true, any text selections will be cleared when focus is lost.
		/// </summary>
		public bool ClearSelectionOnLoseFocus { get; set; }

		/// <summary>
		/// Alternative line break character. Useful if Enter/Return is unavailable.
		/// </summary>
		public char NewLineChar { get; set; }

		public IFocusHandler FocusHandler { get; }

		/// <summary>
		/// Custom bind input interface for this element.
		/// Handles input events for cut, copy, paste, etc.
		/// </summary>
		public IBindInput BindInput { get; }

		public IMouseInput MouseInput { get; }

		public override bool IsMousedOver => MouseInput.IsMousedOver;

		/// <exclude/>
		protected readonly MouseInputElement _mouseInput;
		/// <exclude/>
		protected readonly BindInputElement _bindInput;
		/// <exclude/>
		protected readonly ToolTip warningToolTip;

		private readonly TextInput textInput;
		private readonly TextCaret caret;
		private readonly SelectionBox selectionBox;
		private bool canHighlight, isHighlighting, allowInput, textUpdatePending;
		private Vector2I lastCaretIndex;

		/// <exclude/>
		protected static readonly Vector2I caretMin = new Vector2I(0, -1);

		public TextBox(HudParentBase parent) : base(parent)
		{
			FocusHandler = new InputFocusHandler(this)
			{
				GainedInputFocusCallback = GainFocus,
				LostInputFocusCallback = LoseFocus
			};
			_mouseInput = new MouseInputElement(this)
			{
				ShareCursor = true,
				ZOffset = 1,
				LeftClickedCallback = ClearSelection
			};
			_bindInput = new BindInputElement(this)
			{
				InputPredicate = GetCanAllowInput,
				InputFilterFlags = SeBlacklistModes.Chat,
				CollectionInitializer = 
				{
                    { SharedBinds.Copy, CopyText },
					{ SharedBinds.Cut, CutText },
					{ SharedBinds.Paste, PasteText },
					{ SharedBinds.SelectAll, SelectAllText },
					{ SharedBinds.Escape, ClearSelection }
                }
			};

			MouseInput = _mouseInput;
			BindInput = _bindInput;
			textInput = new TextInput(AddChar, RemoveLastChar, TextInputFilter);
			caret = new TextCaret(this) { Visible = false };
			selectionBox = new SelectionBox(caret, this) { Color = new Color(255, 255, 255, 140) };

			warningToolTip = new ToolTip()
			{
				text = "Open Chat to Enable Text Editing",
				bgColor = ToolTip.OrangeWarningBG
			};

			TextBoard.TextChanged += HandleTextChange;

			EnableEditing = true;
			EnableHighlighting = true;
			NewLineChar = '\n';

			MoveToEndOnGainFocus = false;
			ClearSelectionOnLoseFocus = true;
			Size = new Vector2(60f, 200f);
		}

		public TextBox() : this(null)
		{ }

		/// <summary>
		/// Opens the textbox for input and moves the caret to the end.
		/// </summary>
		public void OpenInput()
		{
			allowInput = true;
			UpdateInputOpen();
			caret.SetPosition(0);
			caret.SetPosition(int.MaxValue);
			textUpdatePending = false;
		}

		/// <summary>
		/// Closes textbox input and clears the text selection.
		/// </summary>
		public void CloseInput()
		{
			allowInput = false;
			UpdateInputOpen();
			selectionBox.ClearSelection();
		}

		/// <summary>
		/// Highlights the range of text specified.
		/// </summary>
		public void SetSelection(Vector2I start, Vector2I end) =>
			selectionBox.SetSelection(start, end);

		/// <summary>
		/// Clears selected text range.
		/// </summary>
		public void ClearSelection()
		{
            selectionBox.ClearSelection();
            isHighlighting = false;
		}

		/// <summary>
		/// Marks a text change as pending invocation of the TextChanged event.
		/// </summary>
		protected virtual void HandleTextChange()
		{
			if (ValueChanged != null)
				textUpdatePending = true;
		}

		/// <summary>
		/// Determines whether or not the given character is within the accepted range for input.
		/// </summary>
		private bool TextInputFilter(char ch)
		{
			if (CharFilterFunc == null)
				return ch >= ' ' || ch == '\n' || ch == '\t';
			else
				return CharFilterFunc(ch) && (ch >= ' ' || ch == '\n');
		}

		/// <summary>
		/// Handles gaining input focus. Moves caret to end if configured to do so.
		/// </summary>
		protected virtual void GainFocus(object sender, EventArgs args)
		{
			if (MoveToEndOnGainFocus)
				caret.SetPosition(int.MaxValue);
		}

		/// <summary>
		/// Handles losing input focus. Clears selection if configured to do so.
		/// </summary>
		protected virtual void LoseFocus(object sender, EventArgs args)
		{
			if (ClearSelectionOnLoseFocus)
				ClearSelection();
		}

		/// <summary>
		/// Copies the currently selected text to the clipboard if input is allowed and highlighting is enabled.
		/// </summary>
		protected virtual void CopyText(object sender, EventArgs args)
		{
			if (EnableHighlighting && !selectionBox.Empty)
			{
				HudMain.ClipBoard = TextBoard.GetTextRange(selectionBox.Start, selectionBox.End);
			}
		}

		/// <summary>
		/// Cuts the currently selected text to the clipboard if input is allowed, editing is enabled, and highlighting is enabled.
		/// Deletes the selection and adjusts the caret position accordingly.
		/// </summary>
		protected virtual void CutText(object sender, EventArgs args)
		{
			if (EnableEditing && !selectionBox.Empty && EnableHighlighting)
			{
				RichText text = TextBoard.GetTextRange(selectionBox.Start, selectionBox.End);
				DeleteSelection();
				caret.Move(new Vector2I(0, -GetRichTextMinLength(text)));
				HudMain.ClipBoard = text;
			}
		}

		/// <summary>
		/// Pastes the clipboard text at the caret position if input is allowed and editing is enabled.
		/// Replaces any existing selection and adjusts the caret position.
		/// </summary>
		protected virtual void PasteText(object sender, EventArgs args)
		{
			if (EnableEditing)
			{
				if (HudMain.ClipBoard != null)
				{
					Vector2I insertIndex = caret.CaretIndex + new Vector2I(0, 1);
					insertIndex.X = MathHelper.Clamp(insertIndex.X, 0, TextBoard.Count);

					DeleteSelection();
					TextBoard.Insert(HudMain.ClipBoard, insertIndex);
					int length = GetRichTextMinLength(HudMain.ClipBoard);

					if (caret.CaretIndex.Y == -1)
						length++;

					caret.Move(new Vector2I(0, length));
				}
			}
		}

		/// <summary>
		/// Selects all text in the textbox if input is allowed and highlighting is enabled.
		/// Moves the caret to the end and sets the full text range as the selection.
		/// </summary>
		protected virtual void SelectAllText(object sender, EventArgs args)
		{
			if (EnableHighlighting)
			{
				caret.SetPosition(int.MaxValue);
				lastCaretIndex = caret.CaretIndex;
				selectionBox.SetSelection(new Vector2I(0, -1), new Vector2I(TextBoard.Count - 1, TextBoard[TextBoard.Count - 1].Count - 1));
				isHighlighting = true;
			}
		}

		/// <summary>
		/// Handles clearing selection on left click or escape
		/// </summary>
		protected virtual void ClearSelection(object sender, EventArgs args)
		{
			if (EnableHighlighting)
				ClearSelection();
		}

		/// <summary>
		/// Inserts the given character to the right of the caret.
		/// </summary>
		private void AddChar(char ch)
		{
			ch = (ch == NewLineChar) ? '\n' : ch;

			if (isHighlighting)
				DeleteSelection();
			else
				ClearSelection();

			TextBoard.Insert(ch, caret.CaretIndex + new Vector2I(0, 1));
			caret.Move(new Vector2I(0, 1));
		}

		/// <summary>
		/// Removes the character immediately preceeding the caret.
		/// </summary>
		private void RemoveLastChar()
		{
			if (TextBoard.Count > 0 && TextBoard[caret.CaretIndex.X].Count > 0 && caret.CaretIndex != caretMin)
			{
                if (isHighlighting)
					DeleteSelection();
				else
				{
					ClearSelection();

                    if (caret.CaretIndex.Y >= 0)
                        TextBoard.RemoveAt(ClampIndex(caret.CaretIndex, TextBoard));

					caret.Move(new Vector2I(0, -1));
				}
			}
		}

		/// <summary>
		/// Removes the text currently highlighted from the textbox.
		/// </summary>
		private void DeleteSelection()
		{
			if (!selectionBox.Empty)
				TextBoard.RemoveRange(selectionBox.Start, selectionBox.End);

            ClearSelection();
        }

		private bool GetCanAllowInput() =>
			(allowInput || (FocusHandler.HasFocus && HudMain.InputMode == HudInputMode.Full));

		/// <summary>
		/// Updates the InputOpen state based on allowInput, focus, and enabled features.
		/// </summary>
		private void UpdateInputOpen() =>
			InputOpen = GetCanAllowInput() && (EnableHighlighting || EnableEditing);

		/// <exclude/>
		protected override void HandleInput(Vector2 cursorPos)
		{
			bool useInput = GetCanAllowInput();

			if (EnableEditing && MouseInput.IsMousedOver && HudMain.InputMode == HudInputMode.CursorOnly)
				HudMain.Cursor.RegisterToolTip(warningToolTip);

			// Editing
			if (useInput && EnableEditing)
                textInput.HandleInput();

			UpdateInputOpen();
			caret.Visible = InputOpen;

			// Copy and highlighting
			if (useInput && EnableHighlighting)
			{
				// Highlighting state
				if (caret.IsNavigating)
				{
					// Determine whether highlighting can start
					if ((!MouseInput.IsNewLeftClicked && MouseInput.IsLeftClicked) || SharedBinds.Shift.IsPressed)
						canHighlight = true;
					else
						canHighlight = false;

					// Track highlighted range
					if (canHighlight || isHighlighting)
						selectionBox.UpdateSelection();

					// Incomplete selections need to be tracked, but not highlighted
					if (isHighlighting && selectionBox.Start.X == selectionBox.End.X)
						isHighlighting = selectionBox.End.Y >= selectionBox.Start.Y;

					// Start highlighting
					if (!isHighlighting && lastCaretIndex != caret.CaretIndex)
						isHighlighting = canHighlight;
				}
			}
			else
				canHighlight = false;

			// Stop highlighting
			if (!canHighlight && lastCaretIndex != caret.CaretIndex)
				ClearSelection();

			lastCaretIndex = caret.CaretIndex;
			selectionBox.Visible = isHighlighting;

			if (!InputOpen && textUpdatePending)
			{
				ValueChanged?.Invoke(FocusHandler?.InputOwner, EventArgs.Empty);
				textUpdatePending = false;
			}
		}

		/// <summary>
		/// Clamps the given index within the range of existing characters.
		/// </summary>
		private static Vector2I ClampIndex(Vector2I index, ITextBuilder text)
		{
			if (text.Count > 0)
			{
				index.X = MathHelper.Clamp(index.X, 0, text.Count - 1);
				index.Y = MathHelper.Clamp(index.Y, 0, text[index.X].Count - 1);

				return index;
			}
			else
				return Vector2I.Zero;
		}

		private static int GetRichTextMinLength(RichText text)
		{
			int length = 0;

			for (int n = 0; n < text.apiData.Count; n++)
				length += text.apiData[n].Item1.Length;

			return length;
		}

		private class TextCaret : TexturedBox
		{
			/// <summary>
			/// Index of the character currently selected by the caret. When Y == -1, that means
			/// the caret is positioned to the left of the first character in the line.
			/// </summary>
			public Vector2I CaretIndex { get; private set; }

			/// <summary>
			/// Determines whether the caret will be visible
			/// </summary>
			public bool ShowCaret { get; set; }

			/// <summary>
			/// True if the caret is being moved for navigation. False for insertions.
			/// </summary>
			public bool IsNavigating { get; private set; }

			private readonly TextBox textElement;
			private readonly ITextBoard text;
			private readonly Stopwatch blinkTimer;
			private bool blink;
			private int caretOffset;
			private Vector2 lastCursorPos;

			public TextCaret(TextBox textElement) : base(textElement)
			{
				this.textElement = textElement;
				text = textElement.TextBoard;
				Size = new Vector2(1f, 16f);
				Color = new Color(240, 240, 230);

				blinkTimer = new Stopwatch();
				blinkTimer.Start();
			}

			/// <summary>
			/// Moves the caret in the direction indicated by the vector. The caret will automatically
			/// wrap to the last/next line if movement in the Y direction would result in the index
			/// going out of range.
			/// </summary>
			/// <param name="dir">Index direction vector</param>
			public void Move(Vector2I dir, bool navigate = false)
			{
				bool moveLeft = dir.Y < 0,
					moveRight = dir.Y > 0;

				if (CaretIndex == caretMin && moveLeft || (text.Count == 0 || text.Count == 1 && text[0].Count == 0))
					return;

				IRichChar ch = text[ClampIndex(CaretIndex, text)];
				Vector2I newIndex;

				bool isPrepending = CaretIndex.Y == -1,
					isPrependStarting = CaretIndex.Y == 0 && (moveLeft && (navigate || CaretIndex.X == 0)) && ch.Ch != '\n';

				// Start prepend. If at start of line set -1 special case
				if (isPrependStarting || (dir.Y == 0 && isPrepending))
				{
					newIndex = CaretIndex + new Vector2I(dir.X, 0);
					newIndex.Y = -1;

					newIndex = ClampCaret(newIndex);
					caretOffset = GetOffsetFromIndex(new Vector2I(newIndex.X, 0));
				}
				else
				{
					int newOffset = Math.Max(caretOffset + dir.Y, 0);

					// Stop prepending
					if ((isPrepending && moveRight) && (CaretIndex.X > 0 || text[0].Count > 1))
						newOffset -= 1;

					// Combine horz character and vertical offset
					newIndex = GetIndexFromOffset(newOffset) + new Vector2I(dir.X, 0);
					newIndex = ClampCaret(newIndex);
					// Get offset from combined index
					caretOffset = GetOffsetFromIndex(newIndex);

					// Start prepending on first char on move to next line, unless it's a manual break
					ch = text[ClampIndex(newIndex, text)];

					if (navigate && moveRight && newIndex.X > CaretIndex.X && ch.Ch != '\n')
						newIndex.Y = -1;
				}

				CaretIndex = newIndex;

				if (CaretIndex.Y >= 0)
					text.MoveToChar(CaretIndex);
				else
					text.MoveToChar(CaretIndex + new Vector2I(0, 1));

				blink = true;
				blinkTimer.Restart();

				IsNavigating = navigate;
			}

			/// <summary>
			/// Sets the caret position to the specified index, clamping as needed.
			/// </summary>
			public void SetPosition(Vector2I index)
			{
				CaretIndex = ClampCaret(index);
				caretOffset = Math.Max(GetOffsetFromIndex(CaretIndex), 0);
				text.MoveToChar(CaretIndex);
			}

			/// <summary>
			/// Sets the caret position based on the total character offset from the start of the text.
			/// </summary>
			public void SetPosition(int offset) =>
                SetPosition(GetIndexFromOffset(offset));

            protected override void Draw()
			{
				if (ShowCaret)
				{
					bool isCharVisible = text.Count == 0 || text[0].Count == 0;
					CaretIndex = ClampCaret(CaretIndex);

					// If line visible
					if ((text.Count > 0 && text[0].Count > 0) &&
						(CaretIndex.X >= text.VisibleLineRange.X && CaretIndex.X <= text.VisibleLineRange.Y))
					{
						Vector2I index = Vector2I.Max(CaretIndex, Vector2I.Zero);

						// Calculate visibilty on line
						IRichChar ch = text[index];
						Vector2 size = ch.Size,
							pos = ch.Offset + text.TextOffset;
						BoundingBox2 textBounds = BoundingBox2.CreateFromHalfExtent(Vector2.Zero, .5f * text.Size),
							charBounds = BoundingBox2.CreateFromHalfExtent(pos, .5f * Vector2.Max(size, new Vector2(8f)));

						if (textBounds.Contains(charBounds) != ContainmentType.Disjoint)
							isCharVisible = true;
					}

					if (blink & isCharVisible)
					{
						UpdateOffset();
						base.Draw();
					}

					if (blinkTimer.ElapsedMilliseconds > 500)
					{
						blink = !blink;
						blinkTimer.Restart();
					}
				}
			}

			/// <summary>
			/// Updates the position of the caret to match that of the currently selected character.
			/// </summary>
			private void UpdateOffset()
			{
				Vector2 offset = new Vector2();
				Vector2I index = Vector2I.Max(CaretIndex, Vector2I.Zero);

				if (text.Count > 0 && text[index.X].Count > 0)
				{
					IRichChar ch;
					Height = text[index.X].Size.Y - 2f;
					ch = text[index];

					if (CaretIndex.Y == -1)
					{
						offset = ch.Offset + text.TextOffset;
						offset.X -= ch.Size.X * .5f + 1f;
					}
					else
					{
						offset = ch.Offset + text.TextOffset;
						offset.X += ch.Size.X * .5f + 1f;
					}
				}
				else
				{
					if (text.Format.Alignment == TextAlignment.Left)
						offset.X = -textElement.Size.X * .5f + 2f;
					else if (text.Format.Alignment == TextAlignment.Right)
						offset.X = textElement.Size.X * .5f - 2f;

					var parentFull = Parent as HudElementBase;
					offset += parentFull.Padding * .5f;

					if (!text.VertCenterText)
						offset.Y = (text.Size.Y - Height) * .5f - 4f;
				}

				Offset = offset;
			}

			/// <summary>
			/// Handles input for moving the caret via arrow keys or mouse clicks.
			/// </summary>
			protected override void HandleInput(Vector2 cursorPos)
			{
				if (SharedBinds.DownArrow.IsPressedAndHeld || SharedBinds.DownArrow.IsNewPressed)
					Move(new Vector2I(1, 0), true);

				if (SharedBinds.UpArrow.IsPressedAndHeld || SharedBinds.UpArrow.IsNewPressed)
					Move(new Vector2I(-1, 0), true);

				if (SharedBinds.RightArrow.IsPressedAndHeld || SharedBinds.RightArrow.IsNewPressed)
					Move(new Vector2I(0, 1), true);

				if (SharedBinds.LeftArrow.IsPressedAndHeld || SharedBinds.LeftArrow.IsNewPressed)
					Move(new Vector2I(0, -1), true);

				if (textElement.MouseInput.IsLeftClicked)
					GetClickedChar(cursorPos);
			}

			/// <summary>
			/// Sets the index of the caret to that of the character closest to the cursor.
			/// </summary>
			private void GetClickedChar(Vector2 cursorPos)
			{
				if ((cursorPos - lastCursorPos).LengthSquared() > 4f)
				{
					CaretIndex = ClampCaret(CaretIndex);

					Vector2 offset = cursorPos - textElement.Position;
					Vector2I index = Vector2I.Max(CaretIndex, Vector2I.Zero),
						newIndex = text.GetCharAtOffset(offset);

					if (text.Count > newIndex.X && text[newIndex.X].Count > newIndex.Y)
					{
						IRichChar clickedCh = text[newIndex];

						if (offset.X <= clickedCh.Offset.X)
							newIndex -= new Vector2I(0, 1);

						CaretIndex = ClampCaret(newIndex);
						caretOffset = GetOffsetFromIndex(CaretIndex);
						lastCursorPos = cursorPos;

						blink = true;
						blinkTimer.Restart();
						IsNavigating = true;
					}
				}
			}

			/// <summary>
			/// Clamps the given index within the range of existing characters.
			/// </summary>
			private Vector2I ClampCaret(Vector2I index)
			{
				if (text.Count > 0)
				{
					index.X = MathHelper.Clamp(index.X, 0, text.Count - 1);
					index.Y = MathHelper.Clamp(index.Y, -1, text[index.X].Count - 1);

					if (index.Y == -1 && text[index.X].Count > 0 && text[index.X][0].Ch == '\n')
						index.Y = 0;

					return index;
				}
				else
					return Vector2I.Zero;
			}

			/// <summary>
			/// Returns the total number of characters between the start of the text and the current index.
			/// </summary>
			private int GetOffsetFromIndex(Vector2I index)
			{
				int offset = 0;

				for (int line = 0; line < index.X; line++)
				{
					offset += text[line].Count;
				}

				offset += index.Y;
				return Math.Max(offset, 0);
			}

			/// <summary>
			/// Calculates the index with given the number of characters between it and the beginning of the
			/// text.
			/// </summary>
			private Vector2I GetIndexFromOffset(int offset)
			{
				Vector2I index = Vector2I.Zero;
				int charCount = 0;

				for (int line = 0; line < text.Count; line++)
					charCount += text[line].Count;

				offset = Math.Min(offset, charCount - 1);

				for (int line = 0; line < text.Count; line++)
				{
					int lineLength = text[line].Count;

					if (offset < lineLength)
					{
						index.Y = offset;
						break;
					}
					else
					{
						offset -= lineLength;
						index.X++;
					}
				}

				return index;
			}
		}

		private class SelectionBox : HudElementBase
		{
			/// <summary>
			/// Color of the selection box
			/// </summary>
			public Color Color { get { return highlightBoard.Color; } set { highlightBoard.Color = value; } }

			/// <summary>
			/// Index of the first character in the selection.
			/// </summary>
			public Vector2I Start { get; private set; }

			/// <summary>
			/// Index of the last character in the selection.
			/// </summary>
			public Vector2I End { get; private set; }

			/// <summary>
			/// If true, then the current selection is empty.
			/// </summary>
			public bool Empty => (Start == -Vector2I.One || End == -Vector2I.One);

			private readonly TextCaret caret;
			private readonly ITextBoard text;
			private readonly MatBoard highlightBoard;
			private readonly List<HighlightBox> highlightList;
			private Vector2I selectionAnchor;

			public SelectionBox(TextCaret caret, Label parent) : base(parent)
			{
				text = parent.TextBoard;
				this.caret = caret;

				Start = -Vector2I.One;
				selectionAnchor = -Vector2I.One;
				highlightBoard = new MatBoard();
				highlightList = new List<HighlightBox>();

				text.TextChanged += ClearSelection;
			}

			/// <summary>
			/// Sets the start and end indices for the text selection.
			/// </summary>
			public void SetSelection(Vector2I start, Vector2I end)
			{
				Start = start;
				End = end;
				selectionAnchor = start;
			}

			/// <summary>
			/// Clears the current text selection and resets indices.
			/// </summary>
			public void ClearSelection()
			{
				Start = -Vector2I.One;
				End = -Vector2I.One;
				selectionAnchor = -Vector2I.One;
				highlightList.Clear();
			}

			/// <summary>
			/// Updates the selection range based on the current caret position and anchor.
			/// Ensures the start is before the end and adjusts for line boundaries.
			/// </summary>
			public void UpdateSelection()
			{
				if (text.Count > 0)
				{
					Vector2I caretIndex = caret.CaretIndex;
					bool wasSelecting = selectionAnchor != -Vector2I.One;

                    // Set anchor on new selection
                    if (!wasSelecting)
                        selectionAnchor = caretIndex;

					bool isAfterAnchor;

					if (caretIndex.X < selectionAnchor.X)
						isAfterAnchor = false;
					else if (caretIndex.X > selectionAnchor.X)
						isAfterAnchor = true;
					else // Same line
						isAfterAnchor = (caretIndex.Y >= selectionAnchor.Y);

					// If the caret is after the anchor, anchor Start
					if (isAfterAnchor)
					{
                        Start = selectionAnchor;
						End = caretIndex;
                    }
					else
					{
                        Start = caretIndex;
						End = selectionAnchor;                       
                    }

                    if (Start.Y < text[Start.X].Count - 1)
                        Start += new Vector2I(0, 1);

                    Start = ClampIndex(Start, text);
					End = ClampIndex(End, text);
				}

                else
				{
					Start = -Vector2I.One;
					End = -Vector2I.One;
					selectionAnchor = -Vector2I.One;
				}
			}

			protected override void Draw()
			{
				if (!Empty)
				{
					UpdateHighlight();

					Vector2 highlightOffset = Origin + text.TextOffset;
					BoundingBox2 bounds = new BoundingBox2(-text.Size * .5f, text.Size * .5f);
					bounds.Translate(Origin + Offset);

					for (int n = 0; n < highlightList.Count; n++)
						highlightList[n].Draw(highlightBoard, highlightOffset, bounds, HudSpace.PlaneToWorldRef);
				}
			}

			/// <summary>
			/// Calculates the size and offsets for the boxes highlighting the selection.
			/// </summary>
			private void UpdateHighlight()
			{
				highlightList.Clear();

				// Clamp range
				Vector2I lineRange = text.VisibleLineRange;
				Start = ClampIndex(Start, text);
				End = ClampIndex(End, text);

				int startLine = Math.Max(Start.X, lineRange.X),
					endLine = Math.Min(End.X, lineRange.Y);

				// Add start and end
				if (Start.X == End.X && Start.X == startLine)
					AddHighlightBox(Start.X, Start.Y, End.Y);
				else
				{
					for (int line = startLine; line <= endLine; line++)
					{
						if (line == Start.X)
							AddHighlightBox(Start.X, Start.Y, text[Start.X].Count - 1); // Top
						else if (line == End.X)
							AddHighlightBox(End.X, 0, End.Y); // Bottom
						else
							AddHighlightBox(line, 0, text[line].Count - 1); // Middle
					}
				}

				if (highlightList.Count > 10 && highlightList.Capacity > 3 * highlightList.Count)
					highlightList.TrimExcess();
			}

			/// <summary>
			/// Adds an appropriately sized highlight box for the range of characters on the given line.
			/// Does not take into account text clipping or text offset.
			/// </summary>
			private void AddHighlightBox(int lineIdx, int startCh, int endCh)
			{
				var line = text[lineIdx];
				if (line.Count == 0) return;

				// Clamp
				startCh = Math.Max(0, Math.Min(startCh, line.Count - 1));
				endCh = Math.Min(endCh, line.Count - 1);

				IRichChar startChar = line[startCh],
					endChar = line[endCh],
					prevChar = startCh > 0 ? line[startCh - 1] : null;

				// Left bound: Max of (start char left, prev char right)
				float startLeft = startChar.Offset.X - 0.5f * startChar.Size.X;

				if (prevChar != null)
				{
					float prevRight = prevChar.Offset.X + 0.5f * prevChar.Size.X;
					startLeft = Math.Max(startLeft, prevRight);
				}

				// Right bound: End char's right edge
				float endRight = endChar.Offset.X + 0.5f * endChar.Size.X;

				// Final box
				float width = Math.Max(endRight - startLeft, 1f);
				float centerX = startLeft + width * 0.5f;
				float centerY = line.VerticalOffset - line.Size.Y * 0.5f;

				var box = new HighlightBox
				{
					size = new Vector2(width, line.Size.Y),
					offset = new Vector2(centerX, centerY)
				};

				highlightList.Add(box);
			}

			private struct HighlightBox
			{
				public Vector2 size, offset;

				public void Draw(MatBoard matBoard, Vector2 highlightOffset, BoundingBox2 tbBounds, MatrixD[] matrixRef)
				{
					CroppedBox box = default(CroppedBox);
					Vector2 highlightPos = highlightOffset + offset,
						halfSize = 0.5f * size;

					box.bounds = new BoundingBox2(highlightPos - halfSize, highlightPos + halfSize);
					box.mask = tbBounds;

					matBoard.Draw(ref box, matrixRef);
				}
			}
		}
	}
}