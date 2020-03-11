using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;

namespace DarkHelmet.BuildVision2
{
    internal partial class PropertyBlock
    {
        private abstract class ScrollablePropertyBase<TProp, TValue> : BvTerminalProperty<TProp, TValue>, IBlockScrollable where TProp : ITerminalProperty<TValue>
        {
            protected ScrollablePropertyBase(string name, TProp property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            { }

            public abstract void ScrollUp();

            public abstract void ScrollDown();
        }

        private abstract class ScrollableValueControlBase<TProp, TValue> : BvTerminalValueControl<TProp, TValue>, IBlockScrollable where TProp : IMyTerminalValueControl<TValue>
        {
            protected ScrollableValueControlBase(string name, TProp property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            { }

            public abstract void ScrollUp();

            public abstract void ScrollDown();
        }
    }
}