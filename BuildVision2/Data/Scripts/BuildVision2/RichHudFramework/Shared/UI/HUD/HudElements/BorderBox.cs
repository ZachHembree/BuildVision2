using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI
{
    public class BorderBox : HudElementBase
    {
        public Material Material { get { return hudBoard.Material; } set { hudBoard.Material = value; } }
        public MaterialAlignment MatAlignment { get { return hudBoard.MatAlignment; } set { hudBoard.MatAlignment = value; } }
        public Color Color { get { return hudBoard.Color; } set { hudBoard.Color = value; } }
        public float Thickness { get { return thickness * Scale; } set { thickness = value / Scale; } }

        private float thickness;
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
                hudBoard.Size = new Vector2(Thickness, Height);
                hudBoard.Draw(Origin + Offset + new Vector2(-Width / 2f, 0f));

                hudBoard.Size = new Vector2(Width, Thickness);
                hudBoard.Draw(Origin + Offset + new Vector2(0f, Height / 2f));

                hudBoard.Size = new Vector2(Thickness, Height);
                hudBoard.Draw(Origin + Offset + new Vector2(Width / 2f, 0f));

                hudBoard.Size = new Vector2(Width, Thickness);
                hudBoard.Draw(Origin + Offset + new Vector2(0f, -Height / 2f));
            }
        }
    }
}
