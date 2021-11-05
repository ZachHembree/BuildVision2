using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Text;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRageMath;

namespace DarkHelmet.BuildVision2
{
    public partial class PropertyBlock
    {
        /// <summary>
        /// Scrollable property for <see cref="IMyTerminalControlCombobox"/> terminal properties.
        /// </summary>
        private class ComboBoxProperty : ScrollableValueControlBase<IMyTerminalControlCombobox, long>
        {
            public override StringBuilder Display 
            {
                get 
                {
                    int i = (int)GetValue();

                    if (keys.Count > 0 && i >= 0 && i < keys.Count)
                        return names[i];
                    else
                        return null;
                }
            }

            public override StringBuilder Status 
            { 
                get
                {
                    if (GetPostfixFunc != null)
                    {
                        GetPostfixFunc(postfixBuilder);
                        return postfixBuilder;
                    }
                    else
                        return null;
                }
            }

            private readonly List<long> keys;
            private readonly List<StringBuilder> names;

            private Action<StringBuilder> GetPostfixFunc, GetChargePostfixFunc;
            private BvPropPool<ComboBoxProperty> poolParent;
            private readonly StringBuilder postfixBuilder;

            public ComboBoxProperty()
            {
                keys = new List<long>();
                names = new List<StringBuilder>();

                GetChargePostfixFunc = GetChargePostfix;
                postfixBuilder = new StringBuilder();
            }

            public override void SetProperty(StringBuilder name, IMyTerminalControlCombobox comboBox, PropertyBlock block)
            {
                base.SetProperty(name, comboBox, block);

                if (poolParent == null)
                    poolParent = block.comboPropPool;

                if (control.Id == "ChargeMode" && block.SubtypeId.UsesSubtype(TBlockSubtypes.Battery)) // Insert bat charge info
                    GetPostfixFunc = GetChargePostfixFunc;
            }

            public override void Reset()
            {
                base.Reset();

                GetPostfixFunc = null;
                keys.Clear();
                names.Clear();
                postfixBuilder.Clear();
            }

            public override void Return()
            {
                poolParent.Return(this);
            }

            public static ComboBoxProperty GetProperty(StringBuilder name, IMyTerminalControlCombobox comboBox, List<MyTerminalControlComboBoxItem> comboItems, PropertyBlock block)
            {
                ComboBoxProperty prop = block.comboPropPool.Get();
                prop.SetProperty(name, comboBox, block);
                prop.SetComboItems(comboItems);

                return prop;
            }

            public override void ScrollUp() =>
                ChangePropValue(1);

            public override void ScrollDown() =>
                ChangePropValue(-1);

            private void SetComboItems(List<MyTerminalControlComboBoxItem> comboItems)
            {
                keys.EnsureCapacity(comboItems.Count);
                names.EnsureCapacity(comboItems.Count);

                foreach (MyTerminalControlComboBoxItem item in comboItems)
                {
                    StringBuilder itemName = MyTexts.Get(item.Value);
                    keys.Add(item.Key);
                    names.Add(itemName);
                }
            }

            private void GetChargePostfix(StringBuilder sb)
            {
                sb.Clear();
                sb.Append('(');
                sb.Append(Math.Round((block.Battery.Charge / block.Battery.Capacity) * 100f, 1));
                sb.Append("%)");
            }

            private void ChangePropValue(int delta)
            {
                int index = MathHelper.Clamp((int)(GetValue() + delta), 0, keys.Count - 1);
                SetValue(keys[index]);
            }

            public override long GetValue()
            {
                long key = base.GetValue();

                for (int n = 0; n < keys.Count; n++)
                {
                    if (keys[n] == key)
                        return n;
                }

                return 0;
            }

            public override bool TryParseValue(string valueData, out long value) =>
                long.TryParse(valueData, out value);
        }
    }
}