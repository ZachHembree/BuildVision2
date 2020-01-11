using System;
using VRageMath;
using RichHudFramework.UI.Rendering;

namespace RichHudFramework.UI
{
    using Client;
    using Server;

    /// <summary>
    /// Base type for all windows. Supports dragging/resizing like pretty much every other window ever.
    /// </summary>
    public abstract class WindowBase : HudElementBase
    {
        public ITextBuilder Title { get { return header.TextBoard; } }

        /// <summary>
        /// Determines the color of both the header and the border.
        /// </summary>
        public virtual Color BorderColor
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
        public virtual Color BodyColor { get { return body.Color; } set { body.Color = value; } }

        public override Vector2 Offset
        {
            set
            {
                Vector2 bounds = new Vector2(HudMain.ScreenWidth, HudMain.ScreenHeight) / 2f,
                    newPos = value + Origin;

                newPos.X = Utils.Math.Clamp(newPos.X, -bounds.X, bounds.X);
                newPos.Y = Utils.Math.Clamp(newPos.Y, -bounds.Y, bounds.Y - Size.Y / 2);

                base.Offset = newPos - Origin;
            }
        }

        /// <summary>
        /// Minimum allowable size for the window.
        /// </summary>
        public Vector2 MinimumSize { get { return minimumSize * Scale; } set { minimumSize = value / Scale; } }

        public bool AllowResizing { get; set; }
        public bool CanDrag { get; set; }

        public readonly TextBoxButton header;
        public readonly TexturedBox body;
        public readonly BorderBox border;

        private readonly ClickableElement resizeLeft, resizeTop, resizeRight, resizeBottom;
        private const float cornerSize = 8f;
        private bool canMoveWindow, canResize;
        private int resizeDir;
        private Vector2 cursorOffset, minimumSize;

        public WindowBase(IHudParent parent = null) : base(parent)
        {
            CaptureCursor = true;
            AllowResizing = true;
            CanDrag = true;
            MinimumSize = new Vector2(200f, 200f);

            body = new TexturedBox(this)
            {
                DimAlignment = DimAlignments.Both,
            };

            border = new BorderBox(this)
            { Thickness = 1f, DimAlignment = DimAlignments.Both, };

            header = new TextBoxButton(this)
            {
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner,
                HighlightEnabled = false,
                Height = 24f,
                AutoResize = false
            };

            resizeBottom = new ClickableElement(this) { Height = 1f, ParentAlignment = ParentAlignments.Bottom };
            resizeTop = new ClickableElement(this) { Height = 1f, ParentAlignment = ParentAlignments.Top };
            resizeLeft = new ClickableElement(this) { Width = 1f, ParentAlignment = ParentAlignments.Left };
            resizeRight = new ClickableElement(this) { Width = 1f, ParentAlignment = ParentAlignments.Right };

            header.MouseInput.OnLeftClick += HeaderClicked;

            resizeBottom.OnLeftClick += ResizeClicked;
            resizeTop.OnLeftClick += ResizeClicked;
            resizeLeft.OnLeftClick += ResizeClicked;
            resizeRight.OnLeftClick += ResizeClicked;
        }

        protected override void BeforeDraw()
        {
            if (canMoveWindow)
                Offset = HudMain.Cursor.Origin + cursorOffset - Origin;

            if (canResize)
                Resize();

            header.Width = Width;

            resizeBottom.Width = Width;
            resizeTop.Width = Width;

            resizeLeft.Height = Height;
            resizeRight.Height = Height;
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
            Vector2 center = Origin + Offset, 
                cursorPos = HudMain.Cursor.Origin, newOffset = Offset;
            float newWidth, newHeight;

            // 1 == horizontal, 3 == both
            if (resizeDir == 1 || resizeDir == 3)
            {
                newWidth = Math.Abs(newOffset.X - cursorPos.X) + Width / 2f;

                if (newWidth >= MinimumSize.X)
                {
                    Width = newWidth;

                    if (cursorPos.X > center.X)
                        newOffset.X = cursorPos.X - Width / 2f;
                    else
                        newOffset.X = cursorPos.X + Width / 2f;
                }
            }

            // 2 == vertical
            if (resizeDir == 2 || resizeDir == 3)
            {
                newHeight = Math.Abs(newOffset.Y - cursorPos.Y) + Height / 2f;

                if (newHeight >= MinimumSize.Y)
                {
                    Height = newHeight;

                    if (cursorPos.Y > center.Y)
                        newOffset.Y = cursorPos.Y - Height / 2f;
                    else
                        newOffset.Y = cursorPos.Y + Height / 2f;
                }
            }

            Offset = newOffset;
        }

        private void HeaderClicked()
        {
            if (CanDrag)
            {
                canMoveWindow = true;
                cursorOffset = (Origin + Offset) - HudMain.Cursor.Origin;
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
                Vector2 pos = Origin + Offset;
                canResize = true;
                resizeDir = 0;

                if (Width - 2d * Math.Abs(pos.X - HudMain.Cursor.Origin.X) <= cornerSize)
                    resizeDir += 1;

                if (Height - 2d * Math.Abs(pos.Y - HudMain.Cursor.Origin.Y) <= cornerSize)
                    resizeDir += 2;
            }
        }

        private void ResizeStopped()
        {
            canResize = false;
        }
    }
}