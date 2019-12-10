using System;
using VRageMath;
using DarkHelmet.UI.Rendering;

namespace DarkHelmet.UI
{
    using Client;
    using Server;

    /// <summary>
    /// Base type for all windows. Supports dragging/resizing like pretty much every other window ever.
    /// </summary>
    public abstract class WindowBase : HudElementBase
    {
        public ITextBuilder Title { get { return header.Text; } }

        /// <summary>
        /// Determines the color of both the header and the border.
        /// </summary>
        public Color BorderColor
        {
            get { return header.Color; }
            set
            {
                header.Color = value;
                border.Color = value;
            }
        }

        /// <summary>
        /// Determines the color of the body of the window.
        /// </summary>
        public Color BodyColor { get { return body.Color; } set { body.Color = value; } }

        public override Vector2 Origin
        {
            protected set
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
            set
            {
                base.Width = value;
                header.Width = value;
                resizer.Width = value + 2f;
            }
        }
        public override float Height
        {
            set
            {
                base.Height = value;
                resizer.Height = value + 2f;
            }
        }

        /// <summary>
        /// Minimum allowable size for the window.
        /// </summary>
        public Vector2 MinimumSize { get; set; }

        public IHudElement Header => header;
        public IHudElement Background => body;
        public bool AllowResizing { get; set; }
        public bool CanDrag { get; set; }

        protected readonly TextBoxButton header;
        protected readonly TexturedBox body;
        protected readonly BorderBox border;
        private readonly ClickableElement resizer;

        private const float cornerSize = 8f;
        private bool canMoveWindow, canResize;
        private int resizeDir;
        private Vector2 cursorOffset;

        public WindowBase(IHudParent parent = null) : base(parent)
        {
            CaptureCursor = true;
            AllowResizing = true;
            CanDrag = true;
            MinimumSize = new Vector2(200f, 200f);

            resizer = new ClickableElement(this);
            header = new TextBoxButton(this) { ParentAlignment = ParentAlignment.Top, HighlightEnabled = false, Height = 24f, AutoResize = false };
            border = new BorderBox(this) { Thickness = 1f, MatchParentSize = true };
            body = new TexturedBox(header) { ParentAlignment = ParentAlignment.Bottom };

            header.MouseInput.OnLeftClick += HeaderClicked;
            resizer.OnLeftClick += ResizeClicked;
        }

        protected override void Draw()
        {
            body.Size = new Vector2(Width, Height - header.Height);
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
                if (!SharedBinds.LeftButton.IsPressed)
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
            if (CanDrag)
            {
                canMoveWindow = true;
                cursorOffset = Origin - HudMain.Cursor.Origin;
            }
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
                    resizeDir += 1;

                if (Height - 2d * Math.Abs(Origin.Y - HudMain.Cursor.Origin.Y) <= cornerSize)
                    resizeDir += 2;
            }
        }

        private void ResizeStopped()
        {
            canResize = false;
        }
    }
}