using System;
using System.Collections.Generic;
using VRage;
using VRageMath;
using ApiMemberAccessor = System.Func<object, int, object>;

namespace RichHudFramework
{
	using Client;

	namespace UI.Client
	{
		using BindClientMembers = MyTuple<
			ApiMemberAccessor, // GetOrSetMember
			MyTuple<Func<int, object, int, object>, Func<int>>, // GetOrSetGroupMember, GetGroupCount
			MyTuple<Func<Vector2I, object, int, object>, Func<int, int>>, // GetOrSetBindMember, GetBindCount
			Func<Vector2I, int, bool>, // IsBindPressed
			MyTuple<Func<int, int, object>, Func<int>>, // GetControlMember, GetControlCount
			Action // Unload
		>;

		/// <summary>
		/// The central hub for creating and retrieving key binds, control groups, and 
		/// configuring input blacklisting in the framework.
		/// </summary>
		public sealed partial class BindManager : RichHudClient.ApiModule
		{
			/// <summary>
			/// The maximum number of controls allowed in a single key bind combination (currently 3).
			/// </summary>
			public const int MaxBindLength = 3;

			/// <summary>
			/// A read-only collection of all registered bind groups.
			/// </summary>
			public static IReadOnlyList<IBindGroup> Groups => Instance.groups;

			/// <summary>
			/// A read-only collection of all available controls (keys, mouse buttons, gamepad inputs) supported by the framework.
			/// </summary>
			public static IReadOnlyList<IControl> Controls => Instance.controls;

			/// <summary>
			/// Gets or sets the persistent input blacklist mode. 
			/// <para>This setting remains active until explicitly changed. Use this to block game inputs 
			/// (like mouse clicks or camera movement) while UI elements are interactable.</para>
			/// </summary>
			public static SeBlacklistModes BlacklistMode
			{
				get
				{
					if (_instance == null) Init();
					return (SeBlacklistModes)_instance.GetOrSetMemberFunc(null, (int)BindClientAccessors.RequestBlacklistMode);
				}
				set
				{
					if (_instance == null)
						Init();

					lastBlacklist = value;
					_instance.GetOrSetMemberFunc(value, (int)BindClientAccessors.RequestBlacklistMode);
				}
			}

			/// <summary>
			/// Checks if the chat window is currently open. 
			/// <para>Unlike the standard game API, this updates instantly when the chat bind is pressed.</para>
			/// </summary>
			public static bool IsChatOpen => (bool)_instance.GetOrSetMemberFunc(null, (int)BindClientAccessors.IsChatOpen);

			private static BindManager Instance
			{
				get { Init(); return _instance; }
			}
			private static BindManager _instance;

			// Group list
			private readonly Func<int, object, int, object> GetOrSetGroupMemberFunc;
			private readonly Func<int> GetGroupCountFunc;

			// Bind lists
			private readonly Func<Vector2I, object, int, object> GetOrSetBindMemberFunc;
			private readonly Func<Vector2I, int, bool> IsBindPressedFunc;
			private readonly Func<int, int> GetBindCountFunc;

			// Control list
			private readonly Func<int, int, object> GetControlMember;
			private readonly Func<int> GetControlCountFunc;

			private readonly ApiMemberAccessor GetOrSetMemberFunc;
			private readonly Action UnloadAction;

			private readonly ReadOnlyApiCollection<IBindGroup> groups;
			private readonly ReadOnlyApiCollection<IControl> controls;
			private readonly List<int> conIDbuf;
			private readonly List<List<int>> aliasIDbuf;

			private static SeBlacklistModes lastBlacklist, tmpBlacklist;

			private BindManager() : base(ApiModuleTypes.BindManager, false, true)
			{
				var clientData = (BindClientMembers)GetApiData();

				GetOrSetMemberFunc = clientData.Item1;
				UnloadAction = clientData.Item6;

				// Group list
				GetOrSetGroupMemberFunc = clientData.Item2.Item1;
				GetGroupCountFunc = clientData.Item2.Item2;

				// Bind lists
				IsBindPressedFunc = clientData.Item4;
				GetOrSetBindMemberFunc = clientData.Item3.Item1;
				GetBindCountFunc = clientData.Item3.Item2;

				// Control list
				GetControlMember = clientData.Item5.Item1;
				GetControlCountFunc = clientData.Item5.Item2;

				groups = new ReadOnlyApiCollection<IBindGroup>(x => new BindGroup(x), GetGroupCountFunc);
				controls = new ReadOnlyApiCollection<IControl>(x => new Control(x), GetControlCountFunc);

				conIDbuf = new List<int>();
				aliasIDbuf = new List<List<int>>();
			}

			/// <summary>
			/// Initializes the BindManager
			/// </summary>
			/// <exclude/>
			private static void Init()
			{
				if (_instance == null)
				{
					_instance = new BindManager();
				}
			}

			/// <summary>
			/// Unloads the bind manager and releases the instance.
			/// </summary>
			/// <exclude/>
			public override void Close()
			{
				UnloadAction?.Invoke();
				_instance = null;
			}

			/// <summary>
			/// Applies a temporary blacklist flag that lasts only for the current frame.
			/// <para>This is additive to the persistent <see cref="BlacklistMode"/>.</para>
			/// </summary>
			public static void RequestTempBlacklist(SeBlacklistModes mode)
			{
				tmpBlacklist |= mode;
			}

			/// <summary>
			/// Internal update cycle used to apply blacklist modes and reset temporary flags.
			/// </summary>
			/// <exclude/>
			public override void Draw()
			{
				GetOrSetMemberFunc(lastBlacklist | tmpBlacklist, (int)BindClientAccessors.RequestBlacklistMode);
				tmpBlacklist = SeBlacklistModes.None;
			}

			/// <summary>
			/// Retrieves an existing bind group by name, or creates a new one if it does not exist.
			/// </summary>
			/// <param name="name">The unique name of the group.</param>
			public static IBindGroup GetOrCreateGroup(string name)
			{
				var index = (int)Instance.GetOrSetMemberFunc(name, (int)BindClientAccessors.GetOrCreateGroup);
				return index != -1 ? Groups[index] : null;
			}

			/// <summary>
			/// Retrieves an existing bind group by name. Returns null if the group is not found.
			/// </summary>
			/// <param name="name">The name of the group to retrieve.</param>
			public static IBindGroup GetBindGroup(string name)
			{
				var index = (int)Instance.GetOrSetMemberFunc(name, (int)BindClientAccessors.GetBindGroup);
				return index != -1 ? Groups[index] : null;
			}

			/// <summary>
			/// Looks up the unique ID for a control by its name and returns a <see cref="ControlHandle"/>.
			/// </summary>
			/// <param name="name">The name of the control (e.g., "LeftButton", "W", etc.).</param>
			public static ControlHandle GetControl(string name)
			{
				var index = (int)Instance.GetOrSetMemberFunc(name, (int)BindClientAccessors.GetControlByName);
				return new ControlHandle(index);
			}

			/// <summary>
			/// Retrieves the string name associated with a specific <see cref="ControlHandle"/>.
			/// </summary>
			public static string GetControlName(ControlHandle con)
			{
				return Instance.GetOrSetMemberFunc(con.id, (int)BindClientAccessors.GetControlName) as string;
			}

			/// <summary>
			/// Retrieves the string name associated with a specific control ID.
			/// </summary>
			public static string GetControlName(int conID)
			{
				return Instance.GetOrSetMemberFunc(conID, (int)BindClientAccessors.GetControlName) as string;
			}

			/// <summary>
			/// Batch retrieves the names for a list of control IDs.
			/// </summary>
			public static string[] GetControlNames(IReadOnlyList<int> conIDs)
			{
				return Instance.GetOrSetMemberFunc(conIDs, (int)BindClientAccessors.GetControlNames) as string[];
			}

			/// <summary>
			/// Retrieves the <see cref="IControl"/> object associated with the given <see cref="ControlHandle"/>.
			/// </summary>
			public static IControl GetControl(ControlHandle handle) =>
				Controls[handle.id];

			/// <summary>
			/// Converts a list of <see cref="ControlHandle"/> objects into a list of integer IDs.
			/// </summary>
			/// <param name="controls">Input list of control handles.</param>
			/// <param name="combo">Output list to store integer IDs.</param>
			/// <param name="sanitize">If true, sorts the list and removes duplicates/invalid IDs.</param>
			public static void GetComboIndices(IReadOnlyList<ControlHandle> controls, List<int> combo, bool sanitize = true)
			{
				combo.Clear();

				for (int n = 0; n < controls.Count; n++)
					combo.Add(controls[n].id);

				if (sanitize)
					SanitizeCombo(combo);
			}

			private static IReadOnlyList<int> GetComboIndicesTemp(IReadOnlyList<ControlHandle> controls, bool sanitize = true)
			{
				var buf = _instance.conIDbuf;
				GetComboIndices(controls, buf, sanitize);
				return buf;
			}

			/// <summary>
			/// Helper method that sorts control IDs, removes duplicates, and filters out invalid IDs (zero or less).
			/// </summary>
			private static void SanitizeCombo(List<int> combo)
			{
				combo.Sort();

				for (int i = combo.Count - 1; i > 0; i--)
				{
					if (combo[i] == combo[i - 1] || combo[i] <= 0)
						combo.RemoveAt(i);
				}

				if (combo.Count > 0 && combo[0] == 0)
					combo.RemoveAt(0);
			}
		}
	}
}