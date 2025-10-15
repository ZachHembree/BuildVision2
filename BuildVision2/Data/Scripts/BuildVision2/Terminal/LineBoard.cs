using RichHudFramework;
using RichHudFramework.UI.Rendering;
using System.Collections.Generic;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public class LineBoard
    {
        /// <summary>
        /// RGBA line color
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Thickness of the line in meters
        /// </summary>
        public float Thickness { get; set; }

        /// <summary>
        /// Triangle indices for two quads
        /// </summary>
        private static readonly int[] indices = { 
            0, 1, 2, 
            3, 0, 2,
            4, 5, 6,
            7, 4, 6
        };

        private readonly Vector3D[] vertices;
        private PolyMaterial material;
        private Color _color;

        public LineBoard()
        {
            Color = new Color(255, 255, 255, 180);
            Thickness = 0.03f; // 3cm across
            vertices = new Vector3D[8];
            material = PolyMaterial.Default;
            material.texCoords = new List<Vector2>(8);

            for (int i = 0; i < 8; i++)
                material.texCoords.Add(Vector2.Zero);
        }

        /// <summary>
        /// Draws a line in world space between the From and To vectors in the given LineD
        /// </summary>
        public void Draw(LineD line)
        {
            Vector3D forward = line.Direction;
            Vector3D right = 0.5d * Thickness * Vector3D.Cross(forward, new Vector3D(1d, 0d, 0d)).Normalized(),
                up = 0.5d * Thickness * Vector3D.Cross(forward, right).Normalized();

            // Horizontal
            vertices[0] = line.From - right;
            vertices[1] = line.To - right;
            vertices[2] = line.To + right;
            vertices[3] = line.From + right;

            // Vertical
            vertices[4] = line.From - up;
            vertices[5] = line.To - up;
            vertices[6] = line.To + up;
            vertices[7] = line.From + up;

            if (_color != Color)
                material.bbColor = Color.GetBbColor();

            BillBoardUtils.AddTriangles(indices, vertices, ref material);
        }
    }
}
