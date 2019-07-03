using DarkHelmet.UI.TextHudApi;
using Sandbox.ModAPI;
using System;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace DarkHelmet.UI
{
    /// <summary>
    /// Base class for hud elements that can be manually resized.
    /// </summary>
    public abstract class ResizableElementBase : HudUtilities.ElementBase
    {
        public sealed override Vector2D UnscaledSize
        {
            get { return new Vector2D(UnscaledWidth, UnscaledHeight); }
            protected set { UnscaledWidth = value.X; UnscaledHeight = value.Y; }
        }
        /// <summary>
        /// With of the hud element in pixels.
        /// </summary>
        public double Width { get { return UnscaledWidth * Scale; } set { UnscaledWidth = value / Scale; } }
        /// <summary>
        /// Height of the hud element in pixels.
        /// </summary>
        public double Height { get { return UnscaledHeight * Scale; } set { UnscaledHeight = value / Scale; } }
        /// <summary>
        /// Widht of the hud element without scaling applied.
        /// </summary>
        public virtual double UnscaledWidth { get { return unscaledWidth; } set { unscaledWidth = Math.Abs(value); } }
        /// <summary>
        /// Height of the hud element without scaling applied.
        /// </summary>
        public virtual double UnscaledHeight { get { return unscaledHeight; } set { unscaledHeight = Math.Abs(value); } }

        private double unscaledWidth, unscaledHeight;

        public virtual void SetSize(Vector2D newSize) =>
            Size = newSize;

        public virtual void SetUnscaledSize(Vector2D newSize) =>
            UnscaledSize = newSize;
    }

    /// <summary>
    /// Base class for hud elements that have text elements and a background of some sort for that text.
    /// </summary>
    public abstract class TextBoxBase : ResizableElementBase
    {
        /// <summary>
        /// The smallest size the background is allowed to be set to. (TextSize + Padding * Scale)
        /// </summary>
        public Vector2D MinimumSize => TextSize + Padding;
        public Vector2D Padding { get { return padding * Scale; } set { padding = Utils.Math.Abs(value / Scale); } }
        public abstract Vector2D TextSize { get; }
        public virtual double TextScale { get; set; }

        public bool autoResize;
        private Vector2D padding;

        public TextBoxBase()
        {
            autoResize = true;
            TextScale = 1d;
        }

        protected override void BeforeDraw()
        {
            if (Visible)
            {
                Vector2D minSize = MinimumSize;

                if (autoResize)
                    Size = minSize;
                else
                {
                    if (Width < minSize.X)
                        Width = minSize.X;

                    if (Height < minSize.Y)
                        Height = minSize.Y;
                }

                Draw();
            }
        }
    }

    /// <summary>
    /// Wrapper used to make precise pixel-level manipluation of <see cref="HudAPIv2.HUDMessage"/> easier.
    /// </summary>
    public class TextHudMessage : HudUtilities.ElementBase
    {
        public string Text { get { return text; } set { text = value; UpdateMessage(); } }

        public TextAlignment textAlignment;
        private HudAPIv2.HUDMessage hudMessage;
        private Vector2D alignmentOffset;
        private string text;

        public TextHudMessage()
        {
            textAlignment = TextAlignment.Center;
        }

        protected override void Draw()
        {
            if (hudMessage == null)
            {
                hudMessage = new HudAPIv2.HUDMessage
                {
                    Blend = BlendTypeEnum.PostPP,
                    Scale = Scale * HudUtilities.InvTextApiScale,
                    Options = HudAPIv2.Options.Fixed,
                    Visible = false,
                };

                UpdateMessage();
            }

            hudMessage.Scale = Scale * HudUtilities.InvTextApiScale;
            UpdateTextOffset();

            hudMessage.Origin = HudUtilities.GetRelativeVector(Origin + Offset + alignmentOffset);
            hudMessage.Draw();
        }

        private void UpdateMessage()
        {
            if (hudMessage != null && Text != null)
            {
                hudMessage.Message.Clear();
                hudMessage.Message.Append(Text);
                RelativeSize = hudMessage.GetTextLength() / Scale;
            }
        }

        private void UpdateTextOffset()
        {
            Vector2D offset = Size / 2d;
            alignmentOffset = offset;
            alignmentOffset.X *= -1;

            if (textAlignment == TextAlignment.Right)
                alignmentOffset.X -= offset.X;
            else if (textAlignment == TextAlignment.Left)
                alignmentOffset.X += offset.X;
        }
    }

    /// <summary>
    /// Creates a colored box of a given width and height with a given mateiral. The default material is just a plain color.
    /// </summary>
    public class TexturedBox : ResizableElementBase
    {
        public MyStringId material;
        public Color color;
        private static readonly MyStringId square = MyStringId.GetOrCompute("Square");

        public TexturedBox()
        {
            material = square;
            color = Color.White;
        }

        protected override void Draw()
        {
            if (color.A > 0)
            {
                MatrixD cameraMatrix;
                Quaternion rotquad;
                Vector3D boardPos;
                Vector2D boardOrigin, boardSize;

                boardSize = RelativeSize * Scale * HudUtilities.FovScale / 2d;
                boardSize.X *= HudUtilities.AspectRatio;

                boardOrigin = HudUtilities.GetRelativeVector(Origin + Offset) * HudUtilities.FovScale;
                boardOrigin.X *= HudUtilities.AspectRatio;

                cameraMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
                boardPos = Vector3D.Transform(new Vector3D(boardOrigin.X, boardOrigin.Y, -0.1), cameraMatrix);

                rotquad = Quaternion.CreateFromAxisAngle(cameraMatrix.Forward, 0f);
                cameraMatrix = MatrixD.Transform(cameraMatrix, rotquad);

                MyTransparentGeometry.AddBillboardOriented
                (
                    material,
                    GetBillboardColor(color),
                    boardPos,
                    cameraMatrix.Left,
                    cameraMatrix.Up,
                    (float)boardSize.X,
                    (float)boardSize.Y,
                    Vector2.Zero,
                    BlendTypeEnum.PostPP
                );
            }
        }

        private static Color GetBillboardColor(Color color)
        {
            double opacity = color.A / 255d;

            color.R = (byte)(color.R * opacity);
            color.G = (byte)(color.G * opacity);
            color.B = (byte)(color.B * opacity);

            return color;
        }
    }

}