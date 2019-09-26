using Sandbox.ModAPI;
using VRage.Game;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace DarkHelmet.UI.Rendering
{
    /// <summary>
    /// Used to determine how a given <see cref="Material"/> is scaled on a given Billboard.
    /// </summary>
    public enum MaterialAlignment
    {
        /// <summary>
        /// Stretches/compresses the material to fit on the billboard
        /// </summary>
        StretchToFit,
        /// <summary>
        ///  Resizes the material so that it matches the height of the Billboard while maintaining its aspect ratio
        /// </summary>
        FitVertical,
        /// <summary>
        /// Resizes the material so that it matches the width of the Billboard while maintaining its aspect ratio
        /// </summary>
        FitHorizontal,
        /// <summary>
        /// No resizing, the material is rendered at its native resolution without regard to the size of the Billboard
        /// </summary>
        None
    }

    public class Material
    {
        /// <summary>
        /// ID of the Texture the <see cref="Material"/> is based on.
        /// </summary>
        public readonly MyStringId TextureID;
        /// <summary>
        /// The dimensions, in pixels, of the <see cref="Material"/>.
        /// </summary>
        public readonly Vector2 size;
        /// <summary>
        /// The dimensions of the <see cref="Material"/> relative to the size of the texture its based on.
        /// </summary>
        public readonly Vector2 scaledSize;
        /// <summary>
        /// The starting point of the <see cref="Material"/> on the texture scaled relative to the size of the texture.
        /// </summary>
        public readonly Vector2 scaledOffset;

        /// <summary>
        /// Creates a <see cref="Material"/> using the name of the Texture's ID and its size in pixels.
        /// </summary>
        public Material(string TextureName, Vector2 size)
            : this(MyStringId.GetOrCompute(TextureName), size)
        { }

        /// <summary>
        /// Creates a <see cref="Material"/> based on a Texture Atlas/Sprite with a given offset and size.
        /// </summary>
        public Material(string TextureName, Vector2 textureSize, Vector2 offset, Vector2 size)
            : this(MyStringId.GetOrCompute(TextureName), textureSize, offset, size)
        { }

        /// <summary>
        /// Creates a <see cref="Material"/> using the <see cref="MyStringId"/> of the texture and its size in pixels.
        /// </summary>
        public Material(MyStringId TextureID, Vector2 size)
        {
            this.TextureID = TextureID;
            this.size = size;

            scaledOffset = Vector2.Zero;
            scaledSize = Vector2.One;
        }

        /// <summary>
        /// Creates a <see cref="Material"/> based on an Atlas/Sprite with a given offset and size.
        /// </summary>
        public Material(MyStringId TextureID, Vector2 textureSize, Vector2 offset, Vector2 size)
        {
            this.TextureID = TextureID;
            this.size = size;

            size.X /= textureSize.X;
            size.Y /= textureSize.Y;

            scaledSize = size;

            offset.X /= textureSize.X;
            offset.Y /= textureSize.Y;

            scaledOffset = offset;
        }
    }

    /// <summary>
    /// Defines a quad comprised of four <see cref="Vector2"/>s.
    /// </summary>
    public struct FlatQuad
    {
        public Vector2 Point0, Point1, Point2, Point3;
    }

    public class HudBoard
    {
        public Color Color { get { return color; } set { color = value; bbColor = GetBillboardColor(value); } }
        public Vector2 Size { get { return new Vector2(width, height); } set { width = value.X; height = value.Y; updateMatFit = true; } }
        public float Width { get { return width; } set { width = value; updateMatFit = true; } }
        public float Height { get { return height; } set { height = value; updateMatFit = true; } }

        public Material Material { get { return material; } set { material = value; updateMatFit = true; } }
        public MaterialAlignment MatAlignment { get { return matAlignment; } set { matAlignment = value; updateMatFit = true; } }
        public Vector2 offset;

        private static readonly Material flatColor = new Material(MyStringId.GetOrCompute("HudLibDefault"), new Vector2(4f, 4f));

        private Color color;
        private Vector4 bbColor;
        private float width, height;
        private Material material;
        private MaterialAlignment matAlignment;
        private FlatQuad matFit;
        private bool updateMatFit;

        public HudBoard()
        {
            Material = flatColor;
            MatAlignment = MaterialAlignment.StretchToFit;
            Color = Color.White;
            updateMatFit = true;
        }

        public void Draw(Vector2 origin)
        {
            MatrixD cameraMatrix;
            Vector3D worldPos;
            Vector2 screenPos, boardSize;

            if (updateMatFit)
            {
                matFit = GetMaterialAlignment();
                updateMatFit = false;
            }

            boardSize = HudMain.GetNativeVector(Size) * HudMain.FovScale / 2f;
            boardSize.X *= HudMain.AspectRatio;

            screenPos = HudMain.GetNativeVector(origin + offset) * HudMain.FovScale;
            screenPos.X *= HudMain.AspectRatio;

            cameraMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
            worldPos = Vector3D.Transform(new Vector3D(screenPos.X, screenPos.Y, -0.1), cameraMatrix);

            MyQuadD quad;
            Vector3 normal = MyAPIGateway.Session.Camera.ViewMatrix.Forward;
            MyUtils.GenerateQuad(out quad, ref worldPos, boardSize.X, boardSize.Y, ref cameraMatrix);

            RenderUtils.AddBillboard(worldPos, quad, normal, Material.TextureID, matFit, bbColor);
        }

        private FlatQuad GetMaterialAlignment()
        {
            float xScale, yScale;

            if (MatAlignment == MaterialAlignment.StretchToFit)
            {
                xScale = 1f;
                yScale = 1f;
            }
            else
            {
                xScale = Size.X / Material.size.X;
                yScale = Size.Y / Material.size.Y;

                if (MatAlignment == MaterialAlignment.FitVertical)
                {
                    xScale /= yScale;
                    yScale = 1f;
                }
                else if (MatAlignment == MaterialAlignment.FitHorizontal)
                {
                    yScale /= xScale;
                    xScale = 1f;
                }
            }

            return new FlatQuad()
            {
                Point0 = Material.scaledOffset,
                Point1 = new Vector2(Material.scaledOffset.X, Material.scaledOffset.Y + (Material.scaledSize.Y * yScale)),
                Point2 = new Vector2(Material.scaledOffset.X + (Material.scaledSize.X * xScale), Material.scaledOffset.Y + (Material.scaledSize.Y * yScale)),
                Point3 = new Vector2(Material.scaledOffset.X + (Material.scaledSize.X * xScale), Material.scaledOffset.Y),
            };
        }

        /// <summary>
        /// Returns the color with alpha premultiplied.
        /// </summary>
        /// <returns></returns>
        private static Vector4 GetBillboardColor(Color color)
        {
            float opacity = color.A / 255f;

            color.R = (byte)(color.R * opacity);
            color.G = (byte)(color.G * opacity);
            color.B = (byte)(color.B * opacity);

            return ((Vector4)color).ToLinearRGB();
        }
    }

    public class RichTextBoard : HudDocument
    {
        public IRichChar this[int x, int y] => lines[x][y];
        public override float Scale
        {
            get { return base.Scale; }
            set
            {
                TextSize *= (value / base.Scale);
                MaxSize *= (value / base.Scale);
                base.Scale = value;
            }
        }
        public Vector2 MaxSize
        {
            get { return maxSize; }
            set
            {
                maxSize = value;
                UpdateOffsets();

                if (WordWrapping)
                    SetLineWrapWidth(maxSize.X);
            }
        }
        public Vector2 TextSize { get; private set; }
        public GlyphFormat Format { get; set; }
        public TextAlignment Alignment { get; set; }
        public int startLine;

        private int drawEnd;
        private Vector2 maxSize;

        public RichTextBoard(bool wordWrapping, int capacity = 3) : base(wordWrapping, capacity)
        {
            Scale = 1f;
            startLine = 0;
            drawEnd = -1;
            Format = GlyphFormat.Default;
            Alignment = TextAlignment.Left;
            MaxSize = new Vector2(500f, 500f);
        }

        public int GetLineCount() =>
            lines.Count;

        public int GetLineLength(int index) =>
            lines[index].Count;

        public void SetText(string text)
        {
            Clear();
            Append(text);
        }

        public void SetText(RichString text)
        {
            Clear();
            Append(text);
        }

        public void SetText(RichText text)
        {
            Clear();
            Append(text);
        }

        public void Append(string text) =>
            Append(new RichString(text, Format));

        public Vector2 GetCharEndAtPos(Vector2 pos)
        {
            return Vector2.Zero;
        }

        public static RichTextBoard operator +(RichTextBoard left, string right)
        {
            left.Append(right);
            return left;
        }

        public static RichTextBoard operator +(RichTextBoard left, RichString right)
        {
            left.Append(right);
            return left;
        }

        public static RichTextBoard operator +(RichTextBoard left, RichText right)
        {
            left.Append(right);
            return left;
        }

        public void Draw(Vector2 origin)
        {
            for (int line = startLine; line < drawEnd; line++)
            {
                for (int ch = 0; ch < lines[line].Count; ch++)
                    lines[line][ch].GlyphBoard.Draw(origin);
            }
        }

        protected override void AfterTextUpdate() =>
            UpdateOffsets();

        private void UpdateOffsets()
        {
            UpdateSize();

            if (lines.Count > 0)
            {
                float height = WordWrapping ? MaxSize.Y / 2f : TextSize.Y / 2f;
                height -= lines[startLine].Size.Y / 2f;

                for (int line = startLine; line < drawEnd; line++)
                {
                    UpdateLineOffsets(line, height);
                    height += lines[line].Size.Y;
                }
            }
        }

        private void UpdateSize()
        {
            float width = 0f, height = 0f;

            for (int line = startLine; line < lines.Count; line++)
            {
                if (height <= (MaxSize.Y - lines[line].Size.Y))
                {
                    if (lines[line].Size.X > width)
                        width = lines[line].Size.X;

                    height += lines[line].Size.Y;
                    drawEnd = line + 1;
                }
                else
                    break;
            }

            TextSize = new Vector2(width, height);
        }

        private void UpdateLineOffsets(int line, float height)
        {
            float width = 0f, xAlign;

            if (!WordWrapping)
                xAlign = GetLineAlignment(lines[line], Alignment, TextSize);
            else
                xAlign = GetLineAlignment(lines[line], Alignment, MaxSize);

            for (int ch = 0; ch < lines[line].Count; ch++)
            {
                RichChar richChar = lines[line][ch];
                GlyphFormat formatting = richChar.Formatting;
                float scale = formatting.scale * formatting.fontStyle.FontScale * Scale;

                if (ch > 0 && CanUseKernings(lines[line][ch - 1], richChar))
                    width += formatting.fontStyle.GetKerningAdjustment(lines[line][ch - 1].Ch, richChar.Ch) * scale;

                richChar.GlyphBoard.offset = new Vector2()
                {
                    X = width + richChar.GlyphBoard.Size.X / 2f + (richChar.Glyph.leftSideBearing * scale) + xAlign,
                    Y = height + (lines[line].Size.Y - richChar.Size.Y)
                };

                width += richChar.Size.X;
            }
        }

        private static float GetLineAlignment(Line line, TextAlignment alignment, Vector2 size)
        {
            float offset = 0f;

            if ((alignment & TextAlignment.Left) == TextAlignment.Left)
                offset = -size.X / 2f;
            else if ((alignment & TextAlignment.Center) == TextAlignment.Center)
                offset = -line.Size.X / 2f;
            else if ((alignment & TextAlignment.Right) == TextAlignment.Right)
                offset = (size.X / 2f) - line.Size.X;

            return offset;
        }

        private bool CanUseKernings(RichChar left, RichChar right) =>
             left.Formatting.fontStyle == right.Formatting.fontStyle && left.Formatting.scale == right.Formatting.scale;
    }

    public static class RenderUtils
    {
        public static void AddBillboard(Vector3D pos, MyQuadD quad, Vector3 normal, MyStringId matID, FlatQuad matFit, Vector4 color)
        {
            MyTransparentGeometry.AddTriangleBillboard
            (
                quad.Point0,
                quad.Point1,
                quad.Point2,
                normal, normal, normal,
                matFit.Point0,
                matFit.Point1,
                matFit.Point2,
                matID, 0,
                pos,
                color,
                BlendTypeEnum.PostPP
            );

            MyTransparentGeometry.AddTriangleBillboard
            (
                quad.Point0,
                quad.Point2,
                quad.Point3,
                normal, normal, normal,
                matFit.Point0,
                matFit.Point2,
                matFit.Point3,
                matID, 0,
                pos,
                color,
                BlendTypeEnum.PostPP
            );
        }
    }
}