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
        private abstract class NumericPropertyBase<TValue> : BvTerminalProperty<ITerminalProperty<TValue>, TValue>, IBlockTextMember
        {
            public Func<char, bool> CharFilterFunc { get; }

            public bool CanUseMultipliers { get; protected set; }

            public bool IsInteger { get; protected set; }

            public NumericPropertyBase()
            {
                CharFilterFunc = FilterCharInput;
                CanUseMultipliers = true;
                IsInteger = false;
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