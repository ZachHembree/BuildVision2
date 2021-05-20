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
            public override StringBuilder Display { get { CleanText(GetValue(), valueBuilder); return valueBuilder; } }

            public override StringBuilder Status => null;

            public Func<char, bool> CharFilterFunc { get; }

            protected readonly StringBuilder valueBuilder;
            protected BvPropPool<TextProperty> poolParent;

            public TextProperty()
            {
                CharFilterFunc = FilterCharInput;
                valueBuilder = new StringBuilder();
            }

            public override void SetProperty(StringBuilder name, ITerminalProperty<StringBuilder> property, IMyTerminalControl control, PropertyBlock block)
            {
                base.SetProperty(name, property, control, block);

                if (poolParent == null)
                    poolParent = block.textPropPool;
            }

            public override void Reset()
            {
                base.Reset();
                valueBuilder.Clear();
            }

            public override void Return()
            {
                poolParent.Return(this);
            }

            public static TextProperty GetProperty(StringBuilder name, ITerminalProperty<StringBuilder> property, IMyTerminalControl control, PropertyBlock block)
            {
                TextProperty prop = block.textPropPool.Get();
                prop.SetProperty(name, property, control, block);

                return prop;
            }

            public void SetValueText(string text)
            {
                valueBuilder.Clear();
                valueBuilder.Append(text);

                SetValue(valueBuilder);
            }

            public override bool TryParseValue(string valueData, out StringBuilder value)
            {
                valueBuilder.Clear();
                valueBuilder.Append(valueData);
                value = valueBuilder;

                return true;
            }

            protected virtual bool FilterCharInput(char x) =>
                (x >= ' ');
        }
    }
}