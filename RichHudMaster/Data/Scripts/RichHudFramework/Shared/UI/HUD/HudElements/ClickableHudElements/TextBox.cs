using DarkHelmet.UI.Rendering;
using System;
using System.Text;
using VRageMath;

namespace DarkHelmet.UI
{
    using Client;
    using Server;

    /// <summary>
    /// Clickable text box.
    /// </summary>
    public class TextBox : Label
    {
        public override float Width
        {
            get { return base.Width; }
            set
            {
                base.Width = value;
                mouseInput.Width = value;
            }
        }
        public override float Height
        {
            get { return base.Height; }
            set
            {
                base.Height = value;
                mouseInput.Height = value;
            }
        }
        public bool UseMouseInput { get { return mouseInput.Visible; } set { mouseInput.Visible = value; } }
        public bool InputOpen { get; set; }
        public Func<char, bool> IsCharAllowedFunc { get { return textInput.IsCharAllowedFunc; } set { textInput.IsCharAllowedFunc = value; } }

        public readonly ClickableElement mouseInput;

        private bool acceptInput;
        private readonly TextInput textInput;
        private readonly TextCaret caret;
        private readonly SelectionBox selectionBox;

        public TextBox(IHudParent parent = null, bool wordWrapping = false) : base(parent, wordWrapping)
        {
            mouseInput = new ClickableElement(this);
            textInput = new TextInput(AddChar, RemoveLastChar, TextInputFilter);
            
            caret = new TextCaret(this) { Visible = false };
            selectionBox = new SelectionBox(caret, this) { Color = new Color(255, 255, 255, 140), Visible = false };
        }

        private bool TextInputFilter(char ch)
        {
            if (IsCharAllowedFunc == null)
                return ch >= ' ' || ch == '\n';
            else
                return IsCharAllowedFunc(ch) && (ch >= ' ' || ch == '\n');
        }

        protected override void HandleInput()
        {
            acceptInput = (UseMouseInput && mouseInput.HasFocus) || InputOpen;

            if (acceptInput)
            {
                textInput.HandleInput();

                if (SharedBinds.Copy.IsNewPressed && !selectionBox.Empty)
                    HudMain.ClipBoard = Text.GetTextRange(selectionBox.Start, selectionBox.End);

                if (SharedBinds.Cut.IsNewPressed && !selectionBox.Empty)
                {
                    HudMain.ClipBoard = Text.GetTextRange(selectionBox.Start, selectionBox.End);
                    DeleteSelection();
                }

                if (SharedBinds.Paste.IsNewPressed)
                {
                    if (HudMain.ClipBoard != null)
                    {
                        DeleteSelection();
                        Text.Insert(HudMain.ClipBoard, caret.Position + new Vector2I(0, 1));

                        int count = 0;

                        for (int n = 0; n < HudMain.ClipBoard.Count; n++)
                            count += HudMain.ClipBoard[n].Length;

                        caret.Move(new Vector2I(0, count));
                    }
                }               
            }
        }

        private void DeleteSelection()
        {
            if (!selectionBox.Empty)
            {
                Text.RemoveRange(selectionBox.Start, selectionBox.End);
                selectionBox.ClearSelection();
            }
        }

        protected override void Draw()
        {
            base.Draw();

            if (acceptInput)
            {
                caret.Visible = true;
                selectionBox.Visible = true;
            }
            else if (caret.Visible || selectionBox.Visible)
            {
                caret.Visible = false;
                selectionBox.Visible = false;
            }
        }

        private void AddChar(char ch)
        {
            DeleteSelection();
            Text.Insert(ch.ToString(), caret.Position + new Vector2I(0, caret.Prepend ? 0 : 1));
            caret.Move(new Vector2I(0, 1));
        }

        private void RemoveLastChar()
        {
            if (Text.Count > 0 && Text[caret.Position.X].Count > 0)
            {
                DeleteSelection();

                if (caret.Prepend)
                {
                    if (caret.Position.X > 0 || caret.Position.Y > 0)
                    {
                        caret.Move(new Vector2I(0, -1), true);
                        Text.RemoveAt(caret.Position);
                    }
                }
                else
                {
                    Text.RemoveAt(caret.Position);
                    caret.Move(new Vector2I(0, -1), true);
                }
            }
        }

        protected class TextCaret : TexturedBox
        {
            public Vector2I Position { get; private set; }
            public bool Prepend { get; private set; }

            private readonly Label textElement;
            private readonly ITextBoard text;
            private readonly Utils.Stopwatch blinkTimer;
            private bool blink;
            private int caretOffset;

            public TextCaret(Label textElement) : base(textElement)
            {
                this.textElement = textElement;
                text = textElement.Text;
                Size = new Vector2(1f, 16f);
                Color = new Color(240, 240, 230);

                blinkTimer = new Utils.Stopwatch();
                blinkTimer.Start();
            }

            public void Move(Vector2I dir, bool ignorePrepend = false)
            {
                int newOffset = Math.Max(caretOffset + dir.Y, 0);
                Vector2I newIndex = GetIndexFromOffset(newOffset);
                newIndex.X = Utils.Math.Clamp(newIndex.X + dir.X, 0, text.Count - 1);

                if (!ignorePrepend && !Prepend && dir.Y < 0 && Position.Y == 0)
                    Prepend = true;
                else
                {
                    Position = ClampIndex(newIndex);
                    caretOffset = GetOffsetFromIndex(Position);
                    text.MoveToChar(Position);
                    Prepend = false;
                }

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

                Position = ClampIndex(Position);
                Vector2 offset = new Vector2();

                if (text.Count > 0 && text[Position.X].Count > 0)
                {
                    IRichChar ch = text[Position];
                    Height = text[Position.X].Size.Y - 2f;
                    offset = text[Position].Offset;

                    if (Prepend)
                        offset.X -= ch.Size.X / 2f + 1f;
                    else
                        offset.X += ch.Size.X / 2f + 1f;
                }
                else
                {
                    offset.Y = (textElement.Size.Y - Height) / 2f - 1f;

                    if (text.Format.Alignment == TextAlignment.Left)
                        offset.X = -textElement.Size.X / 2f + 2f;
                    else if (text.Format.Alignment == TextAlignment.Right)
                        offset.X = textElement.Size.X / 2f - 2f;
                }

                Offset = offset;
            }

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
            }

            private void GetClickedChar()
            {
                Vector2 offset = HudMain.Cursor.Origin - textElement.Origin;
                Position = ClampIndex(text.GetCharAtOffset(offset));
                caretOffset = GetOffsetFromIndex(Position);

                if (text.Count > 0 && text[Position.X].Count > 0 && offset.X < text[Position].Offset.X)
                    Prepend = true;
                else
                    Prepend = false;
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

        protected class SelectionBox : HudElementBase
        {
            public Color Color { get { return selectionBoard.Color; } set { selectionBoard.Color = value; } }
            public Vector2I Start { get; private set; }
            public Vector2I End { get; private set; }
            public bool Empty => Start == -Vector2I.One || End == -Vector2I.One || Start == End;

            private readonly TextCaret caret;
            private readonly ITextBoard text;
            private readonly MatBoard selectionBoard;
            private bool canSelect;
            private Vector2 topSize, middleSize, bottomSize,
                topOffset, middleOffset, bottomOffset;

            public SelectionBox(TextCaret caret, Label parent) : base(parent)
            {
                selectionBoard = new MatBoard();
                text = parent.Text;
                this.caret = caret;
            }

            public void ClearSelection()
            {
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
                        canSelect = true;
                    }
                    else if (SharedBinds.LeftButton.IsPressed && canSelect)
                    {
                        if (Start == -Vector2I.One || (caret.Position.X < Start.X || (caret.Position.X == Start.X && caret.Position.Y < Start.Y)))
                            Start = caret.Position;
                        else
                            End = caret.Position;

                        if (!Empty)
                            UpdateSelection();
                    }
                    else if (SharedBinds.LeftButton.IsReleased)
                    {
                        if (Empty)
                            ClearSelection();

                        canSelect = false;
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

            private void UpdateSelection()
            {
                IRichChar selectionStart = text[Start],
                    selectionEnd = text[End];

                if (End.X != Start.X)
                {
                    IRichChar firstLineStart = text[Start.X][0],
                        lastLineStart = text[End.X][0];

                    topSize = new Vector2()
                    {
                        X = text.Size.X - (selectionStart.Offset.X - firstLineStart.Offset.X) - (firstLineStart.Size.X) / 2f,
                        Y = text[Start.X].Size.Y
                    };

                    topOffset = new Vector2()
                    {
                        X = (text.Size.X - topSize.X - selectionStart.Size.X) / 2f,
                        Y = firstLineStart.Offset.Y
                    };

                    bottomSize = new Vector2()
                    {
                        X = (selectionEnd.Offset.X - lastLineStart.Offset.X) + (lastLineStart.Size.X + selectionEnd.Size.X) / 2f,
                        Y = text[End.X].Size.Y
                    };

                    bottomOffset = new Vector2()
                    {
                        X = (bottomSize.X - text.Size.X) / 2f,
                        Y = lastLineStart.Offset.Y
                    };

                    if (End.X - Start.X > 1)
                    {
                        middleSize = new Vector2()
                        {
                            X = text.Size.X,
                            Y = (topOffset.Y - bottomOffset.Y) - (topSize.Y + bottomSize.Y) / 2f
                        };

                        middleOffset = new Vector2()
                        {
                            X = 0f,
                            Y = (topOffset.Y + bottomOffset.Y) / 2f
                        };
                    }
                    else
                        middleSize = Vector2.Zero;
                }
                else
                {
                    topSize = new Vector2()
                    {
                        X = selectionEnd.Offset.X - selectionStart.Offset.X + (selectionEnd.Size.X + selectionStart.Size.X) / 2f,
                        Y = text[Start.X].Size.Y
                    };

                    topOffset = new Vector2()
                    {
                        X = selectionStart.Offset.X + (topSize.X - selectionStart.Size.X) / 2f,
                        Y = selectionStart.Offset.Y
                    };

                    middleSize = Vector2.Zero;
                    bottomSize = Vector2.Zero;
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


            protected override void Draw()
            {
                if (!Empty)
                {
                    // Top
                    selectionBoard.Size = topSize;
                    selectionBoard.offset = topOffset;
                    selectionBoard.Draw(Origin);

                    // Middle
                    if (middleSize != Vector2.Zero)
                    {
                        selectionBoard.Size = middleSize;
                        selectionBoard.offset = middleOffset;
                        selectionBoard.Draw(Origin);
                    }

                    // Bottom
                    if (bottomSize != Vector2.Zero)
                    {
                        selectionBoard.Size = bottomSize;
                        selectionBoard.offset = bottomOffset;
                        selectionBoard.Draw(Origin);
                    }
                }
            }
        }
    }
}
