using RichHudFramework.UI.Rendering;
using System;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI.Rendering
{
    /// <summary>
    /// Renders a 2D polygon using billboards
    /// </summary>
    public class PolyBoard
    {
        /// <summary>
        /// Tinting applied to the material
        /// </summary>
        public virtual Color Color
        {
            get { return color; }
            set
            {
                if (value != color)
                    polyMat.bbColor = BillBoardUtils.GetBillBoardBoardColor(value);

                color = value;
            }
        }

        /// <summary>
        /// Texture applied to the billboard.
        /// </summary>
        public virtual Material Material
        {
            get { return material; }
            set
            {
                if (value != material)
                {
                    material = value;
                    polyMat.textureID = value.TextureID;
                }
            }
        }

        /// <summary>
        /// Get/set number of sides on the polygon
        /// </summary>
        public virtual int Sides
        {
            get { return _sides; }
            set
            {
                if (value != _sides)
                    updateVertices = true;

                _sides = value;
            }
        }

        protected int _sides;

        protected Material material;
        protected Color color;
        protected bool updateVertices;

        protected PolyMaterial polyMat;
        protected readonly List<int> triangles;
        protected readonly List<Vector2> vertices;
        protected readonly List<Vector3D> drawVertices;

        public PolyBoard()
        {
            triangles = new List<int>();
            vertices = new List<Vector2>();
            drawVertices = new List<Vector3D>();

            polyMat = PolyMaterial.Default;
            polyMat.texCoords = new List<Vector2>();

            _sides = 16;
            updateVertices = true;
        }

        /// <summary>
        /// Draws a polygon using billboards
        /// </summary>
        public virtual void Draw(Vector2 size, Vector2 origin, ref MatrixD matrix)
        {
            if (_sides > 2)
            {
                if (updateVertices)
                    GeneratePolygon();

                // Generate final vertices for drawing from unscaled vertices
                for (int i = 0; i < drawVertices.Count; i++)
                {
                    var point = new Vector3D(origin + size * vertices[i], 0d);
                    Vector3D.TransformNoProjection(ref point, ref matrix, out point);
                    drawVertices[i] = point;
                }

                BillBoardUtils.AddTriangles(triangles, drawVertices, ref polyMat);
            }
        }

        /// <summary>
        /// Draws the given range of faces
        /// </summary>
        public virtual void Draw(Vector2 size, Vector2 origin, ref MatrixD matrix, Vector2I faceRange)
        {
            if (_sides > 2)
            {
                if (updateVertices)
                    GeneratePolygon();

                // Generate final vertices for drawing from unscaled vertices
                int max = drawVertices.Count - 1;
                var point = new Vector3D(origin + size * vertices[max], 0d);
                Vector3D.TransformNoProjection(ref point, ref matrix, out point);
                drawVertices[max] = point;

                for (int i = faceRange.X; i <= (faceRange.Y + 1); i++)
                {
                    point = new Vector3D(origin + size * vertices[i % max], 0d);
                    Vector3D.TransformNoProjection(ref point, ref matrix, out point);
                    drawVertices[i % max] = point;
                }

                faceRange *= 3;
                BillBoardUtils.AddTriangleRange(faceRange, triangles, drawVertices, ref polyMat);
            }
        }

        protected virtual void GeneratePolygon()
        {
            GenerateVertices();
            GenerateTextureCoordinates();
            GenerateTriangles();
            drawVertices.Clear();

            for (int i = 0; i < vertices.Count; i++)
                drawVertices.Add(Vector3D.Zero);
        }

        protected virtual void GenerateTriangles()
        {
            int max = vertices.Count - 1;
            triangles.Clear();
            triangles.EnsureCapacity(_sides * 3);

            for (int i = 0; i < vertices.Count - 1; i++)
            {
                triangles.Add(max);
                triangles.Add(i);
                triangles.Add((i + 1) % max);
            }
        }

        protected virtual void GenerateTextureCoordinates()
        {
            polyMat.texCoords.Clear();
            polyMat.texCoords.EnsureCapacity(vertices.Count);

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector2 uv = vertices[i];
                uv.Y *= -1f;
                uv += 0.5f * Vector2.One;

                polyMat.texCoords.Add(uv);
            }
        }

        protected virtual void GenerateVertices()
        {
            float rotStep = (float)(Math.PI * 2f / _sides),
                rotPos = -.5f * rotStep;

            vertices.Clear();
            vertices.EnsureCapacity(_sides + 1);

            for (int i = 0; i < _sides; i++)
            {
                Vector2 point = Vector2.Zero;
                point.X = (float)Math.Cos(rotPos);
                point.Y = (float)Math.Sin(rotPos);
                vertices.Add(.5f * point);
                rotPos += rotStep;
            }

            vertices.Add(Vector2.Zero);
        }
    }
}