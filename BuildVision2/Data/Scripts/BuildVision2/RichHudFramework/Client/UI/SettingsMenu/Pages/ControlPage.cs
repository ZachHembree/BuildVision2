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

	namespace UI.Client
	{
		/// <summary>
		/// A page that organizes settings into vertically scrolling <see cref="ControlCategory"/>s in the <see cref="RichHudTerminal"/>.
		/// </summary>
		public class ControlPage : TerminalPageBase, IControlPage
		{
			/// <summary>
			/// Read-only list of control categories registered to this page.
			/// </summary>
			public IReadOnlyList<ControlCategory> Categories { get; }

			/// <summary>
			/// Interface accessor enabling nested collection initializers for adding categories.
			/// </summary>
			public IControlPage<ControlCategory, ControlTile> CategoryContainer => this;

			public ControlPage() : base(ModPages.ControlPage)
			{
				// Retrieve the category list data from the API and wrap it in a read-only collection
				var catData = (MyTuple<object, Func<int>>)GetOrSetMemberFunc(null, (int)ControlPageAccessors.CategoryData);
				var GetCatDataFunc = catData.Item1 as Func<int, ControlContainerMembers>;

				Func<int, ControlCategory> GetCatFunc = (x => new ControlCategory(GetCatDataFunc(x)));
				Categories = new ReadOnlyApiCollection<ControlCategory>(GetCatFunc, catData.Item2);
			}

			IEnumerator<ControlCategory> IEnumerable<ControlCategory>.GetEnumerator() =>
				Categories.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() =>
				Categories.GetEnumerator();

			/// <summary>
			/// Adds the given <see cref="ControlCategory"/> to the page.
			/// </summary>
			public void Add(ControlCategory category) =>
				GetOrSetMemberFunc(category.ID, (int)ControlPageAccessors.AddCategory);
		}
	}
}