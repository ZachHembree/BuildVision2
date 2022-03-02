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
            where TValue : IEquatable<TValue>
        {
            /// <summary>
            /// Get/sets the value associated with the property
            /// </summary>
            public override TValue Value
            {
                get { return _value; }
                set
                {
                    if (!_value.Equals(value))
                        valueChanged = true;

                    _value = value;
                }
            }

            /// <summary>
            /// Delegate used for filtering text input. Returns true if a given character is in the accepted range.
            /// </summary>
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
                    Value = newValue;
            }
        }
    }
}