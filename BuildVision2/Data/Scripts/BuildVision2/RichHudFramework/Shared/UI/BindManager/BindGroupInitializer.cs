using System;
using System.Collections.Generic;
using VRage;
using VRage.Input;

namespace RichHudFramework
{
    using KeyComboInitData = IReadOnlyList<int>;

    namespace UI
    {
        using Server;
        using Client;
        using System.Collections;
        using BindInitData = MyTuple<string, KeyComboInitData, IReadOnlyList<KeyComboInitData>>;

        /// <summary>
        /// Bind data container used to simplify bind registration. Used with <see cref="KeyComboInit"/>, and supports
        /// control names, <see cref="MyKeys"/>, <see cref="RichHudControls"/> and <see cref="MyJoystickButtonsEnum"/>
        /// </summary>
        public class BindGroupInitializer : IReadOnlyList<BindInitData>
        {
            public BindInitData this[int index] => bindData[index];

            public int Count => bindData.Count;

            private readonly List<BindInitData> bindData;

            public BindGroupInitializer()
            {
                bindData = new List<BindInitData>();
            }

            public IEnumerator<BindInitData> GetEnumerator() =>
                bindData.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                bindData.GetEnumerator();

            /// <summary>
            /// Adds a bind with the given name and the given key combo
            /// </summary>
            public void Add(string bindName, ControlHandle? con1 = null, ControlHandle? con2 = null, ControlHandle? con3 = null)
            {
                var combo = new KeyComboInit();

                if (con1 != null)
                    combo.Add(con1.Value);

                if (con2 != null)
                    combo.Add(con2.Value);

                if (con3 != null)
                    combo.Add(con3.Value);

                bindData.Add(new BindInitData(bindName, combo, null));
            }

            public void Add(string bindName, ControlHandle? con1, KeyComboInit alias)
            {
                var combo = new KeyComboInit();

                if (con1 != null)
                    combo.Add(con1.Value);

                bindData.Add(new BindInitData(bindName, combo, new List<KeyComboInitData> { alias }));
            }

            public void Add(string bindName, ControlHandle? con1, ControlHandle? con2, KeyComboInit alias)
            {
                var combo = new KeyComboInit();

                if (con1 != null)
                    combo.Add(con1.Value);

                if (con2 != null)
                    combo.Add(con2.Value);

                bindData.Add(new BindInitData(bindName, combo, new List<KeyComboInitData> { alias }));
            }

            public void Add(string bindName, ControlHandle? con1, ControlHandle? con2, ControlHandle? con3, KeyComboInit alias)
            {
                var combo = new KeyComboInit();

                if (con1 != null)
                    combo.Add(con1.Value);

                if (con2 != null)
                    combo.Add(con2.Value);

                if (con3 != null)
                    combo.Add(con3.Value);

                bindData.Add(new BindInitData(bindName, combo, new List<KeyComboInitData> { alias }));
            }

            public void Add(string bindName, KeyComboInit combo, KeyComboInit alias)
            {
                bindData.Add(new BindInitData(bindName, combo, new List<KeyComboInitData> { alias }));
            }

            public void Add(string bindName, KeyComboInit combo, KeyComboInit alias1, KeyComboInit alias2)
            {
                bindData.Add(new BindInitData(bindName, combo, new List<KeyComboInitData> { alias1, alias2 }));
            }

            public BindDefinition[] GetBindDefinitions()
            {
                var bindDefs = new BindDefinition[bindData.Count];

                for (int i = 0; i < bindData.Count; i++)
                {
                    var bindName = bindData[i].Item1;
                    var mainCombo = bindData[i].Item2;
                    var aliases = bindData[i].Item3;

                    bindDefs[i].name = bindName;
                    bindDefs[i].controlNames = BindManager.GetControlNames(mainCombo);
                    bindDefs[i].aliases = new BindAliasDefinition[aliases.Count];

                    for (int j = 0; j < aliases.Count; j++)
                    {
                        bindDefs[i].aliases[j].controlNames = BindManager.GetControlNames(aliases[j]);
                    }
                }

                return bindDefs;
            }

            public static implicit operator List<BindInitData>(BindGroupInitializer gInit)
            {
                return gInit.bindData;
            }
        }

        /// <summary>
        /// Container for bind aliases. Used with <see cref="BindGroupInitializer"/>, and supports
        /// control names, <see cref="MyKeys"/>, <see cref="RichHudControls"/> and <see cref="MyJoystickButtonsEnum"/>
        /// </summary>
        public class KeyComboInit : IReadOnlyList<int>
        {
            public int this[int index] => comboData[index];

            public int Count => comboData.Count;

            private readonly List<int> comboData;

            public KeyComboInit()
            {
                comboData = new List<int>(3);
            }

            public KeyComboInit(List<int> comboData)
            {
                this.comboData = comboData;
            }

            public KeyComboInit(ControlHandle con)
            {
                comboData = new List<int> { con.id };
            }

            public KeyComboInit(ControlHandle con1, ControlHandle con2)
            {
                comboData = new List<int> { con1.id, con2.id };
            }

            public KeyComboInit(ControlHandle con1, ControlHandle con2, ControlHandle con3)
            {
                comboData = new List<int> { con1.id, con2.id, con3.id };
            }

            public IEnumerator<int> GetEnumerator() =>
                comboData.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() =>
                comboData.GetEnumerator();

            public void Add(ControlHandle con)
            {
                if (comboData.Count < BindManager.MaxBindLength)
                    comboData.Add(con.id);
                else
                    throw new Exception("Attempted to add more than 3 controls to a key combo.");
            }

            public static implicit operator KeyComboInit(List<int> comboData)
            {
                return new KeyComboInit(comboData);
            }

            public static implicit operator List<int>(KeyComboInit cInit)
            {
                return cInit.comboData;
            }
        }
    }
}