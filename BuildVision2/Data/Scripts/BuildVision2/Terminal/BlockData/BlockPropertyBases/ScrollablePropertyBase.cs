using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        private abstract class ScrollablePropertyBase<TProp, TValue> : BvTerminalProperty<TProp, TValue>, IBlockScrollable 
            where TProp : class, ITerminalProperty<TValue>
        {
            public abstract void ScrollUp();

            public abstract void ScrollDown();
        }

        private abstract class ScrollableValueControlBase<TProp, TValue> : BvTerminalValueControl<TProp, TValue>, IBlockScrollable 
            where TProp : class, IMyTerminalValueControl<TValue>
        {
            public abstract void ScrollUp();

            public abstract void ScrollDown();
        }
    }
}