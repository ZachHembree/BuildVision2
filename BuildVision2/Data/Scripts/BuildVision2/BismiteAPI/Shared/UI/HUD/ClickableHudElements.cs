using System;
using System.Collections.Generic;
using VRageMath;
using DarkHelmet.UI.Rendering;

namespace DarkHelmet.UI
{
    public class ClickableElement : ResizableElementBase, IReadonlyClickableElement
    {
        public event Action OnLeftClick, OnRightClick, OnLeftRelease, OnRightRelease, OnCursorEnter, OnCursorExit;
        private bool mouseCursorEntered;

        public ClickableElement(IHudParent parent = null) : base(parent)
        {
            CaptureCursor = true;
        }

        protected override void HandleInput()
        {
            if (IsMousedOver)
            {
                if (!mouseCursorEntered)
                {
                    mouseCursorEntered = true;
                    OnCursorEnter?.Invoke();
                }

                if (SharedBinds.LeftButton.IsNewPressed)
                    OnLeftClick?.Invoke();
                else if (SharedBinds.RightButton.IsNewPressed)
                    OnRightClick?.Invoke();
                else if (SharedBinds.LeftButton.IsReleased)
                    OnLeftRelease?.Invoke();
                else if (SharedBinds.RightButton.IsReleased)
                    OnRightRelease?.Invoke();
            }
            else if (mouseCursorEntered)
            {
                mouseCursorEntered = false;
                OnCursorExit?.Invoke();

                if (SharedBinds.LeftButton.IsPressed)
                    OnLeftRelease?.Invoke();
                else if (SharedBinds.RightButton.IsPressed)
                    OnRightRelease?.Invoke();
            }
        }
    }

    public class Button : TexturedBox
    {
        public override float Width { set { base.Width = value; mouseInput.Width = value; } }
        public override float Height { set { base.Height = value; mouseInput.Height = value; } }

        public IReadonlyClickableElement MouseInput => mouseInput;
        public bool highlightEnabled;
        public Color highlightColor;

        private readonly ClickableElement mouseInput;
        private Color oldColor;

        public Button(IHudParent parent = null) : base(parent)
        {
            mouseInput = new ClickableElement(this);
            highlightColor = new Color(255, 255, 255, 125);
            highlightEnabled = true;

            mouseInput.OnCursorEnter += CursorEntered;
            mouseInput.OnCursorExit += CursorExited;
        }

        protected void CursorEntered()
        {
            if (highlightEnabled)
            {
                oldColor = Color;
                Color = highlightColor;
            }
        }

        protected void CursorExited()
        {
            if (highlightEnabled)
            {
                Color = oldColor;
            }
        }
    }

    public class TextButton : TextBox
    {
        public override float Width { set { base.Width = value; mouseInput.Width = value; } }
        public override float Height { set { base.Height = value; mouseInput.Height = value; } }

        public Color highlightColor;
        public bool highlightEnabled;
        public IReadonlyClickableElement MouseInput => mouseInput;

        private readonly ClickableElement mouseInput;
        private Color oldColor;

        public TextButton(IHudParent parent = null) : base(parent)
        {
            mouseInput = new ClickableElement(this);
            highlightColor = new Color(255, 255, 255, 125);
            highlightEnabled = true;

            mouseInput.OnCursorEnter += CursorEntered;
            mouseInput.OnCursorExit += CursorExited;
        }

        protected void CursorEntered()
        {
            if (highlightEnabled)
            {
                oldColor = BgColor;
                BgColor = highlightColor;
            }
        }

        protected void CursorExited()
        {
            if (highlightEnabled)
            {
                BgColor = oldColor;
            }
        }
    }

    public class Window : ResizableElementBase
    {
        public TextBoard Title { get { return header.Text; } set { header.Text = value; } }
        public Color HeaderColor
        {
            get { return header.BgColor; }
            set
            {
                header.BgColor = value;
                leftBorder.Color = value;
                rightBorder.Color = value;
                bottomBorder.Color = value;
                topBorder.Color = value;
            }
        }
        public Color BgColor { get { return background.Color; } set { background.Color = value; } }

        public override Vector2 Origin
        {
            set
            {
                Vector2 bounds = new Vector2(HudMain.ScreenWidth, HudMain.ScreenHeight) / 2f,
                    newPos = value;

                newPos.X = Utils.Math.Clamp(newPos.X, -bounds.X, bounds.X);
                newPos.Y = Utils.Math.Clamp(newPos.Y, -bounds.Y, bounds.Y - Size.Y / 2);

                base.Origin = newPos;
            }
        }
        public override float Width
        {
            get { return base.Width; }
            set
            {
                base.Width = value;
                topBorder.Width = value;
                bottomBorder.Width = value;
            }
        }
        public override float Height
        {
            get { return base.Height; }
            set
            {
                base.Height = value;
                leftBorder.Height = value;
                rightBorder.Height = value;
            }
        }
        public override Vector2 MinimumSize => header.MinimumSize;
        public IReadonlyHudElement Header => header;
        public IReadonlyHudElement Background => background;
        public bool AllowResizing { get; set; }

        protected readonly TextButton header;
        protected readonly Button leftBorder, rightBorder, bottomBorder, topBorder;
        protected readonly TexturedBox background;

        private const float cornerSize = 8f;
        private bool canMoveWindow, canResize;
        private int resizeDir;
        private Vector2 cursorOffset;

        public Window(IHudParent parent = null) : base(parent)
        {
            CaptureCursor = true;
            AllowResizing = true;

            header = new TextButton(this) { ParentAlignment = ParentAlignment.Top, autoResize = false, highlightEnabled = false };
            background = new TexturedBox(header) { ParentAlignment = ParentAlignment.Bottom };

            leftBorder = new Button(this) { ParentAlignment = ParentAlignment.Left, autoResize = false, highlightEnabled = false, Width = 1f, Offset = new Vector2(1f, 0f) };
            rightBorder = new Button(this) { ParentAlignment = ParentAlignment.Right, autoResize = false, highlightEnabled = false, Width = 1f, Offset = new Vector2(-1f, 0f) };
            bottomBorder = new Button(this) { ParentAlignment = ParentAlignment.Bottom, autoResize = false, highlightEnabled = false, Height = 1f, Offset = new Vector2(0f, 1f) };
            topBorder = new Button(this) { ParentAlignment = ParentAlignment.Top, autoResize = false, highlightEnabled = false, Height = 1f, Offset = new Vector2(0f, -1f) };

            header.MouseInput.OnLeftClick += HeaderClicked;
            leftBorder.MouseInput.OnLeftClick += ResizeClicked;
            rightBorder.MouseInput.OnLeftClick += ResizeClicked;
            bottomBorder.MouseInput.OnLeftClick += ResizeClicked;
            topBorder.MouseInput.OnLeftClick += ResizeClicked;
        }

        protected override void AfterDraw()
        {
            header.SetSize(new Vector2(Size.X, header.MinimumSize.Y));
            background.SetSize(new Vector2(Size.X, Size.Y - header.MinimumSize.Y));
            header.Offset = new Vector2(0f, -header.Size.Y);

            if (canMoveWindow)
                Origin = HudMain.Cursor.Origin + cursorOffset;

            if (canResize)
                Resize();
        }

        protected override void HandleInput()
        {
            if (IsMousedOver)
            {
                if (SharedBinds.LeftButton.IsNewPressed)
                    GetFocus();
            }

            if (canResize || canMoveWindow)
            {
                if (!(SharedBinds.LeftButton.IsPressed || SharedBinds.RightButton.IsPressed))
                {
                    HeaderReleased();
                    ResizeStopped();
                }
            }
        }

        private void Resize()
        {
            Vector2 cursorPos = HudMain.Cursor.Origin, newOrigin = Origin;
            float newWidth, newHeight;

            if (resizeDir == 1 || resizeDir == 3)
            {
                newWidth = Math.Abs(newOrigin.X - cursorPos.X) + Width / 2f;

                if (newWidth >= MinimumSize.X)
                {
                    Width = newWidth;

                    if (cursorPos.X > Origin.X)
                        newOrigin.X = cursorPos.X - Width / 2f;
                    else
                        newOrigin.X = cursorPos.X + Width / 2f;
                }
                else
                    canResize = false;
            }

            if (resizeDir == 2 || resizeDir == 3)
            {
                newHeight = Math.Abs(newOrigin.Y - cursorPos.Y) + Height / 2f;

                if (newHeight >= MinimumSize.Y)
                {
                    Height = newHeight;

                    if (cursorPos.Y > Origin.Y)
                        newOrigin.Y = cursorPos.Y - Height / 2f;
                    else
                        newOrigin.Y = cursorPos.Y + Height / 2f;
                }
                else
                    canResize = false;
            }

            Origin = newOrigin;
        }

        private void HeaderClicked()
        {
            canMoveWindow = true;
            cursorOffset = Origin - HudMain.Cursor.Origin;
        }

        private void HeaderReleased()
        {
            canMoveWindow = false;
        }

        private void ResizeClicked()
        {
            if (AllowResizing)
            {
                canResize = true;
                resizeDir = 0;

                if (Width - 2d * Math.Abs(Origin.X - HudMain.Cursor.Origin.X) <= cornerSize)
                    resizeDir++;

                if (Height - 2d * Math.Abs(Origin.Y - HudMain.Cursor.Origin.Y) <= cornerSize)
                    resizeDir += 2;
            }
        }

        private void ResizeStopped()
        {
            canResize = false;
        }
    }

    public class ScrollBar : ResizableElementBase
    {
        public override float Width { set { base.Width = value; background.Width = value; } }
        public override float Height { set { base.Height = value; scrollBox.Height = value; } }
        public override Vector2 MinimumSize => scrollBox.Size;
        public IReadonlyResizableElement Background => background;
        public IReadonlyResizableElement ScrollBox => scrollBox;

        protected readonly TexturedBox background, scrollBox;

        private bool canMoveButton;
        private Vector2 cursorOffset;

        public ScrollBar(IHudParent parent = null) : base(parent)
        {
            CaptureCursor = true;
            ShareCursor = false;

            background = new TexturedBox(this) { Height = 12f };
            scrollBox = new TexturedBox(this) { Width = 16f };
        }

        protected override void AfterDraw()
        {
            if (canMoveButton)
                MoveButtonToMouse();
        }

        private void MoveButtonToMouse()
        {
            float
                newOriginX = HudMain.Cursor.Origin.X + cursorOffset.X,
                leftBound = (Origin.X - Width / 2f) + scrollBox.Width / 2f,
                rightBound = (Origin.X + Width / 2f) - scrollBox.Width / 2f;

            newOriginX = Utils.Math.Clamp(newOriginX, leftBound, rightBound);
            scrollBox.Origin = new Vector2(newOriginX, scrollBox.Origin.Y);
        }

        protected override void HandleInput()
        {
            if (IsMousedOver)
            {
                if (SharedBinds.LeftButton.IsNewPressed)
                    canMoveButton = true;

                if (canMoveButton && !SharedBinds.LeftButton.IsPressed)
                    canMoveButton = false;
            }
            else
                canMoveButton = false;
        }
    }
}