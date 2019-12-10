using VRageMath;
using DarkHelmet.UI.Rendering;
using System;

namespace DarkHelmet.UI
{
    using Rendering.Client;
    using Rendering.Server;

    public class SettingsMenu : WindowBase
    {
        private Label headerLabel;
        private ScrollBox<IScrollBoxMember> chain;

        public SettingsMenu(IHudParent parent = null) : base(parent)
        {
            BodyColor = new Color(37, 46, 53);
            BorderColor = new Color(84, 98, 107);

            header.Height = 30f;
            headerLabel = new Label(new RichText("Settings Menu", GlyphFormat.White));
            Size = new Vector2(500f, 800f);

            chain = new ScrollBox<IScrollBoxMember>(this)
            {
                AutoResize = true,
                Width = 365f,
                Spacing = 60f,
                ChainContainer =
                {
                    { new OnOffButton(), true },
                    { new Checkbox(), true },
                    { new SliderSetting(), true },
                    {
                        new ListBox() {
                            new RichText("Item 1", GlyphFormat.White),
                            new RichText("Item 2", GlyphFormat.White),
                            new RichText("Item 3", GlyphFormat.White),
                            new RichText("Item 4", GlyphFormat.White),
                            new RichText("Item 5", GlyphFormat.White),
                            new RichText("Item 6", GlyphFormat.White),
                            new RichText("Item 7", GlyphFormat.White),
                            new RichText("Item 8", GlyphFormat.White),
                            new RichText("Item 9", GlyphFormat.White),
                            new RichText("Item 10", GlyphFormat.White),
                            new RichText("Item 11", GlyphFormat.White),
                            new RichText("Item 12", GlyphFormat.White),
                        }, true
                    },
                    {
                        new Dropdown() {
                            { new RichText("Item 1", GlyphFormat.White), 1 },
                            { new RichText("Item 2", GlyphFormat.White), 2 },
                            { new RichText("Item 3", GlyphFormat.White), 3 },
                            { new RichText("Item 4", GlyphFormat.White), 4 },
                            { new RichText("Item 5", GlyphFormat.White), 5 },
                            { new RichText("Item 6", GlyphFormat.White), 6 },
                            { new RichText("Item 7", GlyphFormat.White), 7 },
                            { new RichText("Item 8", GlyphFormat.White), 8 },
                            { new RichText("Item 9", GlyphFormat.White), 9 },
                            { new RichText("Item 10", GlyphFormat.White), 10 },
                            { new RichText("Item 11", GlyphFormat.White), 11 },
                            { new RichText("Item 12", GlyphFormat.White), 12 },
                        }, true
                    },
                    //{ new ColorPicker(), true }
                }
            };
        }

        protected override void Draw()
        {
            base.Draw();
            chain.Height = Height;
        }
    }

    public abstract class TerminalSetting<T> : PaddedElementBase, IScrollBoxMember
    {
        public event Action OnValueChanged;

        public abstract ITextBuilder Name { get; }
        public virtual T Value { get; set; }
        public bool Enabled => true;

        public TerminalSetting(IHudParent parent) : base(parent)
        { }
    }

    public class TerminalButton : TextBoxButton, IScrollBoxMember
    {
        private readonly BorderBox border;
        public bool Enabled => true;

        public TerminalButton(IHudParent parent = null) : base(parent)
        {
            Init();
            border = new BorderBox(this)
            { Color = new Color(53, 66, 75), Thickness = 2f, MatchParentSize = true };

            Color = new Color(42, 55, 63);
            Size = new Vector2(253f, 55f);
        }

        public TerminalButton(RichText text, IHudParent parent = null, bool wordWrapping = false) : this(parent)
        {
            Text.SetText(text);
        }

        public TerminalButton(RichString text, IHudParent parent = null, bool wordWrapping = false) : this(parent)
        {
            Text.SetText(text);
        }

        public TerminalButton(string text, GlyphFormat format, IHudParent parent = null, bool wordWrapping = false) : this(parent)
        {
            Format = format;
            Text.SetText(text);
        }
    }

    /// <summary>
    /// Boolean toggle designed to mimic the appearance of the On/Off button in the SE Terminal.
    /// </summary>
    public class OnOffButton : TerminalSetting<bool>
    {
        public override ITextBuilder Name => name.Text;
        public ITextBuilder OnText => on.Text;
        public ITextBuilder OffText => off.Text;

        public override float Width
        {
            get { return buttonChain.Width; }
            set
            {
                name.Width = value;
                value = Math.Max(value - buttonChain.Padding.X - buttonChain.Spacing, 8f) / 2f;

                on.Width = value;
                off.Width = value;
            }
        }
        public override float Height
        {
            get { return buttonChain.Height + name.Height; }
            set { buttonChain.Height = value - name.Height; }
        }

        private readonly Label name;
        private readonly TerminalButton on, off;
        private readonly BorderBox selectionHighlight;
        private readonly HudChain<HudElementBase> buttonChain;

        public OnOffButton(IHudParent parent = null) : base(parent)
        {
            name = new Label("NewOnOffButton", GlyphFormat.White, this)
            {
                AutoResize = false,
                Height = 22f,
                Padding = new Vector2(0f, 0f),
                ParentAlignment = ParentAlignment.Top | ParentAlignment.Left | ParentAlignment.Inner
            };

            on = new TerminalButton("On", new GlyphFormat(Color.White, TextAlignment.Center))
            { AutoResize = false };

            off = new TerminalButton("Off", new GlyphFormat(Color.White, TextAlignment.Center))
            { AutoResize = false };

            buttonChain = new HudChain<HudElementBase>(name)
            {
                AutoResize = true,
                ParentAlignment = ParentAlignment.Bottom,
                Padding = new Vector2(164f, 0f),
                Spacing = 9f,
                ChildContainer =
                {
                    { on, true },
                    { off, true }
                }
            };

            on.MouseInput.OnLeftClick += ToggleValue;
            off.MouseInput.OnLeftClick += ToggleValue;
            selectionHighlight = new BorderBox(buttonChain) { Color = Color.White };

            Size = new Vector2(319f, 72f);
        }

        private void ToggleValue()
        {
            Value = !Value;
        }

        protected override void Draw()
        {
            if (Value)
            {
                selectionHighlight.Size = on.Size;
                selectionHighlight.Offset = on.Offset;
            }
            else
            {
                selectionHighlight.Size = off.Size;
                selectionHighlight.Offset = off.Offset;
            }
        }
    }

    /// <summary>
    /// Creates a named checkbox designed to mimic the appearance of checkboxes in the SE terminal.
    /// </summary>
    public class Checkbox : HudElementBase, IScrollBoxMember
    {
        public override float Width
        {
            get { return chain.Width; }
            set { name.Width = value - box.Width - chain.Spacing; }
        }
        public override float Height { get { return chain.Height; } set { chain.Height = value; } }
        public bool Value { get { return box.Visible; } set { box.Visible = value; } }
        public bool Enabled => true;

        private readonly HudChain<HudElementBase> chain;
        private readonly Label name;
        private readonly Button button;
        private readonly TexturedBox box;

        public Checkbox(IHudParent parent = null) : base(parent)
        {
            name = new Label("NewCheckbox", new GlyphFormat(Color.White, TextAlignment.Right));

            button = new Button()
            {
                Size = new Vector2(37f, 36f),
                Color = new Color(39, 52, 60),
                highlightColor = new Color(50, 60, 70),
                ChildContainer =
                {
                    new BorderBox()
                    { Color = new Color(53, 66, 75), Thickness = 2f, MatchParentSize = true }
                }
            };

            box = new TexturedBox(button)
            {
                MatchParentSize = true,
                Padding = new Vector2(16f, 16f),
                Color = new Color(114, 121, 139)
            };

            chain = new HudChain<HudElementBase>(this)
            {
                AutoResize = true,
                Spacing = 17f,
                ChildContainer =
                {
                    { name, true },
                    { button, true }
                }
            };

            button.MouseInput.OnLeftClick += ToggleValue;
            Height = 36f;
        }

        private void ToggleValue()
        {
            Value = !Value;
        }
    }

    public class SliderSetting : SliderBox, IScrollBoxMember
    {
        public ITextBuilder Name => name.Text;
        public ITextBuilder Value => value.Text;
        public bool Enabled => true;

        private readonly Label name, value;

        public SliderSetting(IHudParent parent = null) : base(parent)
        {
            name = new Label("NewSlideBox", GlyphFormat.White, background)
            { ParentAlignment = ParentAlignment.InnerH | ParentAlignment.Top | ParentAlignment.Left };

            value = new Label("Value", GlyphFormat.White, background)
            { ParentAlignment = ParentAlignment.InnerH | ParentAlignment.Top | ParentAlignment.Right };
        }
    }

    public class SliderBox : PaddedElementBase, IScrollBoxMember
    {
        public override float Width
        {
            get { return background.Width; }
            set
            {
                background.Width = value;
                slide.Width = value - Padding.X;
            }
        }
        public override float Height
        {
            get { return background.Height; }
            set
            {
                background.Height = value;
                slide.Height = value - Padding.Y;
            }
        }
        public override Vector2 Padding
        {
            set
            {
                slide.Size = Size - value;
                base.Padding = value;
            }
        }
        public bool Enabled => true;

        public float Min { get { return slide.Min; } set { slide.Min = value; } }
        public float Max { get { return slide.Max; } set { slide.Max = value; } }
        public float Current { get { return slide.Current; } set { slide.Current = value; } }
        public float Percent { get { return slide.Percent; } set { slide.Percent = value; } }

        protected readonly TexturedBox background;
        protected readonly BorderBox border;
        protected readonly SliderBar slide;

        public SliderBox(IHudParent parent = null) : base(parent)
        {
            background = new TexturedBox(this)
            { Color = new Color(41, 54, 62) };

            border = new BorderBox(background)
            { Color = new Color(53, 66, 75), Thickness = 2f, MatchParentSize = true };

            slide = new SliderBar(background);
            slide.button.Size = new Vector2(14f, 28f);
            slide.button.Color = new Color(103, 109, 124);

            slide.bar.Height = 5f;
            slide.bar.Color = new Color(103, 109, 124);

            Padding = new Vector2(18f, 18f);
            Size = new Vector2(317f, 47f);
        }
    }

    public class ColorPicker : TerminalSetting<Color>
    {
        public override ITextBuilder Name => name.Text;
        public override float Width
        {
            get { return mainChain.Width + Padding.X; }

            set
            {
                float scale = value / mainChain.Width;

                name.Width *= scale;
                display.Width *= scale;
                colorText.Width *= scale;
                colorSliders.Width *= scale;
            }
        }

        public override float Height
        {
            get { return mainChain.Height + Padding.Y; }

            set
            {
                float scale = value / mainChain.Height;

                displayChain.Height *= scale;
                rText.Height *= scale;
                gText.Height *= scale;
                bText.Height *= scale;

                r.Height *= scale;
                g.Height *= scale;
                b.Height *= scale;
            }
        }

        private readonly Label name, rText, gText, bText;
        private readonly TexturedBox display;
        private readonly SliderBox r, g, b;
        private readonly HudChain<HudElementBase> mainChain, displayChain,
            sliderChain, colorText, colorSliders;

        public ColorPicker(IHudParent parent = null) : base(parent)
        {
            name = new Label("NewColorPicker", GlyphFormat.White) { AutoResize = false, Size = new Vector2(88f, 22f) };

            display = new TexturedBox()
            {
                Width = 231f,
                ChildContainer =
                {
                    new BorderBox()
                    { Color = Color.White, Thickness = 1f, MatchParentSize = true }
                }
            };

            displayChain = new HudChain<HudElementBase>()
            {
                AutoResize = true,
                Height = 22f,
                Spacing = 0f,
                ChildContainer =
                {
                    { name, true },
                    { display, true }
                }
            };

            rText = new Label() { AutoResize = false, Format = GlyphFormat.White, Height = 47f };
            gText = new Label() { AutoResize = false, Format = GlyphFormat.White, Height = 47f };
            bText = new Label() { AutoResize = false, Format = GlyphFormat.White, Height = 47f };

            colorText = new HudChain<HudElementBase>()
            {
                AlignVertical = true,
                AutoResize = true,
                Width = 87f,
                Spacing = 5f,
                ChildContainer =
                {
                    { rText, true },
                    { gText, true },
                    { bText, true }
                }
            };

            r = new SliderBox() { Min = 0f, Max = 255f, Height = 47f };
            g = new SliderBox() { Min = 0f, Max = 255f, Height = 47f };
            b = new SliderBox() { Min = 0f, Max = 255f, Height = 47f };

            colorSliders = new HudChain<HudElementBase>()
            {
                AlignVertical = true,
                AutoResize = true,
                Width = 231f,
                Spacing = 5f,
                ChildContainer =
                {
                    { r, true },
                    { g, true },
                    { b, true }
                }
            };

            sliderChain = new HudChain<HudElementBase>()
            {
                { colorText, true },
                { colorSliders, true }
            };

            mainChain = new HudChain<HudElementBase>(this)
            {
                AlignVertical = true,
                AutoResize = true,
                Spacing = 5f,
                Size = new Vector2(318f, 171f),
                ChildContainer =
                {
                    { displayChain, true },
                    { sliderChain, true }
                }
            };
        }

        protected override void Draw()
        {
            Color color = new Color()
            {
                R = (byte)r.Current.Round(0),
                G = (byte)g.Current.Round(0),
                B = (byte)b.Current.Round(0),
                A = 255
            };

            rText.Text.SetText($"R: {color.R}");
            gText.Text.SetText($"G: {color.G}");
            bText.Text.SetText($"B: {color.B}");

            display.Color = color;
        }
    }
}