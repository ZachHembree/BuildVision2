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
		/// A collection designed to simplify the definition and registration of groups of key binds.
		/// <para>Supports mixed control types (Strings, MyKeys, RichHudControls, JoystickButtons) via <see cref="ControlHandle"/>.</para>
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
			/// Adds a new bind to the group with a primary key combination.
			/// </summary>
			/// <param name="bindName">Unique name for the bind.</param>
			/// <param name="con1">First control in the primary combo.</param>
			/// <param name="con2">Optional second control.</param>
			/// <param name="con3">Optional third control.</param>
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

			/// <summary>
			/// Adds a new bind with a primary control and one alias combination.
			/// </summary>
			public void Add(string bindName, ControlHandle? con1, KeyComboInit alias)
			{
				var combo = new KeyComboInit();

				if (con1 != null)
					combo.Add(con1.Value);

				bindData.Add(new BindInitData(bindName, combo, new List<KeyComboInitData> { alias }));
			}

			/// <summary>
			/// Adds a new bind with a two-key primary combo and one alias combination.
			/// </summary>
			public void Add(string bindName, ControlHandle? con1, ControlHandle? con2, KeyComboInit alias)
			{
				var combo = new KeyComboInit();

				if (con1 != null)
					combo.Add(con1.Value);

				if (con2 != null)
					combo.Add(con2.Value);

				bindData.Add(new BindInitData(bindName, combo, new List<KeyComboInitData> { alias }));
			}

			/// <summary>
			/// Adds a new bind with a three-key primary combo and one alias combination.
			/// </summary>
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

			/// <summary>
			/// Adds a new bind using pre-configured KeyComboInit objects for both primary and alias.
			/// </summary>
			public void Add(string bindName, KeyComboInit combo, KeyComboInit alias)
			{
				bindData.Add(new BindInitData(bindName, combo, new List<KeyComboInitData> { alias }));
			}

			/// <summary>
			/// Adds a new bind using pre-configured KeyComboInit objects for primary and two aliases.
			/// </summary>
			public void Add(string bindName, KeyComboInit combo, KeyComboInit alias1, KeyComboInit alias2)
			{
				bindData.Add(new BindInitData(bindName, combo, new List<KeyComboInitData> { alias1, alias2 }));
			}

			/// <summary>
			/// Converts the initialization data into an array of <see cref="BindDefinition"/> structs.
			/// </summary>
			public BindDefinition[] GetBindDefinitions()
			{
				var bindDefs = new BindDefinition[bindData.Count];

				for (int i = 0; i < bindData.Count; i++)
				{
					var bindName = bindData[i].Item1;
					var mainCombo = bindData[i].Item2;
					var aliases = bindData[i].Item3;

					bindDefs[i].name = bindName;

					if (mainCombo != null)
						bindDefs[i].controlNames = BindManager.GetControlNames(mainCombo);

					if (aliases != null)
					{
						bindDefs[i].aliases = new BindAliasDefinition[aliases.Count];

						for (int j = 0; j < aliases.Count; j++)
							bindDefs[i].aliases[j].controlNames = BindManager.GetControlNames(aliases[j]);
					}
				}

				return bindDefs;
			}

			/// <exclude/>
			public static implicit operator List<BindInitData>(BindGroupInitializer gInit)
			{
				return gInit.bindData;
			}
		}

		/// <summary>
		/// Helper class for defining a specific combination of controls (up to 3).
		/// <para>Used with <see cref="BindGroupInitializer"/> to specify bind combos or aliases.</para>
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

			/// <summary>
			/// Adds a control to the combination. Throws an exception if more than 3 controls are added.
			/// </summary>
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