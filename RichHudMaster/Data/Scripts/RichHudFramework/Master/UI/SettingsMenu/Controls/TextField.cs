using System;
using RichHudFramework.UI.Rendering;
using VRageMath;

namespace RichHudFramework.UI.Server
{
    public class TextField : TerminalValue<string>
    {
        public override event Action OnControlChanged;

        public override float Width
        {
            get { return background.Width; }
            set
            {
                background.Width = value;
                name.Width = value;
            }
        }
        public override float Height
        {
            get { return background.Height + name.Height; }
            set { background.Height = value - name.Height; }
        }

        public override RichText Name { get { return name.TextBoard.GetText(); } set { name.TextBoard.SetText(value); } }
        public override string Value
        {
            get { return textBox.TextBoard.GetText().ToString(); }
            set
            {
                textBox.TextBoard.SetText(value);
                OnControlChanged?.Invoke();
            }
        }

        private readonly Label name;
        private readonly TextBox textBox;
        private readonly TexturedBox background;
        private readonly BorderBox border;

        public TextField(IHudParent parent = null) : base(parent)
        {
            name = new Label(this)
            {
                Format = ModMenu.ControlText,
                Text = "NewTextField",
                AutoResize = false,
                Height = 22f,
                Padding = new Vector2(0f, 0f),
                ParentAlignment = ParentAlignments.Top | ParentAlignments.InnerV
            };

            background = new TexturedBox(name)
            {
                Color = new Color(42, 55, 63),
                ParentAlignment = ParentAlignments.Bottom,
            };

            border = new BorderBox(background)
            {
                Color = new Color(53, 66, 75),
                Thickness = 2f,
                DimAlignment = DimAlignments.Both,
            };

            textBox = new TextBox(background)
            {
                AutoResize = false,
                DimAlignment = DimAlignments.Both,
                Padding = new Vector2(24f, 0f),
            };

            textBox.TextBoard.SetText(new RichString("TextBox", GlyphFormat.White));
            Size = new Vector2(319f, 62f);
        }
    }
}