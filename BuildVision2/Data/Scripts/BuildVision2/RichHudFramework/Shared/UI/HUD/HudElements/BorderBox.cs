﻿using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// A textured frame. The default texture is just a plain color.
    /// </summary>
    public class BorderBox : HudElementBase
    {
        /// <summary>
        /// Material applied to the box.
        /// </summary>
        public Material Material { get { return hudBoard.Material; } set { hudBoard.Material = value; } }

        /// <summary>
        /// Determines how the material reacts to changes in element size/aspect ratio.
        /// </summary>
        public MaterialAlignment MatAlignment { get { return hudBoard.MatAlignment; } set { hudBoard.MatAlignment = value; } }

        /// <summary>
        /// Coloring applied to the material.
        /// </summary>
        public Color Color { get { return hudBoard.Color; } set { hudBoard.Color = value; } }

        /// <summary>
        /// Size of the border on all four sides in pixels.
        /// </summary>
        public float Thickness { get { return _thickness * Scale; } set { _thickness = value / Scale; } }

        private float _thickness;
        protected readonly MatBoard hudBoard;

        public BorderBox(IHudParent parent = null) : base(parent)
        {
            hudBoard = new MatBoard();
            Thickness = 1f;
        }

        protected override void Draw()
        {
            if (Color.A > 0)
            {
                float height = _height * _scale, width = _width * _scale, 
                    thickness = _thickness * _scale;

                hudBoard.Size = new Vector2(thickness, height);
                hudBoard.Draw(cachedPosition + new Vector2(-width / 2f, 0f));

                hudBoard.Size = new Vector2(width, thickness);
                hudBoard.Draw(cachedPosition + new Vector2(0f, height / 2f));

                hudBoard.Size = new Vector2(thickness, height);
                hudBoard.Draw(cachedPosition + new Vector2(width / 2f, 0f));

                hudBoard.Size = new Vector2(width, thickness);
                hudBoard.Draw(cachedPosition + new Vector2(0f, -height / 2f));
            }
        }
    }
}
