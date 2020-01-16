using RichHudFramework.UI.Rendering;
using Sandbox.ModAPI;
using System;
using System.Text;
using VRageMath;

namespace RichHudFramework.UI
{
    using Client;
    using Server;

    /// <summary>
    /// Clickable text box.
    /// </summary>
    public class TextBox : Label
    {
        /// <summary>
        /// Determines whether or not this element will accept input from the mouse.
        /// </summary>
        public bool UseMouseInput { get { return ShareCursor; } set { ShareCursor = value; } }

        /// <summary>
        /// Determines whether or not text can be entered.
        /// </summary>
        public bool InputOpen { get; set; }

        /// <summary>
        /// Used to restrict the range of characters allowed for input.
        /// </summary>
        public Func<char, bool> CharFilterFunc { get; set; }
        public readonly ClickableElement mouseInput;

        private bool acceptInput;
        private readonly TextInput textInput;
        private readonly TextCaret caret;
        private readonly SelectionBox selectionBox;

        public TextBox(IHudParent parent = null) : base(parent)
        {
            CaptureCursor = true;
            ShareCursor = true;

            mouseInput = new ClickableElement(this) { DimAlignment = DimAlignments.Both | DimAlignments.IgnorePadding };
            textInput = new TextInput(AddChar, RemoveLastChar, TextInputFilter);

            caret = new TextCaret(this) { Visible = false };
            selectionBox = new SelectionBox(caret, this) { Color = new Color(255, 255, 255, 140), Visible = false };
        }

        public void OpenInput()
        {
            InputOpen = true;
            caret.Move(new Vector2I(100000), true);
        }

        public void CloseInput()
        {
            InputOpen = false;
            selectionBox.ClearSelection();
        }

        /// <summary>
        /// Determines whether or not the given character is within the accepted range for input.
        /// </summary>
        private bool TextInputFilter(char ch)
        {
            if (CharFilterFunc == null)
                return ch >= ' ' || ch == '\n';
            else
                return CharFilterFunc(ch) && (ch >= ' ' || ch == '\n');
        }

        protected override void HandleInput()
        {
            acceptInput = (UseMouseInput && mouseInput.HasFocus && HudMain.Cursor.Visible) || InputOpen;

            if (acceptInput)
            {
                if (!selectionBox.Visible && !SharedBinds.LeftButton.IsPressed)
                    selectionBox.Visible = true;

                caret.Visible = true;
                textInput.HandleInput();

                if (SharedBinds.Copy.IsNewPressed && !selectionBox.Empty)
                    HudMain.ClipBoard = TextBoard.GetTextRange(selectionBox.Start, selectionBox.End);

                if (SharedBinds.Cut.IsNewPressed && !selectionBox.Empty)
                {
                    HudMain.ClipBoard = TextBoard.GetTextRange(selectionBox.Start, selectionBox.End);
                    DeleteSelection();
                    caret.Move(Vector2I.Zero);
                }

                if (SharedBinds.Paste.IsNewPressed)
                {
                    if (HudMain.ClipBoard != null)
                    {
                        DeleteSelection();
                        TextBoard.Insert(HudMain.ClipBoard, caret.Index + new Vector2I(0, 1));

                        int count = 0;

                        for (int n = 0; n < HudMain.ClipBoard.Count; n++)
                            count += HudMain.ClipBoard[n].Length;

                        caret.Move(new Vector2I(0, count));
                    }
                }
            }
            else if (caret.Visible || selectionBox.Visible)
            {
                caret.Visible = false;
                selectionBox.Visible = false;
            }
        }

        /// <summary>
        /// Inserts the given character to the right of the caret.
        /// </summary>
        private void AddChar(char ch)
        {
            DeleteSelection();
            TextBoard.Insert(ch.ToString(), caret.Index + new Vector2I(0, caret.Prepend ? 0 : 1));
            caret.Move(new Vector2I(0, 1), true);
        }

        /// <summary>
        /// Removes the character immediately preceeding the caret.
        /// </summary>
        private void RemoveLastChar()
        {
            if (TextBoard.Count > 0 && TextBoard[caret.Index.X].Count > 0)
            {
                DeleteSelection();

                if (!(caret.Prepend && caret.Index.Y == 0))
                {
                    TextBoard.RemoveAt(caret.Index - new Vector2I(0, caret.Prepend ? 1 : 0));
                    caret.Move(new Vector2I(0, -1), true);
                }
            }
        }

        /// <summary>
        /// Removes the text currently highlighted from the textbox.
        /// </summary>
        private void DeleteSelection()
        {
            if (!selectionBox.Empty)
            {
                TextBoard.RemoveRange(selectionBox.Start, selectionBox.End);
                selectionBox.ClearSelection();
            }
        }

        private class TextCaret : TexturedBox
        {
            /// <summary>
            /// Index of the character currently selected by the caret.
            /// </summary>
            public Vector2I Index { get; private set; }

            /// <summary>
            /// Indicates whether or not text should be inserted before or after the current index.
            /// </summary>
            public bool Prepend { get; private set; }

            private readonly Label textElement;
            private readonly ITextBoard text;
            private readonly Utils.Stopwatch blinkTimer;
            private bool blink;
            private int caretOffset;
            private Vector2 lastCursorPos;

            public TextCaret(Label textElement) : base(textElement)
            {
                this.textElement = textElement;
                text = textElement.TextBoard;
                Size = new Vector2(1f, 16f);
                Color = new Color(240, 240, 230);

                blinkTimer = new Utils.Stopwatch();
                blinkTimer.Start();
            }

            /// <summary>
            /// Moves the caret in the direction indicated by the vector. The caret will automatically
            /// wrap to the last/next line if movement in the Y direction would result in the index
            /// going out of range.
            /// </summary>
            /// <param name="dir">Index direction vector</param>
            public void Move(Vector2I dir, bool ignorePrepend = false)
            {
                Vector2I lastIndex = new Vector2I();

                if (text.Count > 0 && text[text.Count - 1].Count > 0)
                    lastIndex = new Vector2I(text.Count - 1, text[text.Count - 1].Count - 1);

                if (dir.Y != 0 && Index != lastIndex && !ignorePrepend)
                {
                    bool prependNext = dir.Y < 0;

                    if ((!Prepend && dir.Y < 0) || (Prepend && dir.Y > 0))
                        dir.Y = 0;

                    Prepend = prependNext;
                }
                else if (dir.X == 0)
                    Prepend = false;

                int newOffset = Math.Max(caretOffset + dir.Y, 0);
                Vector2I newIndex = GetIndexFromOffset(newOffset) + new Vector2I(dir.X, 0);
                newIndex = ClampIndex(newIndex);

                if (dir.X == 0 && dir.Y != 0 && newIndex.X != Index.X && !ignorePrepend)
                {
                    if (newIndex.X < Index.X)
                        Prepend = false;
                    else
                        Prepend = true;
                }

                Index = newIndex;
                caretOffset = GetOffsetFromIndex(Index);
                text.MoveToChar(Index);

                blink = true;
                blinkTimer.Reset();
            }

            protected override void Draw()
            {
                if (blink)
                    base.Draw();

                if (blinkTimer.ElapsedMilliseconds > 500)
                {
                    blink = !blink;
                    blinkTimer.Reset();
                }

                Index = ClampIndex(Index);
                UpdateOffset();
            }

            /// <summary>
            /// Updates the position of the caret to match that of the currently selected character.
            /// </summary>
            private void UpdateOffset()
            {
                Vector2 offset = new Vector2();

                if (text.Count > 0 && text[Index.X].Count > 0)
                {
                    IRichChar ch = text[Index];
                    Height = text[Index.X].Size.Y - 2f;
                    offset = text[Index].Offset;

                    // If prepending, then draw the caret on the left side
                    if (Prepend)
                        offset.X -= ch.Size.X / 2f + 1f;
                    else
                        offset.X += ch.Size.X / 2f + 1f;
                }
                else
                {
                    if (text.Format.Alignment == TextAlignment.Left)
                        offset.X = -textElement.Size.X / 2f + 2f;
                    else if (text.Format.Alignment == TextAlignment.Right)
                        offset.X = textElement.Size.X / 2f - 2f;

                    offset.X += Padding.X / 2f;
                }

                Offset = offset;
            }

            /// <summary>
            /// Handles input for moving the caret.
            /// </summary>
            protected override void HandleInput()
            {
                if (SharedBinds.DownArrow.IsPressedAndHeld)
                    Move(new Vector2I(1, 0));

                if (SharedBinds.UpArrow.IsPressedAndHeld)
                    Move(new Vector2I(-1, 0));

                if (SharedBinds.RightArrow.IsPressedAndHeld)
                    Move(new Vector2I(0, 1));

                if (SharedBinds.LeftArrow.IsPressedAndHeld)
                    Move(new Vector2I(0, -1));

                if (SharedBinds.LeftButton.IsPressed)
                    GetClickedChar();
                else if (SharedBinds.LeftButton.IsReleased)
                    lastCursorPos = Vector2.PositiveInfinity;
            }

            /// <summary>
            /// Sets the index of the caret to that of the character closest to the cursor.
            /// </summary>
            private void GetClickedChar()
            {
                if (HudMain.Cursor.Origin != lastCursorPos)
                {
                    Vector2 offset = HudMain.Cursor.Origin - textElement.Position;
                    Index = ClampIndex(text.GetCharAtOffset(offset));
                    caretOffset = GetOffsetFromIndex(Index);

                    if (text.Count > 0 && text[Index.X].Count > 0 && offset.X < text[Index].Offset.X)
                        Prepend = true;
                    else
                        Prepend = false;

                    lastCursorPos = HudMain.Cursor.Origin;
                }
            }

            /// <summary>
            /// Clamps the given index within the range of existing characters.
            /// </summary>
            private Vector2I ClampIndex(Vector2I index)
            {
                if (text.Count > 0)
                {
                    index.X = Utils.Math.Clamp(index.X, 0, text.Count - 1);
                    index.Y = Utils.Math.Clamp(index.Y, 0, text[index.X].Count - 1);

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
                return offset;
            }

            /// <summary>
            /// Calculates the index with given the number of characters between it and the beginning of the
            /// text.
            /// </summary>
            private Vector2I GetIndexFromOffset(int offset)
            {
                Vector2I index = new Vector2I();

                for (int line = 0; line < text.Count; line++)
                {
                    if (offset < text[line].Count)
                    {
                        index.Y = offset;
                        break;
                    }
                    else
                    {
                        if (index.X < text.Count - 1)
                        {
                            offset -= text[line].Count;
                            index.X++;
                        }
                        else
                        {
                            index.Y = text[line].Count - 1;
                            break;
                        }
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
            public bool Empty => top == null || Start == End;

            private readonly TextCaret caret;
            private readonly ITextBoard text;
            private readonly MatBoard highlightBoard;

            private HighlightBox top, middle, bottom;

            public SelectionBox(TextCaret caret, Label parent) : base(parent)
            {
                highlightBoard = new MatBoard();
                text = parent.TextBoard;
                this.caret = caret;
                Start = -Vector2I.One;
            }

            public void ClearSelection()
            {
                top = null;
                Start = -Vector2I.One;
                End = -Vector2I.One;
            }

            protected override void HandleInput()
            {
                if (text.Count > 0 && text[text.Count - 1].Count > 0)
                {
                    if (SharedBinds.LeftButton.IsNewPressed)
                    {
                        ClearSelection();
                    }
                    else if (SharedBinds.LeftButton.IsPressed)
                    {
                        if (Start == -Vector2I.One)
                        {
                            Start = caret.Index;
                            End = Start;
                        }
                        else
                        {
                            if (caret.Index.X < End.X || (caret.Index.X == End.X && caret.Index.Y < End.Y))
                            {
                                if (caret.Index.X > Start.X || (caret.Index.X == Start.X && caret.Index.Y > Start.Y))
                                    End = caret.Index;
                                else
                                    Start = caret.Index;
                            }
                            else
                                End = caret.Index;
                        }

                        if (Start != End)
                            UpdateSelection();
                    }
                    else if (SharedBinds.LeftButton.IsReleased || SharedBinds.Shift.IsReleased)
                    {
                        if (Empty)
                            ClearSelection();
                    }

                    if (SharedBinds.SelectAll.IsNewPressed)
                    {
                        Start = Vector2I.Zero;
                        End = new Vector2I(text.Count - 1, text[text.Count - 1].Count - 1);

                        UpdateSelection();
                    }

                    if (SharedBinds.Escape.IsNewPressed || SharedBinds.LeftArrow.IsNewPressed || SharedBinds.RightArrow.IsNewPressed || SharedBinds.UpArrow.IsNewPressed || SharedBinds.DownArrow.IsNewPressed)
                        ClearSelection();
                }
            }

            /// <summary>
            /// Calculates the size and offsets for the boxes highlighting the selection.
            /// </summary>
            private void UpdateSelection()
            {
                IRichChar left = text[Start],
                    right = text[End];

                if (End.X != Start.X)
                {
                    IRichChar firstLineEnd = text[Start.X][text[Start.X].Count - 1],
                        lastLineStart = text[End.X][0];

                    top = GetEndBar(left, firstLineEnd, (caret.Prepend && caret.Index == Start));
                    bottom = GetEndBar(lastLineStart, right, (caret.Prepend && caret.Index == End));

                    if (End.X - Start.X > 1)
                        middle = GetMiddleBar();
                    else
                        middle = null;
                }
                else
                {
                    top = GetEndBar(left, right, caret.Prepend);

                    middle = null;
                    bottom = null;
                }
            }

            /// <summary>
            /// Returns a highlight box that is appropriately sized or positioned to fit between
            /// two <see cref="IRichChar"/>s.
            /// </summary>
            private HighlightBox GetEndBar(IRichChar left, IRichChar right, bool prepend)
            {
                HighlightBox box = new HighlightBox();
                float leftBound = left.Offset.X,
                    rightBound = right.Offset.X + right.Size.X / 2f;

                if (prepend)
                    leftBound += left.Size.X / 2f;
                else
                    leftBound -= left.Size.X / 2f;

                box.size = new Vector2()
                {
                    X = Math.Min(rightBound - leftBound, text.Size.X),
                    Y = text[Start.X].Size.Y
                };

                box.offset = new Vector2()
                {
                    X = (rightBound + leftBound) / 2f,
                    Y = left.Offset.Y
                };

                return box;
            }

            /// <summary>
            /// Returns a highlight box that is sized and positioned to fit between the top and bottom
            /// highlight boxes.
            /// </summary>
            private HighlightBox GetMiddleBar()
            {
                HighlightBox box = new HighlightBox();
                float upperBound = top.offset.Y - top.size.Y / 2f,
                    lowerBound = bottom.offset.Y + bottom.size.Y / 2f;

                box.size = new Vector2()
                {
                    X = text.Size.X,
                    Y = upperBound - lowerBound
                };

                box.offset = new Vector2()
                {
                    X = 0f,
                    Y = (upperBound + lowerBound) / 2f
                };

                return box;
            }

            protected override void Draw()
            {
                if (!Empty)
                {
                    top.Draw(highlightBoard, Origin);

                    if (middle != null)
                        middle.Draw(highlightBoard, Origin);

                    if (bottom != null)
                        bottom.Draw(highlightBoard, Origin);
                }
            }

            private class HighlightBox
            {
                public Vector2 size, offset;

                public void Draw(MatBoard matBoard, Vector2 origin)
                {
                    matBoard.Size = size;
                    matBoard.offset = offset;
                    matBoard.Draw(origin);
                }
            }
        }
    }
}
