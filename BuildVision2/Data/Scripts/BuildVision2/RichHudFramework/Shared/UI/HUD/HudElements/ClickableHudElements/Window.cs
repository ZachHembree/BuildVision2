using System;
using VRageMath;
using RichHudFramework.UI.Rendering;
using RichHudFramework.Internal;

namespace RichHudFramework.UI
{
    using Client;
    using Server;

    /// <summary>
    /// Basic window type with a header, body and border. Supports dragging/resizing like pretty much every 
    /// other window ever.
    /// </summary>
    public class Window : HudElementBase, IClickableElement
    {
        /// <summary>
        /// Window header text
        /// </summary>
        public RichText HeaderText { get { return HeaderBuilder.GetText(); } set { HeaderBuilder.SetText(value); } }

        /// <summary>
        /// Text builder for the window header
        /// </summary>
        public ITextBuilder HeaderBuilder => header.TextBoard;

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
        public virtual Color BodyColor { get { return windowBg.Color; } set { windowBg.Color = value; } }

        /// <summary>
        /// Minimum allowable size for the window.
        /// </summary>
        public Vector2 MinimumSize { get { return _minimumSize; } set { _minimumSize = value; } }

        /// <summary>
        /// Determines whether or not the window can be resized by the user
        /// </summary>
        public bool AllowResizing { get; set; }

        /// <summary>
        /// Determines whether or not the user can reposition the window
        /// </summary>
        public bool CanDrag { get; set; }

        /// <summary>
        /// Returns true if the window has focus and is accepting input
        /// </summary>
        public bool WindowActive { get; protected set; }

        /// <summary>
        /// Returns true if the cursor is over the window
        /// </summary>
        public override bool IsMousedOver => resizeInput.IsMousedOver;

        /// <summary>
        /// Mouse input element for the window
        /// </summary>
        public IMouseInput MouseInput => resizeInput;

        /// <summary>
        /// Window header element
        /// </summary>
        public readonly LabelBoxButton header;

        /// <summary>
        /// Textured background. Body of the window
        /// </summary>
        public readonly HudElementBase body;

        /// <summary>
        /// Window border
        /// </summary>
        public readonly BorderBox border;

        protected readonly MouseInputElement inputInner, resizeInput;
        protected readonly TexturedBox windowBg;

        protected readonly Action<byte> LoseFocusCallback;
        protected float cornerSize = 16f;
        protected bool canMoveWindow;
        protected Vector2 resizeDir, cursorOffset, _minimumSize;

        public Window(HudParentBase parent) : base(parent)
        {
            header = new LabelBoxButton(this)
            {
                DimAlignment = DimAlignments.Width,
                Height = 32f,
                ParentAlignment = ParentAlignments.InnerTop,
                ZOffset = 1,
                Format = GlyphFormat.White.WithAlignment(TextAlignment.Center),
                HighlightEnabled = false,
                AutoResize = false,
            };

            body = new EmptyHudElement(this)
            {
                ParentAlignment = ParentAlignments.InnerBottom,
            };

            windowBg = new TexturedBox(this)
            {
                DimAlignment = DimAlignments.Size,
                ZOffset = -2,
            };

            border = new BorderBox(this)
            {
                ZOffset = 1,
                Thickness = 1f,
                DimAlignment = DimAlignments.Size,
            };

            resizeInput = new MouseInputElement(this)
            {
                ZOffset = sbyte.MaxValue,
                Padding = new Vector2(16f),
                DimAlignment = DimAlignments.Size,
                CanIgnoreMasking = true
            };
            
            inputInner = new MouseInputElement(resizeInput)
            {
                DimAlignment = DimAlignments.UnpaddedSize,
            };

            resizeDir = Vector2.Zero;
            AllowResizing = true;
            CanDrag = true;
            UseCursor = true;
            ShareCursor = false;
            IsMasking = true;
            MinimumSize = new Vector2(200f, 200f);

            LoseFocusCallback = LoseFocus;
            GetFocus();
        }

        protected override void Layout()
        {
            body.Height = CachedSize.Y - Padding.Y - header.Height;
            body.Width = CachedSize.X - Padding.X;
        }

        protected void Resize(Vector2 cursorPos)
        {
            Vector2 pos = Origin + Offset,
                delta = resizeDir * (cursorPos - pos),
                size = CachedSize;

            if (delta.X > 0f)
            {
                delta.X = Math.Max(delta.X, .5f * MinimumSize.X);
                size.X = .5f * size.X + delta.X;
                pos.X = ((resizeDir.X * delta.X) + pos.X) + (-resizeDir.X * .5f * size.X);
            }

            if (delta.Y > 0f)
            {
                delta.Y = Math.Max(delta.Y, .5f * MinimumSize.Y);
                size.Y = .5f * size.Y + delta.Y;
                pos.Y = ((resizeDir.Y * delta.Y) + pos.Y) + (-resizeDir.Y * .5f * size.Y);
            }

            Size = size;
            Offset = pos - Origin;
        }

        protected override void HandleInput(Vector2 cursorPos)
        {
            if (IsMousedOver)
            {
                if (SharedBinds.LeftButton.IsNewPressed && !WindowActive)
                    GetFocus();
            }

            if (AllowResizing && resizeInput.IsNewLeftClicked && !inputInner.IsMousedOver)
            {
                Vector2 pos = Origin + Offset,
                        delta = cursorPos - pos;

                resizeDir = Vector2.Zero;

                if (Width - (2f * Math.Abs(delta.X)) <= cornerSize)
                    resizeDir.X = (delta.X >= 0f) ? 1f : -1f;

                if (Height - (2f * Math.Abs(delta.Y)) <= cornerSize)
                    resizeDir.Y = (delta.Y >= 0f) ? 1f : -1f;
            }
            else if (CanDrag && header.MouseInput.IsNewLeftClicked)
            {
                canMoveWindow = true;
                cursorOffset = (Origin + Offset) - cursorPos;
            }

            if ((resizeDir != Vector2.Zero) || canMoveWindow)
            {
                if (!SharedBinds.LeftButton.IsPressed)
                {
                    canMoveWindow = false;
                    resizeDir = Vector2.Zero;
                }
            }

            if (!WindowActive)
            {
                canMoveWindow = false;
                resizeDir = Vector2.Zero;
            }

            if (canMoveWindow)
                Offset = cursorPos + cursorOffset - Origin;

            if (resizeDir != Vector2.Zero)
                Resize(cursorPos);
        }

        /// <summary>
        /// Brings the window into the foreground
        /// </summary>
        public virtual void GetFocus()
        {
            layerData.zOffsetInner = HudMain.GetFocusOffset(LoseFocusCallback);
            WindowActive = true;
        }

        protected virtual void LoseFocus(byte newOffset)
        {
            layerData.zOffsetInner = newOffset;
            WindowActive = false;
        }
    }
}