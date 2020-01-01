using RichHudFramework.UI.Rendering;
using System;
using System.Text;
using VRage;
using GlyphFormatMembers = VRage.MyTuple<VRageMath.Vector2I, int, VRageMath.Color, float>;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework.UI.Server
{
    using RichStringMembers = MyTuple<StringBuilder, GlyphFormatMembers>;
    using ControlMembers = MyTuple<
        ApiMemberAccessor, // GetOrSetMember
        object // ID
    >;

    public abstract class TerminalControlBase : HudElementBase, IListBoxEntry, ITerminalControl
    {
        public abstract event Action OnControlChanged;
        public abstract RichText Name { get; set; }
        public bool Enabled { get; set; }

        public TerminalControlBase(IHudParent parent) : base(parent)
        {
            Enabled = true;
        }

        /// <summary>
        /// Faciltates access to object members via the Framework API.
        /// </summary>
        public new ControlMembers GetApiData()
        {
            return new ControlMembers()
            {
                Item1 = GetOrSetMember,
                Item2 = this
            };
        }

        protected virtual object GetOrSetMember(object data, int memberEnum)
        {
            var member = (TerminalControlAccessors)memberEnum;

            switch (member)
            {
                case TerminalControlAccessors.OnSettingChanged:
                    {
                        var eventData = (MyTuple<bool, Action>)data;

                        if (eventData.Item1)
                            OnControlChanged += eventData.Item2;
                        else
                            OnControlChanged -= eventData.Item2;

                        break;
                    }
                case TerminalControlAccessors.Name:
                    {
                        if (data == null)
                            return Name.GetApiData();
                        else
                            Name = new RichText((RichStringMembers[])data);

                        break;
                    }
                case TerminalControlAccessors.Enabled:
                    {
                        if (data == null)
                            return Enabled;
                        else
                            Enabled = (bool)data;

                        break;
                    }
            }

            return null;
        }
    }

    public abstract class TerminalValue<T> : TerminalControlBase, ITerminalValue<T>
    {
        public virtual T Value { get; set; }

        public TerminalValue(IHudParent parent) : base(parent)
        { }

        protected override object GetOrSetMember(object data, int memberEnum)
        {
            if (memberEnum < 8)
                return base.GetOrSetMember(data, memberEnum);
            else
            {
                var member = (TerminalControlAccessors)memberEnum;

                if (member == TerminalControlAccessors.Value)
                {
                    if (data == null)
                        return Value;
                    else
                        Value = (T)data;
                }
            }

            return null;
        }
    }
}