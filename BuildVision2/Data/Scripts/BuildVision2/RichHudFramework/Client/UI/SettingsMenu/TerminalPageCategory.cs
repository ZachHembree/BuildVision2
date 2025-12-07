using System;
using System.Collections;
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
	using ControlMembers = MyTuple<
		ApiMemberAccessor, // GetOrSetMember
		object // ID
	>;

	namespace UI.Client
	{
		/// <summary>
		/// A collapsable group of RHF terminal pages that can be added to a mod's control root.
		/// <para>Functions as a folder in the sidebar navigation.</para>
		/// </summary>
		public class TerminalPageCategory : TerminalPageCategoryBase
		{
			public TerminalPageCategory() : base(RichHudTerminal.Instance.GetNewPageCategory())
			{ }
		}

		/// <summary>
		/// Abstract base class for terminal page containers/groups.
		/// </summary>
		public abstract class TerminalPageCategoryBase : ITerminalPageCategory
		{
			/// <summary>
			/// The name of the category as it appears in the <see cref="RichHudTerminal"/> navigation list.
			/// </summary>
			public string Name
			{
				get { return GetOrSetMemberFunc(null, (int)TerminalPageCategoryAccessors.Name) as string; }
				set { GetOrSetMemberFunc(value, (int)TerminalPageCategoryAccessors.Name); }
			}

			/// <summary>
			/// Read-only collection of <see cref="TerminalPageBase"/>s assigned to this category.
			/// </summary>
			public IReadOnlyList<TerminalPageBase> Pages { get; }

			/// <summary>
			/// Interface accessor for collection initializers.
			/// </summary>
			public ITerminalPageCategory PageContainer => this;

			/// <summary>
			/// Unique identifier used by the Framework API.
			/// </summary>
			public object ID => data.Item3;

			/// <summary>
			/// The currently selected <see cref="TerminalPageBase"/> within this category. Returns null if none selected.
			/// </summary>
			public TerminalPageBase SelectedPage
			{
				get
				{
					object id = GetOrSetMemberFunc(null, (int)TerminalPageCategoryAccessors.Selection);

					if (id != null)
					{
						for (int n = 0; n < Pages.Count; n++)
						{
							if (id == Pages[n].ID)
								return Pages[n];
						}
					}

					return null;
				}
			}

			/// <summary>
			/// Determines whether or not the category will appear in the list.
			/// </summary>
			public bool Enabled
			{
				get { return (bool)GetOrSetMemberFunc(null, (int)TerminalPageCategoryAccessors.Enabled); }
				set { GetOrSetMemberFunc(value, (int)TerminalPageCategoryAccessors.Enabled); }
			}

			/// <summary>
			/// Internal page member accessor delegate.
			/// </summary>
			/// <exclude/>
			protected ApiMemberAccessor GetOrSetMemberFunc => data.Item1;

			/// <summary>
			/// Internal API data tuple.
			/// </summary>
			/// <exclude/>
			protected readonly ControlContainerMembers data;

			/// <summary>
			/// Initializes a new control page interface from an API accessor tuple.
			/// </summary>
			/// <exclude/>
			public TerminalPageCategoryBase(ControlContainerMembers data)
			{
				this.data = data;

				var GetPageDataFunc = data.Item2.Item1 as Func<int, ControlMembers>;
				Func<int, TerminalPageBase> GetPageFunc = (x => new TerminalPage(GetPageDataFunc(x)));
				Pages = new ReadOnlyApiCollection<TerminalPageBase>(GetPageFunc, data.Item2.Item2);
			}

			/// <summary>
			/// Adds the given <see cref="TerminalPageBase"/> to the category.
			/// </summary>
			public void Add(TerminalPageBase page) =>
				GetOrSetMemberFunc(page.ID, (int)TerminalPageCategoryAccessors.AddPage);

			/// <summary>
			/// Adds a collection of pages to the category.
			/// </summary>
			public void AddRange(IReadOnlyList<TerminalPageBase> pages)
			{
				foreach (TerminalPageBase page in pages)
					GetOrSetMemberFunc(page.ID, (int)TerminalPageCategoryAccessors.AddPage);
			}

			/// <summary>
			/// Retrieves data used by the Framework API.
			/// </summary>
			/// <exclude/>
			public ControlContainerMembers GetApiData() =>
				data;

			public IEnumerator<TerminalPageBase> GetEnumerator() =>
				Pages.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() =>
				Pages.GetEnumerator();

			/// <summary>
			/// Internal generic page wrapper
			/// </summary>
			/// <exclude/>
			protected class TerminalPage : TerminalPageBase
			{
				public TerminalPage(ControlMembers data) : base(data)
				{ }
			}
		}
	}
}