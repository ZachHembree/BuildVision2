using System;
using VRage;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
	using Client;
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
		using SettingsMenuMembers = MyTuple<
			ApiMemberAccessor, // GetOrSetMembers
			ControlContainerMembers, // MenuRoot
			Func<int, ControlMembers>, // GetNewControl
			Func<int, ControlContainerMembers>, // GetNewContainer
			Func<int, ControlMembers> // GetNewModPage
		>;

		/// <summary>
		/// The central windowed settings menu shared by all mods using the framework.
		/// </summary>
		public sealed partial class RichHudTerminal : RichHudClient.ApiModule
		{
			/// <summary>
			/// The root container for this specific client/mod. Add your pages and categories here.
			/// </summary>
			public static IModControlRoot Root => Instance.menuRoot;

			/// <summary>
			/// Indicates whether the terminal window is currently visible.
			/// </summary>
			public static bool Open => (bool)Instance.GetOrSetMembersFunc(null, (int)TerminalAccessors.GetMenuOpen);

			/// <summary>
			/// The internal singleton instance of the Terminal.
			/// <para>Initialization is handled automatically.</para>
			/// </summary>
			/// <exclude/>
			public static RichHudTerminal Instance
			{
				get { Init(); return _instance; }
				set { _instance = value; }
			}
			private static RichHudTerminal _instance;

			private readonly ModControlRoot menuRoot;
			private readonly ApiMemberAccessor GetOrSetMembersFunc;
			private readonly Func<int, ControlMembers> GetNewControlFunc;
			private readonly Func<int, ControlContainerMembers> GetNewContainerFunc;
			private readonly Func<int, ControlMembers> GetNewPageFunc;
			private readonly Func<ControlContainerMembers> GetNewPageCategoryFunc;

			private RichHudTerminal() : base(ApiModuleTypes.SettingsMenu, false, true)
			{
				var data = (SettingsMenuMembers)GetApiData();

				GetOrSetMembersFunc = data.Item1;
				GetNewControlFunc = data.Item3;
				GetNewContainerFunc = data.Item4;
				GetNewPageFunc = data.Item5;

				GetNewPageCategoryFunc =
					GetOrSetMembersFunc(null, (int)TerminalAccessors.GetNewPageCategoryFunc) as Func<ControlContainerMembers>;

				menuRoot = new ModControlRoot(data.Item2);
			}

			/// <summary>
			/// Initializes the RHF terminal singleton. 
			/// </summary>
			/// <exclude/>
			private static void Init()
			{
				if (_instance == null)
				{
					_instance = new RichHudTerminal();
				}
			}

			/// <summary>
			/// Toggles the visibility of the terminal window.
			/// </summary>
			public static void ToggleMenu()
			{
				if (_instance == null)
					Init();

				_instance.GetOrSetMembersFunc(null, (int)TerminalAccessors.ToggleMenu);
			}

			/// <summary>
			/// Opens the terminal window.
			/// </summary>
			public static void OpenMenu()
			{
				if (_instance == null)
					Init();

				_instance.GetOrSetMembersFunc(null, (int)TerminalAccessors.OpenMenu);
			}

			/// <summary>
			/// Closes the terminal window.
			/// </summary>
			public static void CloseMenu()
			{
				if (_instance == null)
					Init();

				_instance.GetOrSetMembersFunc(null, (int)TerminalAccessors.CloseMenu);
			}

			/// <summary>
			/// Opens the terminal window (if closed) and navigates directly to the specified page.
			/// </summary>
			public static void OpenToPage(TerminalPageBase newPage)
			{
				_instance.GetOrSetMembersFunc(new MyTuple<object, object>(_instance.menuRoot.ID, newPage.ID), (int)TerminalAccessors.OpenToPage);
			}

			/// <summary>
			/// Sets the active page in the terminal without forcing the window to open.
			/// </summary>
			public static void SetPage(TerminalPageBase newPage)
			{
				_instance.GetOrSetMembersFunc(new MyTuple<object, object>(_instance.menuRoot.ID, newPage.ID), (int)TerminalAccessors.SetPage);
			}

			/// <summary>
			/// Clears the singleton instance.
			/// </summary>
			/// <exclude/>
			public override void Close()
			{
				_instance = null;
			}

			/// <summary>
			/// Internal method for creating and returning API accessors to a new control
			/// </summary>
			/// <exclude/>
			public ControlMembers GetNewMenuControl(MenuControls controlEnum) =>
				Instance.GetNewControlFunc((int)controlEnum);

			/// <summary>
			/// Internal method for creating and returning API accessors for a new control tile
			/// </summary>
			/// <exclude/>
			public ControlContainerMembers GetNewMenuTile() =>
				Instance.GetNewContainerFunc((int)ControlContainers.Tile);

			/// <summary>
			/// Internal method for creating and returning API accessors to a new control category
			/// </summary>
			/// <exclude/>
			public ControlContainerMembers GetNewMenuCategory() =>
				Instance.GetNewContainerFunc((int)ControlContainers.Category);

			/// <summary>
			/// Internal method for creating and returning API accessors to a new page of a given type
			/// </summary>
			/// <exclude/>
			public ControlMembers GetNewMenuPage(ModPages pageEnum) =>
				Instance.GetNewPageFunc((int)pageEnum);

			/// <summary>
			/// Internal method for creating and returning API accessors to a new page category
			/// </summary>
			/// <exclude/>
			public ControlContainerMembers GetNewPageCategory() =>
				Instance.GetNewPageCategoryFunc();
		}
	}
}