using System.Xml.Serialization;
using VRage;
using BindDefinitionDataOld = VRage.MyTuple<string, string[]>;

namespace RichHudFramework
{
	using BindDefinitionData = MyTuple<string, string[], string[][]>;

	namespace UI
	{
		/// <summary>
		/// Wrapper for serializing bind aliases (alternate key combinations) to XML.
		/// </summary>
		[XmlType(TypeName = "Alias")]
		public struct BindAliasDefinition
		{
			/// <summary>
			/// The names of the controls in this alias combination.
			/// </summary>
			[XmlArray("Controls")]
			public string[] controlNames;

			public BindAliasDefinition(string[] controlNames)
			{
				this.controlNames = controlNames;
			}

			public static implicit operator string[](BindAliasDefinition aliasDef)
			{
				return aliasDef.controlNames;
			}

			public static implicit operator BindAliasDefinition(string[] alias)
			{
				return new BindAliasDefinition(alias);
			}
		}

		/// <summary>
		/// Serializable container for individual key binds.
		/// <para>Includes the bind name, the primary control combination, and any optional aliases.</para>
		/// </summary>
		[XmlType(TypeName = "Bind")]
		public struct BindDefinition
		{
			/// <summary>
			/// The unique identifier/name of the bind.
			/// </summary>
			[XmlAttribute]
			public string name;

			/// <summary>
			/// The list of control names for the primary key combination.
			/// </summary>
			[XmlArray("Controls")]
			public string[] controlNames;

			/// <summary>
			/// Optional list of alternative key combinations (aliases) for this bind.
			/// </summary>
			[XmlArray("Aliases")]
			public BindAliasDefinition[] aliases;

			public BindDefinition(string name, string[] controlNames, BindAliasDefinition[] aliases = null)
			{
				this.name = name;
				this.controlNames = controlNames;
				this.aliases = aliases;
			}

			/// <summary>
			/// Converts legacy tuple data into a BindDefinition.
			/// </summary>
			public static implicit operator BindDefinition(BindDefinitionDataOld value)
			{
				return new BindDefinition(value.Item1, value.Item2);
			}

			/// <summary>
			/// Explicitly converts internal Tuple-based bind data to the serializable BindDefinition struct.
			/// </summary>
			public static explicit operator BindDefinition(BindDefinitionData value)
			{
				BindAliasDefinition[] aliases = null;
				string[][] aliasData = value.Item3;

				if (aliasData != null)
				{
					aliases = new BindAliasDefinition[aliasData.Length];

					for (int i = 0; i < aliasData.Length; i++)
						aliases[i] = new BindAliasDefinition(aliasData[i]);
				}

				return new BindDefinition(value.Item1, value.Item2, aliases);
			}

			/// <summary>
			/// Explicitly converts a BindDefinition back to the internal Tuple-based format.
			/// </summary>
			public static explicit operator BindDefinitionData(BindDefinition value)
			{
				string[][] aliasData = null;

				if (value.aliases != null)
				{
					aliasData = new string[value.aliases.Length][];

					for (int i = 0; i < aliasData.Length; i++)
						aliasData[i] = value.aliases[i].controlNames;
				}

				return new BindDefinitionData(value.name, value.controlNames, aliasData);
			}
		}
	}
}