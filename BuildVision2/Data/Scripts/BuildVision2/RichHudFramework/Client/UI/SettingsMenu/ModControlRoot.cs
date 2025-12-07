using System;
using System.Collections.Generic;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
	using ControlContainerMembers = MyTuple<
		ApiMemberAccessor, // GetOrSetMember,
		MyTuple<object, Func<int>>, // Member List
		object // ID
	>;

	namespace UI.Client
	{
		public sealed partial class RichHudTerminal
		{
			/// <summary>
			/// Represents the root category for the client's settings. 
			/// <para>This is the entry point where pages and subcategories are added.</para>
			/// </summary>
			private class ModControlRoot : TerminalPageCategoryBase, IModControlRoot
			{
				public IReadOnlyList<TerminalPageCategoryBase> Subcategories { get; }

				/// <summary>
				/// Invoked when the selected page within this root changes.
				/// </summary>
				public event EventHandler SelectionChanged;

				public ModControlRoot(ControlContainerMembers data) : base(data)
				{
					GetOrSetMemberFunc(new Action(ModRootCallback), (int)ModControlRootAccessors.GetOrSetCallback);

					var GetCategoryDataFunc = GetOrSetMemberFunc(null, (int)ModControlRootAccessors.GetCategoryAccessors)
						as Func<int, ControlContainerMembers>;

					Func<int, TerminalPageCategoryBase> GetPageFunc = (x => new TerminalPageCategoryWrapper(GetCategoryDataFunc(x)));
					Subcategories = new ReadOnlyApiCollection<TerminalPageCategoryBase>(GetPageFunc, data.Item2.Item2);
				}

				protected void ModRootCallback()
				{
					SelectionChanged?.Invoke(this, EventArgs.Empty);
				}

				public void Add(TerminalPageCategoryBase subcategory) =>
					GetOrSetMemberFunc(subcategory.ID, (int)ModControlRootAccessors.AddSubcategory);

				public void AddRange(IReadOnlyList<IModRootMember> members)
				{
					foreach (IModRootMember member in members)
					{
						if (member is TerminalPageBase)
							GetOrSetMemberFunc(member.ID, (int)TerminalPageCategoryAccessors.AddPage);
						else
							GetOrSetMemberFunc(member.ID, (int)ModControlRootAccessors.AddSubcategory);
					}
				}

				private class TerminalPageCategoryWrapper : TerminalPageCategoryBase
				{
					public TerminalPageCategoryWrapper(ControlContainerMembers data) : base(data)
					{ }
				}
			}
		}
	}
}