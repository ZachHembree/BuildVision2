using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;

namespace DarkHelmet.BuildVision2
{
    internal partial class PropertyBlock
    {
        /// <summary>
        /// Property for numerical values. Allows scrolling and text input.
        /// </summary>
        private abstract class NumericPropertyBase<TValue> : ScrollablePropertyBase<ITerminalProperty<TValue>, TValue>, IBlockTextMember
        {
            public Func<char, bool> CharFilterFunc { get; protected set; }

            public NumericPropertyBase(string name, ITerminalProperty<TValue> property, IMyTerminalControl control, IMyTerminalBlock block) : base(name, property, control, block)
            {
                CharFilterFunc = x => (x >= '0' && x <= '9') || x == '.' || x == '-' || x == '+';
            }

            public void SetValueText(string value)
            {
                TValue newValue;

                if (TryParseValue(value, out newValue))
                    SetValue(newValue);
            }
        }
    }
}