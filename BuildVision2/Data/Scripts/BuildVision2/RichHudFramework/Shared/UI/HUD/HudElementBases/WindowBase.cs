using System;
using VRageMath;
using RichHudFramework.UI.Rendering;
using RichHudFramework.Internal;

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

        public bool WindowActive { get; protected set; }

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
        protected readonly Action<byte> LoseFocusCallback;
        protected float cornerSize = 8f;
        protected bool canMoveWindow, canResize;
        protected int resizeDir;
        protected Vector2 cursorOffset, minimumSize;

        public WindowBase(HudParentBase parent) : base(parent)
        {
            header = new LabelBoxButton(this)
            {
                Format = GlyphFormat.White.WithAlignment(TextAlignment.Center),
                DimAlignment = DimAlignments.Width,
                ParentAlignment = ParentAlignments.Top | ParentAlignments.Inner,
                HighlightEnabled = false,
                Height = 32f,
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

            UseCursor = true;
            ShareCursor = false;
            AllowResizing = true;
            CanDrag = true;
            MinimumSize = new Vector2(200f, 200f);

            LoseFocusCallback = LoseFocus;
            GetFocus();
        }

        protected override void Layout()
        {
            body.Height = Height - header.Height;

            if (canMoveWindow)
                Offset = HudMain.Cursor.ScreenPos + cursorOffset - Origin;

            if (canResize)
                Resize();            
        }

        protected void Resize()
        {
            Vector2 center = Origin + Offset,
                cursorPos = HudMain.Cursor.ScreenPos, newOffset = Offset;
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
                if (SharedBinds.LeftButton.IsNewPressed && !WindowActive)
                    GetFocus();
            }

            if (CanDrag && header.MouseInput.IsLeftClicked)
            {
                canMoveWindow = true;
                cursorOffset = (Origin + Offset) - HudMain.Cursor.ScreenPos;
            }

            if (AllowResizing && IsBorderClicked())
            {
                Vector2 pos = Origin + Offset;
                canResize = true;
                resizeDir = 0;

                if (Width - 2d * Math.Abs(pos.X - HudMain.Cursor.ScreenPos.X) <= cornerSize)
                    resizeDir += 1;

                if (Height - 2d * Math.Abs(pos.Y - HudMain.Cursor.ScreenPos.Y) <= cornerSize)
                    resizeDir += 2;
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

        public virtual void GetFocus()
        {
            zOffsetInner = HudMain.GetFocusOffset(LoseFocusCallback);
            WindowActive = true;
        }

        protected virtual void LoseFocus(byte newOffset)
        {
            zOffsetInner = newOffset;
            WindowActive = false;
        }

        /// <summary>
        /// Returns true if the border is clicked.
        /// </summary>
        /// <returns></returns>
        protected bool IsBorderClicked()
        {
            return resizeBottom.IsLeftClicked || resizeTop.IsLeftClicked || resizeLeft.IsLeftClicked || resizeRight.IsLeftClicked;
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