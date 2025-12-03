using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
	using ControlMembers = MyTuple<
		ApiMemberAccessor, // GetOrSetMember
		object // ID
	>;

	namespace UI.Client
	{
		/// <summary>
		/// Abstract base for interfacing with terminal page implementations
		/// </summary>
		public abstract class TerminalPageBase : ITerminalPage
		{
			/// <summary>
			/// Name of the <see cref="ITerminalPage"/> as it appears in the dropdown of the <see cref="IModControlRoot"/>.
			/// </summary>
			public string Name
			{
				get { return GetOrSetMemberFunc(null, (int)TerminalPageAccessors.Name) as string; }
				set { GetOrSetMemberFunc(value, (int)TerminalPageAccessors.Name); }
			}

			/// <summary>
			/// Unique identifier
			/// </summary>
			/// <exclude/>
			public object ID => data.Item2;

			/// <summary>
			/// Determines whether or not the page will be visible in the mod root.
			/// </summary>
			public bool Enabled
			{
				get { return (bool)GetOrSetMemberFunc(null, (int)TerminalPageAccessors.Enabled); }
				set { GetOrSetMemberFunc(value, (int)TerminalPageAccessors.Enabled); }
			}

			/// <summary>
			/// Internal API accessor delegate
			/// </summary>
			/// <exclude/>
			protected ApiMemberAccessor GetOrSetMemberFunc => data.Item1;

			/// <summary>
			/// Internal API accessor tuple
			/// </summary>
			/// <exclude/>
			protected readonly ControlMembers data;

			/// <summary>
			/// Constructs a new terminal page corresponding to the given enum
			/// </summary>
			/// <exclude/>
			public TerminalPageBase(ModPages pageEnum)
			{
				data = RichHudTerminal.Instance.GetNewMenuPage(pageEnum);
			}

			/// <summary>
			/// Constructs a new terminal page from API tuple
			/// </summary>
			/// <exclude/>
			public TerminalPageBase(ControlMembers data)
			{
				this.data = data;
			}

			/// <summary>
			/// Retrieves information used by the Framework API
			/// </summary>
			/// <exclude/>
			public ControlMembers GetApiData() =>
				data;
		}
	}
}