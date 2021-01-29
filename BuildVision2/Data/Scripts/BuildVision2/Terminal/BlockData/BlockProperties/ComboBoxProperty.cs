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
            public override string Display => names[GetCurrentIndex()];

            public override string Status => GetPostfixFunc?.Invoke();

            private readonly List<long> keys;
            private readonly List<string> names;
            private Func<string> GetPostfixFunc, GetChargePostfixFunc;
            protected BvPropPool<ComboBoxProperty> poolParent;

            public ComboBoxProperty()
            {
                keys = new List<long>();
                names = new List<string>();
                GetChargePostfixFunc = GetChargePostfix;
            }

            public override void SetProperty(string name, IMyTerminalControlCombobox comboBox, IMyTerminalControl control, PropertyBlock block)
            {
                base.SetProperty(name, comboBox, control, block);

                if (poolParent == null)
                    poolParent = block.comboPropPool;

                List<MyTerminalControlComboBoxItem> content = new List<MyTerminalControlComboBoxItem>();
                comboBox.ComboBoxContent(content);

                keys.EnsureCapacity(content.Count);
                names.EnsureCapacity(content.Count);

                foreach (MyTerminalControlComboBoxItem item in content)
                {
                    string itemName = MyTexts.Get(item.Value).ToString();
                    keys.Add(item.Key);
                    names.Add(itemName);
                }
                
                if (control.Id == "ChargeMode" && block.SubtypeId.UsesSubtype(TBlockSubtypes.Battery)) // Insert bat charge info
                    GetPostfixFunc = GetChargePostfixFunc;
            }

            public override void Reset()
            {
                base.Reset();

                GetPostfixFunc = null;
                keys.Clear();
                names.Clear();
            }

            public override void Return()
            {
                poolParent.Return(this);
            }

            public static ComboBoxProperty GetProperty(string name, IMyTerminalControlCombobox comboBox, IMyTerminalControl control, PropertyBlock block)
            {
                ComboBoxProperty prop = block.comboPropPool.Get();
                prop.SetProperty(name, comboBox, control, block);

                return prop;
            }

            public override void ScrollUp() =>
                ChangePropValue(1);

            public override void ScrollDown() =>
                ChangePropValue(-1);

            private string GetChargePostfix()
            {
                return $"({Math.Round((block.Battery.Charge / block.Battery.Capacity) * 100f, 1)}%)";
            }

            private void ChangePropValue(int delta)
            {
                int index = MathHelper.Clamp((GetCurrentIndex() + delta), 0, keys.Count - 1);
                SetValue(keys[index]);
            }

            private int GetCurrentIndex()
            {
                long key = GetValue();

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