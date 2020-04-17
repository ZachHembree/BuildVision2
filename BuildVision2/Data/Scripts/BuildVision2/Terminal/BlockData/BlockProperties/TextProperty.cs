using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Text;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        /// <summary>
        /// Field for changing block property text. 
        /// </summary>
        private class TextProperty : BvTerminalProperty<ITerminalProperty<StringBuilder>, StringBuilder>, IBlockTextMember
        {
            public override string Value => CleanText(GetValue());
            public override string Postfix => null;
            public Func<char, bool> CharFilterFunc { get; protected set; }

            public TextProperty(string name, ITerminalProperty<StringBuilder> textProp, IMyTerminalControl control, SuperBlock block) : base(name, textProp, control, block)
            {
                CharFilterFunc = x => (x >= ' ');
            }

            public void SetValueText(string text)
            {
                SetValue(new StringBuilder(text));
            }

            public override bool TryParseValue(string valueData, out StringBuilder value)
            {
                value = new StringBuilder(valueData);
                return true;
            }
        }
    }
}