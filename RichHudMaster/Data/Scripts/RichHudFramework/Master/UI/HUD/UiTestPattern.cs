using VRageMath;

namespace RichHudFramework.UI
{
    /// <summary>
    /// Pattern of textured boxes used to test scaling and positioning.
    /// </summary>
    public class UiTestPattern
    {
        private readonly TexturedBox[] testPattern;
        private bool visible;

        public UiTestPattern()
        {
            visible = false;

            testPattern = new TexturedBox[]
            {
                    new TexturedBox() // purple
                    {
                        Color = new Color(170, 0, 210, 255),
                        Height = 100,
                        Width = 100,
                        Offset = new Vector2(300, 0)
                    },
                    new TexturedBox() // green
                    {
                        Color = new Color(0, 255, 0, 255),
                        Height = 100,
                        Width = 100,
                        Offset = new Vector2(-300, 0)
                    },
                    new TexturedBox() // blue
                    {
                        Color = new Color(0, 0, 255, 255),
                        Height = 100,
                        Width = 100,
                        Offset = new Vector2(0, 300)
                    },
                    new TexturedBox() // red
                    {
                        Color = new Color(255, 0, 0, 255),
                        Height = 100,
                        Width = 100,
                        Offset = new Vector2(0, -300)
                    },
                    new TexturedBox() // yellow
                    {
                        Color = new Color(210, 190, 0, 255),
                        Height = 100,
                        Width = 100,
                        Offset = new Vector2(0, 0)
                    },
                    // sqrt(50) x sqrt(50)
                    new TexturedBox() // green
                    {
                        Color = new Color(0, 255, 0, 255),
                        Height = 100,
                        Width = 100,
                        Offset = new Vector2(-200, -200),
                        Scale = .5f
                    },
                    new TexturedBox() // blue
                    {
                        Color = new Color(0, 0, 255, 255),
                        Height = 100,
                        Width = 100,
                        Offset = new Vector2(-200, 200),
                        Scale = .5f
                    },
                    new TexturedBox() // purple
                    {
                        Color = new Color(170, 0, 210, 255),
                        Height = 100,
                        Width = 100,
                        Offset = new Vector2(200, 200),
                        Scale = .5f
                    },
                    new TexturedBox() // red
                    {
                        Color = new Color(255, 0, 0, 255),
                        Height = 100,
                        Width = 100,
                        Offset = new Vector2(200, -200),
                        Scale = .5f
                    },
                    // 50 x 50
                    new TexturedBox() // green
                    {
                        Color = new Color(0, 255, 0, 255),
                        Height = 100,
                        Width = 100,
                        Offset = new Vector2(-400, -400),
                        Scale = .25f
                    },
                    new TexturedBox() // blue
                    {
                        Color = new Color(0, 0, 255, 255),
                        Height = 100,
                        Width = 100,
                        Offset = new Vector2(-400, 400),
                        Scale = .25f
                    },
                    new TexturedBox() // purple
                    {
                        Color = new Color(170, 0, 210, 255),
                        Height = 100,
                        Width = 100,
                        Offset = new Vector2(400, 400),
                        Scale = .25f
                    },
                    new TexturedBox() // red
                    {
                        Color = new Color(255, 0, 0, 255),
                        Height = 100,
                        Width = 100,
                        Offset = new Vector2(400, -400),
                        Scale = .25f
                    },
            };
        }

        public void Toggle()
        {
            if (visible)
                Hide();
            else
                Show();
        }

        public void Show()
        {
            foreach (TexturedBox box in testPattern)
                box.Visible = true;

            visible = true;
        }

        public void Hide()
        {
            foreach (TexturedBox box in testPattern)
                box.Visible = false;

            visible = false;
        }
    }
}