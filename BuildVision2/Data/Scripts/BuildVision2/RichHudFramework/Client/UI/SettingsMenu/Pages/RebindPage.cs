using System.Collections;
using System.Collections.Generic;
using VRage;

namespace RichHudFramework
{
	using BindDefinitionData = MyTuple<string, string[], string[][]>;

	namespace UI.Client
	{
		/// <summary>
		/// A terminal page designed specifically for managing key bindings.
		/// <para>Organizes bindings into scrollable <see cref="IBindGroup"/>s.</para>
		/// </summary>
		public class RebindPage : TerminalPageBase, IRebindPage
		{
			/// <summary>
			/// Read-only list of bind groups registered to this page.
			/// </summary>
			public IReadOnlyList<IBindGroup> BindGroups => bindGroups;

			/// <summary>
			/// Interface accessor for adding bind groups via collection-initializer syntax.
			/// </summary>
			public RebindPage GroupContainer => this;

			private readonly List<IBindGroup> bindGroups;

			public RebindPage() : base(ModPages.RebindPage)
			{
				bindGroups = new List<IBindGroup>();
			}

			/// <summary>
			/// Adds an existing <see cref="IBindGroup"/> to the page.
			/// </summary>
			/// <param name="bindGroup">The bind group to add.</param>
			/// <param name="isAliased">If true, exposes the ability to set alias bindings in the UI.</param>
			public void Add(IBindGroup bindGroup, bool isAliased = false)
			{
				GetOrSetMemberFunc(new MyTuple<object, BindDefinitionData[], bool>(bindGroup.ID, null, isAliased), (int)RebindPageAccessors.Add);
				bindGroups.Add(bindGroup);
			}

			/// <summary>
			/// Adds an existing <see cref="IBindGroup"/> to the page and registers default bind definitions.
			/// </summary>
			/// <param name="bindGroup">The bind group to add.</param>
			/// <param name="defaultBinds">Array of default bind definitions used manual for reset.</param>
			/// <param name="isAliased">If true, exposes the ability to set alias bindings in the UI.</param>
			public void Add(IBindGroup bindGroup, BindDefinition[] defaultBinds, bool isAliased = false)
			{
				BindDefinitionData[] data = new BindDefinitionData[defaultBinds.Length];

				for (int n = 0; n < defaultBinds.Length; n++)
					data[n] = (BindDefinitionData)defaultBinds[n];

				GetOrSetMemberFunc(new MyTuple<object, BindDefinitionData[], bool>(bindGroup.ID, data, isAliased), (int)RebindPageAccessors.Add);
				bindGroups.Add(bindGroup);
			}

			public IEnumerator<IBindGroup> GetEnumerator() =>
				bindGroups.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() =>
				bindGroups.GetEnumerator();
		}
	}
}