using System;
using VRageMath;
using RichHudFramework.UI.Rendering;

namespace RichHudFramework.UI
{
    using Client;
    using Server;

    /// <summary>
    /// Base type for HUD windows. Supports dragging/resizing like pretty much every other window ever.
    /// </summary>
    public abstract class WindowBase : HudElementBase
    {
        public RichText HeaderText { get { return Header.GetText(); } set { Header.SetText(value); } }

        public ITextBuilder Header => header.TextBoard;

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

        /// <summary>
        /// Position of the window relative to its origin. Clamped to prevent the window from moving
        /// off screen.
        /// </summary>
        public override Vector2 Offset
        {
            set
            {
                Vector2 bounds = new Vector2(HudMain.ScreenWidth, HudMain.ScreenHeight) / 2f,
                    newPos = value + Origin;

                newPos.X = MathHelper.Clamp(newPos.X, -bounds.X, bounds.X);
                newPos.Y = MathHelper.Clamp(newPos.Y, -bounds.Y, bounds.Y);

                base.Offset = newPos - Origin;
            }
        }

        /// <summary>
        /// Minimum allowable size for the window.
        /// </summary>
        public Vector2 MinimumSize { get { return minimumSize * Scale; } set { minimumSize = value / Scale; } }

        /// <summary>
        /// Determines whether or not the window can be resized by the user
        /// </summary>
        public bool AllowResizing { get; set; }

        /// <summary>
        /// Determines whether or not the user can reposition the window
        /// </summary>
        public bool CanDrag { get; set; }

        /// <summary>
        /// Window header element.
        /// </summary>
        public readonly LabelBoxButton header;

        /// <summary>
        /// Textured background. Body of the window.
        /// </summary>
        public readonly TexturedBox body;

        /// <summary>
        /// Window border.
        /// </summary>
        public readonly BorderBox border;

        private readonly MouseInputElement resizeLeft, resizeTop, resizeRight, resizeBottom;
        protected float cornerSize = 8f;
        protected bool canMoveWindow, canResize;
        protected int resizeDir;
        protected Vector2 cursorOffset, minimumSize;

        public WindowBase(IHudParent parent) : base(parent)
        {
            CaptureCursor = true;
            AllowResizing = true;
            CanDrag = true;
            MinimumSize = new Vector2(200f, 200f);

            header = new LabelBoxButton(this)
            {
                DimAlignment = DimAlignments.Width,
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner,
                HighlightEnabled = false,
                Height = 24f,
                AutoResize = false
            };

            body = new TexturedBox(header)
            {
                DimAlignment = DimAlignments.Width,
                ParentAlignment = ParentAlignments.Bottom,
            };

            border = new BorderBox(this)
            { Thickness = 1f, DimAlignment = DimAlignments.Both, };

            resizeBottom = new MouseInputElement(this) 
            { Height = 1f, DimAlignment = DimAlignments.Width, ParentAlignment = ParentAlignments.Bottom };

            resizeTop = new MouseInputElement(this) 
            { Height = 1f, DimAlignment = DimAlignments.Width, ParentAlignment = ParentAlignments.Top };

            resizeLeft = new MouseInputElement(this) 
            { Width = 1f, DimAlignment = DimAlignments.Height, ParentAlignment = ParentAlignments.Left };

            resizeRight = new MouseInputElement(this) 
            { Width = 1f, DimAlignment = DimAlignments.Height, ParentAlignment = ParentAlignments.Right };

            header.MouseInput.OnLeftClick += HeaderClicked;

            resizeBottom.OnLeftClick += ResizeClicked;
            resizeTop.OnLeftClick += ResizeClicked;
            resizeLeft.OnLeftClick += ResizeClicked;
            resizeRight.OnLeftClick += ResizeClicked;
        }

        protected virtual void HeaderClicked()
        {
            if (CanDrag)
            {
                canMoveWindow = true;
                cursorOffset = (Origin + Offset) - HudMain.Cursor.Origin;
            }
        }

        protected virtual void ResizeClicked()
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

        protected override void Layout()
        {
            if (canMoveWindow)
                Offset = HudMain.Cursor.Origin + cursorOffset - Origin;

            if (canResize)
                Resize();

            body.Height = Height - header.Height;
        }

        protected void Resize()
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

        protected virtual void HeaderReleased()
        {
            canMoveWindow = false;
        }

        protected virtual void ResizeStopped()
        {
            canResize = false;
        }
    }
}