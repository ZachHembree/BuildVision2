using RichHudFramework.UI.Rendering;
using System;
using System.Text;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;

namespace RichHudFramework.UI.Server
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;

    internal enum SliderSettingsAccessors : int
    {
        /// <summary>
        /// Float
        /// </summary>
        Min = 16,

        /// <summary>
        /// Float
        /// </summary>
        Max = 17,

        /// <summary>
        /// Float
        /// </summary>
        Percent = 18,

        /// <summary>
        /// RichStringMembers[]
        /// </summary>
        ValueText = 19,
    }

    public class SliderSetting : TerminalValue<float>
    {
        public override event Action OnControlChanged;

        public override float Width
        {
            get { return slide.Width; }
            set { slide.Width = value; }
        }

        public override float Height
        {
            get { return slide.Height; }
            set { slide.Height = value; }
        }

        public override Vector2 Padding
        {
            get { return slide.Padding; }
            set { slide.Padding = value; }
        }

        public float Min { get { return slide.Min; } set { slide.Min = value; } }
        public float Max { get { return slide.Max; } set { slide.Max = value; } }
        public override float Value { get { return slide.Current; } set { slide.Current = value; } }
        public float Percent { get { return slide.Percent; } set { slide.Percent = value; } }

        public override RichText Name { get { return name.TextBoard.GetText(); } set { name.TextBoard.SetText(value); } }
        public RichText ValueText { get { return current.TextBoard.GetText(); } set { current.TextBoard.SetText(value); } }

        private readonly Label name, current;
        private readonly SliderBox slide;
        private float lastValue;

        public SliderSetting(IHudParent parent = null) : base(parent)
        {
            slide = new SliderBox(this);

            name = new Label(this)
            {
                Format = ModMenu.ControlText,
                Text = "NewSlideBox",
                ParentAlignment = ParentAlignments.InnerH | ParentAlignments.Top | ParentAlignments.Left
            };

            current = new Label(this)
            {
                Format = ModMenu.ControlText,
                Text = "Value",
                ParentAlignment = ParentAlignments.InnerH | ParentAlignments.Top | ParentAlignments.Right
            };

            lastValue = Value;
        }

        protected override void Draw()
        {
            if (Value != lastValue)
            {
                OnControlChanged?.Invoke();
                lastValue = Value;
            }
        }

        protected override object GetOrSetMember(object data, int memberEnum)
        {
            if (memberEnum < 16)
                return base.GetOrSetMember(data, memberEnum);
            else
            {
                var member = (SliderSettingsAccessors)memberEnum;

                switch (member)
                {
                    case SliderSettingsAccessors.Min:
                        {
                            if (data == null)
                                return Min;
                            else
                                Min = (float)data;

                            break;
                        }
                    case SliderSettingsAccessors.Max:
                        {
                            if (data == null)
                                return Max;
                            else
                                Max = (float)data;

                            break;
                        }
                    case SliderSettingsAccessors.Percent:
                        {
                            if (data == null)
                                return Percent;
                            else
                                Percent = (float)data;

                            break;
                        }
                    case SliderSettingsAccessors.ValueText:
                        {
                            if (data == null)
                                return ValueText.GetApiData();
                            else
                                ValueText = new RichText((RichStringMembers[])data);

                            break;
                        }
                }

                return null;
            }
        }
    }
}