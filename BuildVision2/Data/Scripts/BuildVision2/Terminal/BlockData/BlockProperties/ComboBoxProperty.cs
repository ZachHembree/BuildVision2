﻿using Sandbox.ModAPI.Interfaces.Terminal;
using System;
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
            public override string Value => names[GetCurrentIndex()];
            public override string Postfix => GetPostfixFunc?.Invoke();

            private readonly List<long> keys;
            private readonly List<string> names;
            private readonly Func<string> GetPostfixFunc;

            public ComboBoxProperty(string name, IMyTerminalControlCombobox comboBox, IMyTerminalControl control, SuperBlock block) : base(name, comboBox, control, block)
            {
                List<MyTerminalControlComboBoxItem> content = new List<MyTerminalControlComboBoxItem>();
                comboBox.ComboBoxContent(content);

                keys = new List<long>(content.Count);
                names = new List<string>(content.Count);

                foreach (MyTerminalControlComboBoxItem item in content)
                {
                    string itemName = MyTexts.Get(item.Value).ToString();
                    keys.Add(item.Key);
                    names.Add(itemName);
                }

                if (control.Id == "ChargeMode" && block.SubtypeId.HasFlag(TBlockSubtypes.Battery)) // Insert bat charge info
                    GetPostfixFunc = () => $"({Math.Round((block.Battery.PowerStored / block.Battery.Capacity) * 100f, 1)}%)";
            }

            public override void ScrollUp() =>
                ChangePropValue(1);

            public override void ScrollDown() =>
                ChangePropValue(-1);

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