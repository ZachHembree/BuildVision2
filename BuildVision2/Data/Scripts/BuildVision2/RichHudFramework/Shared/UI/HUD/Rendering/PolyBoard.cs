using System;
using System.Collections.Generic;
using VRageMath;

namespace RichHudFramework.UI.Rendering
{
	/// <summary>
	/// Renders a regular 2D polygon (e.g., triangle, hexagon, circle approximation) using billboards.
	/// The shape is constructed as a triangle fan from the center.
	/// </summary>
	public class PolyBoard
	{
		/// <summary>
		/// The color tint applied to the polygon material.
		/// </summary>
		public virtual Color Color
		{
			get { return _color; }
			set
			{
				if (value != _color)
					polyMat.bbColor = value.GetBbColor();

				_color = value;
			}
		}

		/// <summary>
		/// The texture applied to the polygon.
		/// </summary>
		public virtual Material Material
		{
			get { return matFrame.Material; }
			set
			{
				if (value != matFrame.Material)
				{
					updateMatFit = true;
					matFrame.Material = value;
					polyMat.textureID = value.TextureID;
				}
			}
		}

		/// <summary>
		/// Determines how the texture is scaled to fit the polygon's bounding box.
		/// </summary>
		public MaterialAlignment MatAlignment
		{
			get { return matFrame.Alignment; }
			set
			{
				if (value != matFrame.Alignment)
				{
					updateMatFit = true;
					matFrame.Alignment = value;
				}
			}
		}

		/// <summary>
		/// The number of sides (vertices) on the polygon perimeter.
		/// Higher values approximate a circle.
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

		/// <exclude/>
		protected int _sides;

		/// <exclude/>
		protected Color _color;

		/// <summary>
		/// Internal flags used to indicate stale vertex positions and material alignment
		/// </summary>
		/// <exclude/>
		protected bool updateVertices, updateMatFit;

		/// <summary>
		/// Material for texturing an object with an arbitrary number of vertices
		/// </summary>
		/// <exclude/>
		protected PolyMaterial polyMat;

		/// <exclude/>
		protected readonly MaterialFrame matFrame;

		/// <summary>
		/// Unscaled internal geometry
		/// </summary>
		/// <exclude/>
		protected readonly List<int> triangles;
		/// <exclude/>
		protected readonly List<Vector2> vertices;

		/// <summary>
		/// Buffer for final scaled vertices
		/// </summary>
		/// <exclude/>
		protected readonly List<Vector2> drawVertices;

		public PolyBoard()
		{
			triangles = new List<int>();
			vertices = new List<Vector2>();
			drawVertices = new List<Vector2>();

			matFrame = new MaterialFrame();
			polyMat = PolyMaterial.Default;
			polyMat.texCoords = new List<Vector2>();

			_sides = 16;
			updateVertices = true;
		}

		/// <summary>
		/// Renders the full polygon defined by <see cref="Sides"/> with the specified dimensions and position.
		/// </summary>
		public virtual void Draw(Vector2 size, Vector2 origin, MatrixD[] matrixRef)
		{
			if (_sides > 2)
			{
				if (updateVertices)
					GeneratePolygon();
			}

			if (_sides > 2 && drawVertices.Count > 2)
			{
				if (updateMatFit)
				{
					polyMat.texBounds = matFrame.GetMaterialAlignment(size.X / size.Y);
					GenerateTextureCoordinates();
					updateMatFit = false;
				}

				// Generate final vertices for drawing from unscaled vertices
				for (int i = 0; i < drawVertices.Count; i++)
				{
					drawVertices[i] = origin + size * vertices[i];
				}

				BillBoardUtils.AddTriangles(triangles, drawVertices, ref polyMat, matrixRef);
			}
		}

		/// <summary>
		/// Renders a specific range of faces (pie slice) of the polygon.
		/// </summary>
		/// <param name="faceRange">The start and end index of the triangles to draw.</param>
		public virtual void Draw(Vector2 size, Vector2 origin, Vector2I faceRange, MatrixD[] matrixRef)
		{
			if (_sides > 2)
			{
				if (updateVertices)
					GeneratePolygon();
			}

			if (_sides > 2 && drawVertices.Count > 2)
			{
				if (updateMatFit)
				{
					polyMat.texBounds = matFrame.GetMaterialAlignment(size.X / size.Y);
					GenerateTextureCoordinates();
					updateMatFit = false;
				}

				// Generate final vertices for drawing from unscaled vertices
				int max = drawVertices.Count - 1;
				drawVertices[max] = origin + size * vertices[max];

				for (int i = 0; i < drawVertices.Count; i++)
				{
					drawVertices[i] = origin + size * vertices[i];
				}

				faceRange *= 3;
				BillBoardUtils.AddTriangleRange(faceRange, triangles, drawVertices, ref polyMat, matrixRef);
			}
		}

		/// <summary>
		/// Calculates the center offset of a specific slice of the polygon relative to the billboard center.
		/// Useful for radial menus or separated pie charts.
		/// </summary>
		public virtual Vector2 GetSliceOffset(Vector2 bbSize, Vector2I range)
		{
			if (updateVertices)
				GeneratePolygon();

			int max = vertices.Count;
			Vector2 start = vertices[range.X],
				end = vertices[(range.Y + 1) % max],
				center = Vector2.Zero;

			return bbSize * (start + end + center) / 3f;
		}

		/// <exclude/>
		protected virtual void GeneratePolygon()
		{
			GenerateVertices();
			GenerateTriangles();
			drawVertices.Clear();

			for (int i = 0; i < vertices.Count; i++)
				drawVertices.Add(Vector2.Zero);

			updateMatFit = true;
		}

		/// <exclude/>
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

		/// <exclude/>
		protected virtual void GenerateTextureCoordinates()
		{
			Vector2 texScale = polyMat.texBounds.Size,
				texCenter = polyMat.texBounds.Center;

			polyMat.texCoords.Clear();
			polyMat.texCoords.EnsureCapacity(vertices.Count);

			for (int i = 0; i < vertices.Count; i++)
			{
				Vector2 uv = vertices[i] * texScale;
				uv.Y *= -1f;

				polyMat.texCoords.Add(uv + texCenter);
			}
		}

		/// <exclude/>
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