﻿using System;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI.Rendering
{
    /// <summary>
    /// Renders a 2D polygon using billboards with the center punched out.
    /// </summary>
    public class PuncturedPolyBoard : PolyBoard
    {
        /// <summary>
        /// Scaled inner radius as fractional offset from exterior edge.
        /// </summary>
        public float InnerRadius
        {
            get { return _innerRadius; }
            set
            {
                if (value != _innerRadius)
                    updateVertices = true;

                _innerRadius = value;
            }
        }

        private float _innerRadius;

        public PuncturedPolyBoard()
        {
            _innerRadius = 0.8f;
        }

        /// <summary>
        /// Draws the given range of faces
        /// </summary>
        public override void Draw(Vector2 size, Vector2 origin, ref MatrixD matrix, Vector2I faceRange)
        {
            if (_sides > 2)
            {
                if (updateVertices)
                    GeneratePolygon();

                faceRange *= 2;

                // Generate final vertices for drawing from unscaled vertices
                for (int i = faceRange.X; i <= (faceRange.Y + 3); i++)
                {
                    var point = new Vector3D(origin + size * vertices[i % drawVertices.Count], 0d);
                    Vector3D.TransformNoProjection(ref point, ref matrix, out point);
                    drawVertices[i % drawVertices.Count] = point;
                }

                faceRange *= 3;
                var range = new Vector2I(faceRange.X, faceRange.Y + 3);
                BillBoardUtils.AddTriangleRange(range, triangles, drawVertices, ref polyMat);
            }
        }

        protected override void GenerateTriangles()
        {
            int max = vertices.Count;
            triangles.Clear();
            triangles.EnsureCapacity(_sides * 3 * 2);

            for (int i = 0; i < vertices.Count; i += 2)
            {
                int outerStart = i,
                    innerStart = (i + 1) % max,
                    outerEnd = (i + 2) % max,
                    innerEnd = (i + 3) % max;

                // Left Upper
                triangles.Add(outerStart);
                triangles.Add(outerEnd);
                triangles.Add(innerStart);
                // Right Lower
                triangles.Add(outerEnd);
                triangles.Add(innerEnd);
                triangles.Add(innerStart);
            }
        }

        protected override void GenerateVertices()
        {
            float rotStep = (float)(Math.PI * 2f / _sides),
                rotPos = -.5f * rotStep;

            _innerRadius = Math.Min(1f - 0.01f, _innerRadius);
            vertices.Clear();
            vertices.EnsureCapacity(_sides * 2);

            for (int i = 0; i < _sides; i++)
            {
                Vector2 outerStart = Vector2.Zero;
                outerStart.X = (float)Math.Cos(rotPos);
                outerStart.Y = (float)Math.Sin(rotPos);

                Vector2 innerStart = outerStart * _innerRadius;

                vertices.Add(.5f * outerStart);
                vertices.Add(.5f * innerStart);
                rotPos += rotStep;
            }
        }
    }
}