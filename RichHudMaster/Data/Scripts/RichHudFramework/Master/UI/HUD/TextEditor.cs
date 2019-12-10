using VRageMath;
using DarkHelmet.UI.Rendering;

namespace DarkHelmet.UI
{
    using Rendering.Client;
    using Rendering.Server;

    public class TextEditor : WindowBase
    {
        protected readonly ToolBar toolbar;
        protected readonly TextBox textBox;
        private static readonly float[] textSizes = new float[] { .75f, .875f, 1f, 1.125f, 1.25f, 1.375f, 1.5f };

        public TextEditor(bool wordWrapping, IHudParent parent = null) : base(parent)
        {
            IFont times = FontManager.GetFont("TimesNewRoman");
            Dropdown fontList = new Dropdown()
            {
                Height = 24f,
                Width = 140f,
                //UseMouseInput = false
            },
            sizeList = new Dropdown()
            {
                Height = 24f,
                Width = 60f,
            };

            foreach (IFontMin font in FontManager.Fonts)
                fontList.Add(new RichText(font.Name, new GlyphFormat(fontStyle: new Vector2I(font.Index, 0), color: Color.White)), font.Index);

            for (int n = 0; n < textSizes.Length; n++)
                sizeList.Add(new RichText(textSizes[n].ToString(), GlyphFormat.Black.WithColor(Color.White)), n);

            TextBoxButton
                bold = new TextBoxButton("B", new GlyphFormat(Color.Black, TextAlignment.Center, 1.1625f, times[FontStyleEnum.Bold].GetIndex()))
                { AutoResize = false, Size = new Vector2(32f, 24f) },
                italic = new TextBoxButton("I", new GlyphFormat(Color.Black, TextAlignment.Center, 1.1625f, times[FontStyleEnum.BoldItalic].GetIndex()))
                { AutoResize = false, Size = new Vector2(32f, 24f) };

            textBox = new TextBox(header, wordWrapping)
            {
                ParentAlignment = ParentAlignment.Bottom,
                Padding = new Vector2(8f, 8f),
                Format = new GlyphFormat(Color.White, textSize: 1.1f),
                VertCenterText = false,
                AutoResize = false
            };

            textBox.Text.Append("Test");
            toolbar = new ToolBar(header)
            {
                ParentAlignment = ParentAlignment.Bottom,
                Height = 24f,
                Members =
                {
                    { fontList, true },
                    { sizeList, true },
                    { bold, true },
                    { italic, true }
                }
            };

            fontList.OnSelectionChanged += SetFont;
            sizeList.OnSelectionChanged += SetFontSize;
            bold.MouseInput.OnLeftClick += ToggleBold;
            italic.MouseInput.OnLeftClick += ToggleItalic;
        }

        protected void SetFont(int index)
        {
            GlyphFormat format = textBox.Format;
            Vector2I current = format.StyleIndex;

            if (FontManager.Fonts[index].IsStyleDefined(current.Y))
                textBox.Format = format.WithFont(new Vector2I(index, current.Y));
            else
                textBox.Format = format.WithFont(new Vector2I(index, 0));
        }

        private void SetFontSize(int index)
        {
            textBox.Format = textBox.Format.WithSize(textSizes[index]);
        }

        protected void ToggleBold()
        {
            GlyphFormat format = textBox.Format;
            Vector2I index = format.StyleIndex;

            int bold = (int)FontStyleEnum.Bold;

            if ((format.StyleIndex.Y & bold) == bold)
                index.Y -= bold;  
            else
                index.Y |= bold;

            if (FontManager.Fonts[index.X].IsStyleDefined(index.Y))
                textBox.Format = format.WithFont(index);
        }

        protected void ToggleItalic()
        {
            GlyphFormat format = textBox.Format;
            Vector2I index = format.StyleIndex;

            int italic = (int)FontStyleEnum.Italic;

            if ((format.StyleIndex.Y & italic) == italic)
                index.Y -= italic;
            else
                index.Y |= italic;

            if (FontManager.Fonts[index.X].IsStyleDefined(index.Y))
                textBox.Format = format.WithFont(index);
        }

        protected override void Draw()
        {
            base.Draw();

            textBox.Size = body.Size - new Vector2(12f, 12f + toolbar.Height);
            textBox.Offset = new Vector2(0f, -(4f * Scale) - toolbar.Height);
        }

        protected class ToolBar : TexturedBox
        {
            public HudChain<HudElementBase> Members { get; }

            public ToolBar(IHudParent parent = null) : base(parent)
            {
                Members = new HudChain<HudElementBase>(this)
                {
                    ParentAlignment = ParentAlignment.Left
                };
            }

            protected override void Draw()
            {
                base.Draw();
                Members.Height = Height;
            }
        }
    }
}