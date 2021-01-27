using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        /// <summary>
        /// Property for numerical values. Allows scrolling and text input.
        /// </summary>
        private abstract class NumericPropertyBase<TValue> : ScrollablePropertyBase<ITerminalProperty<TValue>, TValue>, IBlockTextMember
        {
            public Func<char, bool> CharFilterFunc { get; }

            public NumericPropertyBase()
            {
                CharFilterFunc = FilterCharInput;
            }

            protected virtual bool FilterCharInput(char x) =>
                (x >= '0' && x <= '9') || x == '.' || x == ',' || x == 'E' || x == 'e' || x == '-' || x == '+';

            public void SetValueText(string value)
            {
                TValue newValue;

                if (TryParseValue(value, out newValue))
                    SetValue(newValue);
            }
        }
    }
}