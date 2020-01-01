using RichHudFramework.UI.Rendering;
using System;
using System.Text;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace RichHudFramework.UI.Server
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    internal enum OnOffButtonAccessors : int
    {
        OnText = 16,
        OffText = 17,
    }

    /// <summary>
    /// Boolean toggle designed to mimic the appearance of the On/Off button in the SE Terminal.
    /// </summary>
    public class OnOffButton : TerminalValue<bool>
    {
        public override event Action OnControlChanged;

        public override RichText Name { get { return NameBuilder.GetText(); } set { NameBuilder.SetText(value); } }
        public ITextBuilder NameBuilder => name.TextBoard;

        public override bool Value { get { return base.Value; } set { base.Value = value; OnControlChanged?.Invoke(); } }
        public RichText OnText { get { return on.Name; } set { on.Name = value; } }
        public RichText OffText { get { return off.Name; } set { off.Name = value; } }

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
            name = new Label(this)
            {
                Format = ModMenu.ControlText,
                Text = "NewOnOffButton",
                AutoResize = false,
                Height = 22f,
                Padding = new Vector2(0f, 0f),
                ParentAlignment = ParentAlignments.Top | ParentAlignments.InnerV
            };

            on = new TerminalButton()
            {
                Name = new RichText("On", new GlyphFormat(Color.White, TextAlignment.Center)),
                HighlightEnabled = false
            };

            off = new TerminalButton()
            {
                Name = new RichText("Off", new GlyphFormat(Color.White, TextAlignment.Center)),
                HighlightEnabled = false
            };

            buttonChain = new HudChain<HudElementBase>(name)
            {
                AutoResize = true,
                ParentAlignment = ParentAlignments.Bottom,
                Padding = new Vector2(164f, 0f),
                Spacing = 9f,
                ChildContainer = { on, off }
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

        protected override object GetOrSetMember(object data, int memberEnum)
        {
            if (memberEnum < 16)
                return base.GetOrSetMember(data, memberEnum);
            else
            {
                var member = (OnOffButtonAccessors)memberEnum;

                switch (member)
                {
                    case OnOffButtonAccessors.OffText:
                        {
                            if (data == null)
                                return OffText.GetApiData();
                            else
                                OffText = new RichText((RichStringMembers[])data);

                            break;
                        }
                    case OnOffButtonAccessors.OnText:
                        {
                            if (data == null)
                                return OnText.GetApiData();
                            else
                                OnText = new RichText((RichStringMembers[])data);

                            break;
                        }
                }

                return null;
            }
        }
    }
}