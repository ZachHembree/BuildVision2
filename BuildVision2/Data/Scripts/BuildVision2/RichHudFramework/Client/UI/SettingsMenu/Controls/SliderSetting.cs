using System;
using System.Collections.Generic;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<byte, float, VRageMath.Vector2I, VRageMath.Color>;
using ApiMemberAccessor = System.Func<object, int, object>;
using EventAccessor = VRage.MyTuple<bool, System.Action>;

namespace RichHudFramework.UI.Client
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

    public class SliderSetting : TerminalValue<float, SliderSetting>
    {
        public float Min
        {
            get { return (float)GetOrSetMemberFunc(null, (int)SliderSettingsAccessors.Min); }
            set { GetOrSetMemberFunc(value, (int)SliderSettingsAccessors.Min); }
        }

        public float Max
        {
            get { return (float)GetOrSetMemberFunc(null, (int)SliderSettingsAccessors.Max); }
            set { GetOrSetMemberFunc(value, (int)SliderSettingsAccessors.Max); }
        }

        public float Percent
        {
            get { return (float)GetOrSetMemberFunc(null, (int)SliderSettingsAccessors.Percent); }
            set { GetOrSetMemberFunc(value, (int)SliderSettingsAccessors.Percent); }
        }

        public RichText ValueText
        {
            get { return new RichText(GetOrSetMemberFunc(null, (int)SliderSettingsAccessors.ValueText) as IList<RichStringMembers>); }
            set { GetOrSetMemberFunc(value.ApiData, (int)SliderSettingsAccessors.ValueText); }
        }

        public SliderSetting() : base(MenuControls.SliderSetting)
        { }
    }
}