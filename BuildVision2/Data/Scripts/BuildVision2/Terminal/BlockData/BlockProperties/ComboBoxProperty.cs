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
        private class ComboBoxProperty : BvTerminalValueControl<IMyTerminalControlCombobox, long>, IBlockComboBox
        {
            /// <summary>
            /// Localized selection options for combo box
            /// </summary>
            public IReadOnlyList<KeyValuePair<long, StringBuilder>> ComboEntries => comboEntries;

            /// <summary>
            /// Retrieves the value as a <see cref="StringBuilder"/> using formatting specific to the member.
            /// </summary>
            public override StringBuilder FormattedValue 
            {
                get 
                {
                    int i = (int)GetValue();

                    if (comboEntries.Count > 0 && i >= 0 && i < comboEntries.Count)
                        return comboEntries[i].Value;
                    else
                        return null;
                }
            }

            /// <summary>
            /// Additional information following the value of the member.
            /// </summary>
            public override StringBuilder StatusText 
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

            private readonly List<KeyValuePair<long, StringBuilder>> comboEntries;

            private Action<StringBuilder> GetPostfixFunc, GetChargePostfixFunc;
            private BvPropPool<ComboBoxProperty> poolParent;
            private readonly StringBuilder postfixBuilder;

            public ComboBoxProperty()
            {
                comboEntries = new List<KeyValuePair<long, StringBuilder>>();

                GetChargePostfixFunc = GetChargePostfix;
                postfixBuilder = new StringBuilder();
                ValueType = BlockMemberValueTypes.Combo;
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
                comboEntries.Clear();
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

            private void SetComboItems(List<MyTerminalControlComboBoxItem> comboItems)
            {
                comboEntries.EnsureCapacity(comboItems.Count);

                foreach (MyTerminalControlComboBoxItem item in comboItems)
                {
                    StringBuilder itemName = MyTexts.Get(item.Value);
                    comboEntries.Add(new KeyValuePair<long, StringBuilder>(item.Key, itemName));
                }
            }

            private void GetChargePostfix(StringBuilder sb)
            {
                sb.Clear();
                sb.Append('(');
                sb.Append(Math.Round((block.Battery.Charge / block.Battery.Capacity) * 100f, 1));
                sb.Append("%)");
            }

            protected override long GetValue()
            {
                long key = base.GetValue();

                for (int n = 0; n < comboEntries.Count; n++)
                {
                    if (comboEntries[n].Key == key)
                        return n;
                }

                return 0;
            }

            protected override void SetValue(long value)
            {
                int index = MathHelper.Clamp((int)value, 0, comboEntries.Count - 1);
                base.SetValue(comboEntries[index].Key);
            }

            public override bool TryParseValue(string valueData, out long value) =>
                long.TryParse(valueData, out value);
        }
    }
}