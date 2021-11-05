using Sandbox.ModAPI.Interfaces;
using Sandbox.ModAPI.Interfaces.Terminal;
using RichHudFramework;
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

            public override void SetProperty(StringBuilder name, ITerminalProperty<StringBuilder> property, PropertyBlock block)
            {
                base.SetProperty(name, property, block);

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

            public static TextProperty GetProperty(StringBuilder name, ITerminalProperty<StringBuilder> property, PropertyBlock block)
            {
                TextProperty prop = block.textPropPool.Get();
                prop.SetProperty(name, property, block);

                return prop;
            }

            public void SetValueText(string text)
            {
                valueBuilder.Clear();
                valueBuilder.Append(text);

                SetValue(valueBuilder);
            }

            public override PropertyData GetPropertyData()
            {
                byte[] valueData;

                if (Utils.ProtoBuf.TrySerialize(GetValue().ToString(), out valueData) == null)
                {
                    return new PropertyData(PropName.ToString(), valueData);
                }
                else
                    return default(PropertyData);
            }

            public override bool TryImportData(PropertyData data)
            {
                string value;

                if (Utils.ProtoBuf.TryDeserialize(data.valueData, out value) == null)
                {
                    SetValueText(value);
                    return true;
                }
                else
                    return false;
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